﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Facades;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Data.UtilityDTO;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;
using Relativity.Kepler.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using FieldRef = Relativity.Services.Objects.DataContracts.FieldRef;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;
using RelativityObjectRef = Relativity.Services.Objects.DataContracts.RelativityObjectRef;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RelativityObjectManager : IRelativityObjectManager
	{
		private const int _BATCH_SIZE = 1000;

		private readonly IRetryHandler _retryHandler;
		private readonly IServicesMgr _servicesMgr;
		private readonly int _workspaceArtifactId;
		private readonly IAPILog _logger;
		private readonly ISecretStoreHelper _secretStoreHelper;

		public RelativityObjectManager(
			int workspaceArtifactId,
			IHelper helper,
			ISecretStoreHelper secretStoreHelper)
		: this(
			workspaceArtifactId,
			helper.GetServicesManager(),
			helper.GetLoggerFactory().GetLogger(),
			secretStoreHelper)
		{ }

		public RelativityObjectManager(
			int workspaceArtifactId,
			IServicesMgr servicesMgr,
			IAPILog logger,
			ISecretStoreHelper secretStoreHelper)
		{
			_workspaceArtifactId = workspaceArtifactId;
			_servicesMgr = servicesMgr;
			_logger = logger.ForContext<RelativityObjectManager>();
			_secretStoreHelper = secretStoreHelper;

			try
			{
				_retryHandler = new RetryHandler(_logger);
			}
			catch (FileNotFoundException ex) // TODO this is hack for fixing REL-285938
			{
				_logger.LogWarning(ex, $"Exception occured during {nameof(RetryHandler)} instantiation. {nameof(NonRetryHandler)} will be used");
				_retryHandler = new NonRetryHandler();
			}
		}

		public int Create<T>(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			CreateRequest createRequest = new CreateRequest()
			{
				ObjectType = rdo.ToObjectType(),
				FieldValues = rdo.ToFieldValues().ToList()
			};
			SetParentArtifactId(createRequest, rdo);

			return Create(createRequest, executionIdentity);
		}

		public int Create(ObjectTypeRef objectType, List<FieldRefValuePair> fieldValues, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			CreateRequest createRequest = new CreateRequest
			{
				ObjectType = objectType,
				FieldValues = fieldValues
			};
			return Create(createRequest, executionIdentity);
		}

		public int Create(ObjectTypeRef objectType, RelativityObjectRef parentObject, List<FieldRefValuePair> fieldValues, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			CreateRequest createRequest = new CreateRequest
			{
				ObjectType = objectType,
				ParentObject = parentObject,
				FieldValues = fieldValues
			};
			return Create(createRequest, executionIdentity);

		}

		public T Read<T>(int artifactId, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			ReadRequest request = new ReadRequest()
			{
				Object = new RelativityObjectRef { ArtifactID = artifactId },
				Fields = new T().ToFieldList()
			};
			return SendReadRequest<T>(request, true, executionIdentity);
		}

		public T Read<T>(int artifactId, IEnumerable<Guid> fieldsGuids, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			var request = new ReadRequest()
			{
				Object = new RelativityObjectRef { ArtifactID = artifactId },
				Fields = fieldsGuids.Select(x => new FieldRef { Guid = x }).ToArray()
			};
			return SendReadRequest<T>(request, true, executionIdentity);
		}

		public bool Update(int artifactId, IList<FieldRefValuePair> fieldsValues, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			var request = new UpdateRequest
			{
				Object = new RelativityObjectRef { ArtifactID = artifactId },
				FieldValues = fieldsValues
			};

			string rdoType = GetRdoType(null);
			return SendUpdateRequest(executionIdentity, request, rdoType);
		}

		public bool Update<T>(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			var request = new UpdateRequest
			{
				Object = rdo.ToObjectRef(),
				FieldValues = rdo.ToFieldValues().ToList()
			};
			SetEncryptedConfigurationForUpdate(rdo, executionIdentity, request);

			string rdoType = GetRdoType(rdo);
			return SendUpdateRequest(executionIdentity, request, rdoType);
		}

		public bool Delete<T>(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			try
			{
				using (var client = CreateObjectManagerFacade(executionIdentity))
				{
					var request = new DeleteRequest { Object = rdo.ToObjectRef() };

					// ReSharper disable AccessToDisposedClosure
					DeleteResult result = _retryHandler.ExecuteWithRetries(
						() => client.DeleteAsync(_workspaceArtifactId, request));
					// ReSharper enable AccessToDisposedClosure

					return result.Report.DeletedItems.Any();
				}
			}
			catch (ServiceNotFoundException ex)
			{
				throw LogServiceNotFoundException("DELETE", ex);
			}
			catch (Exception ex)
			{
				throw LogObjectManagerException(rdo, "delete", ex);
			}
		}

		public bool Delete(int artifactId, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			try
			{
				using (var client = CreateObjectManagerFacade(executionIdentity))
				{
					DeleteRequest request = new DeleteRequest()
					{
						Object = new RelativityObjectRef() { ArtifactID = artifactId }
					};

					// ReSharper disable AccessToDisposedClosure
					DeleteResult result = _retryHandler.ExecuteWithRetries(
						() => client.DeleteAsync(_workspaceArtifactId, request));
					// ReSharper enable AccessToDisposedClosure
					return result.Report.DeletedItems.Any();
				}
			}
			catch (ServiceNotFoundException ex)
			{
				throw LogServiceNotFoundException("DELETE", ex);
			}
			catch (Exception ex)
			{
				throw LogObjectManagerException("delete", null, ex);
			}
		}


		public ResultSet<T> Query<T>(QueryRequest q, int start, int length, bool noFields = false,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			return QueryAsync<T>(q, start, length, noFields, executionIdentity).GetAwaiter().GetResult();
		}

		public async Task<ResultSet<T>> QueryAsync<T>(QueryRequest q, int start, int length, bool noFields = false,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			try
			{
				BootstrapQuery<T>(q, noFields);

				using (var client = CreateObjectManagerFacade(executionIdentity))
				{
					// ReSharper disable AccessToDisposedClosure
					QueryResult queryResults = await _retryHandler.ExecuteWithRetriesAsync(
						() => client.QueryAsync(_workspaceArtifactId, q, start + 1, length)
						).ConfigureAwait(false);
					// ReSharper enable AccessToDisposedClosure
					return new ResultSet<T>
					{
						ResultCount = queryResults.ResultCount,
						TotalCount = queryResults.TotalCount,
						Items = queryResults.Objects.Select(x => x.ToRDO<T>()).Select(SetDecryptedSecuredConfiguration).ToList()
					};

				}
			}
			catch (ServiceNotFoundException ex)
			{
				throw LogServiceNotFoundException("QUERY", ex);
			}
			catch (Exception ex)
			{
				throw LogObjectManagerException(new T(), q, ex);
			}
		}

		public List<T> Query<T>(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			return QueryAsync<T>(q, false, executionIdentity).GetAwaiter().GetResult();
		}

		public async Task<List<T>> QueryAsync<T>(QueryRequest q, bool noFields = false, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			try
			{
				BootstrapQuery<T>(q, noFields);

				using (var client = CreateObjectManagerFacade(executionIdentity))
				{
					List<T> output = null;
					int retrievedResults = 0;
					int totalResults;

					do
					{
						// ReSharper disable AccessToDisposedClosure
						QueryResult partialResult = await _retryHandler.ExecuteWithRetriesAsync(
							() => client.QueryAsync(_workspaceArtifactId, q, retrievedResults + 1, _BATCH_SIZE)
							).ConfigureAwait(false);
						// ReSharper enable AccessToDisposedClosure

						totalResults = partialResult.TotalCount;
						if (output == null)
						{
							output = new List<T>(totalResults);
						}

						IEnumerable<T> partialResultsAsRdo = partialResult.Objects.Select(x => x.ToRDO<T>()).Select(SetDecryptedSecuredConfiguration);
						output.AddRange(partialResultsAsRdo);

						retrievedResults += partialResult.Objects.Count;
					} while (retrievedResults < totalResults);

					return output;
				}
			}
			catch (ServiceNotFoundException ex)
			{
				throw LogServiceNotFoundException("QUERY", ex);
			}
			catch (Exception ex)
			{
				throw LogObjectManagerException(new T(), q, ex);
			}
		}

		/// <summary>
		/// This method should not ne used to query Rdo
		/// </summary>
		/// <param name="q"></param>
		/// <param name="executionIdentity"></param>
		/// <returns></returns>
		public List<RelativityObject> Query(QueryRequest q,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			return QueryAsync(q, executionIdentity).GetAwaiter().GetResult();
		}

		/// <summary>
		/// This method should not ne used to query Rdo
		/// </summary>
		/// <param name="q"></param>
		/// <param name="executionIdentity"></param>
		/// <returns></returns>
		public async Task<List<RelativityObject>> QueryAsync(QueryRequest q,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			try
			{
				using (var client = CreateObjectManagerFacade(executionIdentity))
				{
					List<RelativityObject> output = null;
					int retrievedResults = 0;
					int totalResults;

					do
					{
						// ReSharper disable AccessToDisposedClosure
						QueryResult partialResult = await _retryHandler.ExecuteWithRetriesAsync(
							() => client.QueryAsync(_workspaceArtifactId, q, retrievedResults + 1, _BATCH_SIZE)
							).ConfigureAwait(false);
						// ReSharper enable AccessToDisposedClosure

						totalResults = partialResult.TotalCount;
						if (output == null)
						{
							output = new List<RelativityObject>(totalResults);
						}

						output.AddRange(partialResult.Objects);

						retrievedResults += partialResult.Objects.Count;
					} while (retrievedResults < totalResults);

					return output;
				}
			}
			catch (ServiceNotFoundException ex)
			{
				throw LogServiceNotFoundException("QUERY", ex);
			}
			catch (Exception ex)
			{
				throw LogObjectManagerException(null, q, ex);
			}
		}

		public ResultSet<RelativityObject> Query(QueryRequest q, int start, int length, bool noFields = false,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			return QueryAsync(q, start, length, noFields, executionIdentity).GetAwaiter().GetResult();
		}

		public async Task<ResultSet<RelativityObject>> QueryAsync(QueryRequest q, int start, int length, bool noFields = false,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			try
			{
				using (var client = CreateObjectManagerFacade(executionIdentity))
				{
					// ReSharper disable AccessToDisposedClosure
					QueryResult result = await _retryHandler.ExecuteWithRetriesAsync(
						() => client.QueryAsync(_workspaceArtifactId, q, start + 1, length)
						).ConfigureAwait(false);
					// ReSharper enable AccessToDisposedClosure

					return new ResultSet<RelativityObject>
					{
						ResultCount = result.ResultCount,
						TotalCount = result.TotalCount,
						Items = result.Objects.ToList()
					};
				}
			}
			catch (Exception ex)
			{
				throw LogObjectManagerException(null, q, ex);
			}
		}

		public int QueryTotalCount(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			return QueryTotalCountAsync(q, executionIdentity).GetAwaiter().GetResult();
		}

		public async Task<int> QueryTotalCountAsync(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			try
			{
				using (var client = CreateObjectManagerFacade(executionIdentity))
				{
					// ReSharper disable AccessToDisposedClosure
					QueryResult result = await _retryHandler.ExecuteWithRetriesAsync(
						() => client.QueryAsync(_workspaceArtifactId, q, 1, 1)
						).ConfigureAwait(false);
					// ReSharper enable AccessToDisposedClosure
					return result.TotalCount;
				}
			}
			catch (ServiceNotFoundException ex)
			{
				throw LogServiceNotFoundException("QUERY_TOTAL_COUNT", ex);
			}
			catch (Exception ex)
			{
				throw LogObjectManagerException(null, q, ex);
			}
		}

		private static void BootstrapQuery<T>(QueryRequest q, bool noFields) where T : BaseRdo, new()
		{
			T rdo = new T();
			q.ObjectType = rdo.ToObjectType();
			if (noFields)
			{
				if (q.Fields.Any())
				{
					throw new IntegrationPointsException("Fields list not empty while trying to execute query with noFields.");
				}
				q.Fields = new FieldRef[0];
			}
			else if (!q.Fields.Any())
			{
				q.Fields = rdo.ToFieldList();
			}
		}

		private int Create(CreateRequest createRequest, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			try
			{
				using (var client = CreateObjectManagerFacade(executionIdentity))
				{
					_secretStoreHelper.SetEncryptedSecuredConfigurationForNewRdo(createRequest.FieldValues);

					// ReSharper disable AccessToDisposedClosure
					CreateResult createResult = _retryHandler.ExecuteWithRetries(
						() => client.CreateAsync(_workspaceArtifactId, createRequest));
					// ReSharper enable AccessToDisposedClosure
					int artifactId = createResult.Object.ArtifactID;
					return artifactId;
				}
			}
			catch (ServiceNotFoundException ex)
			{
				throw LogServiceNotFoundException("CREATE", ex);
			}
			catch (Exception ex)
			{
				throw LogObjectManagerException("create", "[RelativityObject]", ex);
			}
		}

		private T Read<T>(int artifactId, bool decryptSecuredConfiguration, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			ReadRequest request = new ReadRequest()
			{
				Object = new RelativityObjectRef { ArtifactID = artifactId },
				Fields = new T().ToFieldList()
			};
			return SendReadRequest<T>(request, decryptSecuredConfiguration, executionIdentity);
		}

		private T SendReadRequest<T>(ReadRequest request, bool decryptSecuredConfiguration, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			try
			{
				using (var client = CreateObjectManagerFacade(executionIdentity))
				{
					// ReSharper disable AccessToDisposedClosure
					ReadResult result = _retryHandler.ExecuteWithRetries(
						() => client.ReadAsync(_workspaceArtifactId, request));
					// ReSharper restore AccessToDisposedClosure
					T rdo = result.Object.ToRDO<T>();
					return decryptSecuredConfiguration ? SetDecryptedSecuredConfiguration(rdo) : rdo;
				}
			}
			catch (ServiceNotFoundException ex)
			{
				throw LogServiceNotFoundException("READ", ex);
			}
			catch (Exception ex)
			{
				throw LogObjectManagerException(new T(), "read", ex);
			}
		}

		private bool SendUpdateRequest(ExecutionIdentity executionIdentity, UpdateRequest request, string rdoType)
		{
			try
			{
				using (var client = CreateObjectManagerFacade(executionIdentity))
				{
					// ReSharper disable AccessToDisposedClosure
					UpdateResult result = _retryHandler.ExecuteWithRetries(
						() => client.UpdateAsync(_workspaceArtifactId, request));
					// ReSharper restore AccessToDisposedClosure

					return result.EventHandlerStatuses.All(x => x.Success);
				}
			}
			catch (ServiceNotFoundException ex)
			{
				throw LogServiceNotFoundException("UPDATE", ex);
			}
			catch (Exception ex)
			{
				throw LogObjectManagerException("UPDATE", rdoType, ex);
			}
		}

		private void SetEncryptedConfigurationForUpdate<T>(T rdo, ExecutionIdentity executionIdentity, UpdateRequest request) where T : BaseRdo, new()
		{
			if (rdo is IntegrationPoint)
			{
				IntegrationPoint existingRdo = Read<IntegrationPoint>(rdo.ArtifactId, false, executionIdentity);
				_secretStoreHelper.SetEncryptedSecuredConfigurationForExistingRdo(existingRdo, request.FieldValues);
			}
		}

		private T SetDecryptedSecuredConfiguration<T>(T rdo) where T : BaseRdo, new()
		{
			if (rdo is IntegrationPoint && rdo.HasField(new Guid(IntegrationPointFieldGuids.SecuredConfiguration)))
			{
				string secretId = rdo.GetField<string>(new Guid(IntegrationPointFieldGuids.SecuredConfiguration));
				if (!string.IsNullOrWhiteSpace(secretId))
				{
					string decryptedSecret = _secretStoreHelper.DecryptSecuredConfiguration(secretId);
					if (!string.IsNullOrWhiteSpace(decryptedSecret))
					{
						rdo.SetField<string>(new Guid(IntegrationPointFieldGuids.SecuredConfiguration), decryptedSecret);
						return rdo;
					}
				}
			}
			return rdo;
		}

		private void SetParentArtifactId<T>(CreateRequest request, T rdo) where T : BaseRdo, new()
		{
			if (rdo.ParentArtifactId.HasValue)
			{
				request.ParentObject = new RelativityObjectRef
				{
					ArtifactID = rdo.ParentArtifactId.Value
				};
			}
		}

		private ObjectManagerFacadeWithRetries CreateObjectManagerFacade(ExecutionIdentity executionIdentity)
		{
			IObjectManager objectManager = _servicesMgr.CreateProxy<IObjectManager>(executionIdentity);
			return new ObjectManagerFacadeWithRetries(objectManager, _logger);
		}

		private IntegrationPointsException LogServiceNotFoundException(string operationName, ServiceNotFoundException ex)
		{
			string message = $"Error while connecting to object manager service. Cannot perform {operationName} operation.";
			_logger.LogError("Error while connecting to object manager service. Cannot perform {operationName} operation.", operationName);
			return new IntegrationPointsException(message, ex)
			{
				ShouldAddToErrorsTab = true,
				Source = IntegrationPointsExceptionSource.KEPLER
			};
		}

		private IntegrationPointsException LogObjectManagerException(string operationName, string typeName, Exception ex)
		{
			string message = $"Cannot {operationName} object of type {typeName} with ObjectManager (Workspace: {_workspaceArtifactId})";
			_logger.LogError(ex, "Cannot {operationName} object of type {rdoType} with ObjectManager (Workspace: {_workspaceArtifactId})", operationName, typeName, _workspaceArtifactId);
			return new IntegrationPointsException(message, ex)
			{
				ShouldAddToErrorsTab = true,
				ExceptionSource = IntegrationPointsExceptionSource.KEPLER
			};
		}

		private IntegrationPointsException LogObjectManagerException(BaseRdo rdo, string operationName, Exception ex)
		{
			string rdoType = GetRdoType(rdo);
			return LogObjectManagerException(operationName, rdoType, ex);
		}

		private IntegrationPointsException LogObjectManagerException(BaseRdo rdo, QueryRequest q, Exception ex)
		{
			string rdoType = rdo?.GetType().Name ?? "[UnknownObjectType]";
			string queryCondition = q?.Condition;
			string queryObjectType = $"({q?.ObjectType?.Name}: {q?.ObjectType?.Guid})";
			string fields = ConvertFieldsToStringRepresentation(q);

			string message = $"Cannot QUERY object of type {rdoType} with ObjectManager (Workspace: {_workspaceArtifactId}). Condition: {queryCondition}, Fields: {fields}, ObjectType: {queryObjectType}";
			_logger.LogError(ex, "Cannot QUERY object of type {rdoType} with ObjectManager (Workspace: {_workspaceArtifactId}). Condition: {queryCondition}, Fields: {fields}, ObjectType: {queryObjectType}",
				rdoType, _workspaceArtifactId, queryCondition, fields, queryObjectType);
			return new IntegrationPointsException(message, ex)
			{
				ShouldAddToErrorsTab = true,
				ExceptionSource = IntegrationPointsExceptionSource.KEPLER
			};
		}

		private string ConvertFieldsToStringRepresentation(QueryRequest queryRequest)
		{
			IEnumerable<string> fieldsAsString = queryRequest?.Fields?.Select(x => $"({x.Name}: {x.Guid})");
			return fieldsAsString != null ? string.Join(", ", fieldsAsString) : string.Empty;
		}

		private string GetRdoType(BaseRdo rdo)
		{
			return rdo?.GetType().Name ?? "[UnknownObjectType]";
		}
	}
}
