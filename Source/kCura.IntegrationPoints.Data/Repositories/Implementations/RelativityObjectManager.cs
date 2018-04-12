using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
		private const int BATCH_SIZE = 1000;
		private IServicesMgr _servicesMgr;
		private readonly IAPILog _logger;
		private int _workspaceArtifactId;
		private readonly ISecretStoreHelper _secretStoreHelper;

		public RelativityObjectManager(int workspaceArtifactId, IHelper helper, ISecretStoreHelper secretStoreHelper)
		{
			_workspaceArtifactId = workspaceArtifactId;
			_servicesMgr = helper.GetServicesManager();
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RelativityObjectManager>();
			_secretStoreHelper = secretStoreHelper;
		}

		public RelativityObjectManager(int workspaceArtifactId, IServicesMgr servicesMgr, IAPILog logger,
			ISecretStoreHelper secretStoreHelper)
		{
			_workspaceArtifactId = workspaceArtifactId;
			_servicesMgr = servicesMgr;
			_logger = logger.ForContext<RelativityObjectManager>();
			_secretStoreHelper = secretStoreHelper;
		}

		public int Create<T>(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			CreateRequest createRequest = new CreateRequest()
			{
				ObjectType = rdo.ToObjectType(),
				FieldValues = rdo.ToFieldValues().ToList()
			};

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
		
		public bool Update(int artifactId, List<FieldRefValuePair> fieldsValues,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
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
				using (var client = _servicesMgr.CreateProxy<IObjectManager>(executionIdentity))
				{
					DeleteRequest request = new DeleteRequest() { Object = rdo.ToObjectRef() };
					var result = client.DeleteAsync(_workspaceArtifactId, request)
						.GetAwaiter()
						.GetResult();
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
				using (var client = _servicesMgr.CreateProxy<IObjectManager>(executionIdentity))
				{
					DeleteRequest request = new DeleteRequest()
					{
						Object = new RelativityObjectRef() { ArtifactID = artifactId }
					};

					var result = client.DeleteAsync(_workspaceArtifactId, request)
						.GetAwaiter()
						.GetResult();
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

				using (var client = _servicesMgr.CreateProxy<IObjectManager>(executionIdentity))
				{
					var queryResults = await client.QueryAsync(_workspaceArtifactId, q, start + 1, length).ConfigureAwait(false);
					return new ResultSet<T>()
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

				using (var client = _servicesMgr.CreateProxy<IObjectManager>(executionIdentity))
				{
					List<T> output = null;
					int retrievedResults = 0;
					int totalResults;

					do
					{
						QueryResult partialResult = await client.QueryAsync(_workspaceArtifactId, q, retrievedResults + 1, BATCH_SIZE).ConfigureAwait(false);

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
				using (var client = _servicesMgr.CreateProxy<IObjectManager>(executionIdentity))
				{
					List<RelativityObject> output = null;
					int retrievedResults = 0;
					int totalResults;

					do
					{
						QueryResult partialResult = await client.QueryAsync(_workspaceArtifactId, q, retrievedResults + 1, BATCH_SIZE).ConfigureAwait(false);

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
				using (var client = _servicesMgr.CreateProxy<IObjectManager>(executionIdentity))
				{
					var result = await client.QueryAsync(_workspaceArtifactId, q, start + 1, length).ConfigureAwait(false);

					return new ResultSet<RelativityObject>()
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
				using (var client = _servicesMgr.CreateProxy<IObjectManager>(executionIdentity))
				{
					var result = await client.QueryAsync(_workspaceArtifactId, q, 1, 1).ConfigureAwait(false);
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
				using (var client = _servicesMgr.CreateProxy<IObjectManager>(executionIdentity))
				{
					_secretStoreHelper.SetEncryptedSecuredConfigurationForNewRdo(createRequest.FieldValues);

					int artifactId = client.CreateAsync(_workspaceArtifactId, createRequest)
						.GetAwaiter()
						.GetResult()
						.Object
						.ArtifactID;
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
				using (var client = _servicesMgr.CreateProxy<IObjectManager>(executionIdentity))
				{
					ReadResult result = client.ReadAsync(_workspaceArtifactId, request)
						.GetAwaiter()
						.GetResult();
					var rdo = result.Object.ToRDO<T>();
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
				using (var client = _servicesMgr.CreateProxy<IObjectManager>(executionIdentity))
				{
					var result = client.UpdateAsync(_workspaceArtifactId, request)
						.GetAwaiter()
						.GetResult();
					return !result.EventHandlerStatuses.Any(x => !x.Success);
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
				var existingRdo = Read<IntegrationPoint>(rdo.ArtifactId, false, executionIdentity);
				_secretStoreHelper.SetEncryptedSecuredConfigurationForExistingRdo(existingRdo, request.FieldValues);
			}
		}

		private T SetDecryptedSecuredConfiguration<T>(T rdo) where T : BaseRdo, new()
		{
			if (rdo is IntegrationPoint && rdo.HasField(new Guid(IntegrationPointFieldGuids.SecuredConfiguration)))
			{
				var secretId = rdo.GetField<string>(new Guid(IntegrationPointFieldGuids.SecuredConfiguration));
				if (!String.IsNullOrWhiteSpace(secretId))
				{
					var decryptedSecret = _secretStoreHelper.DecryptSecuredConfiguration(secretId);
					if (!String.IsNullOrWhiteSpace(decryptedSecret))
					{
						rdo.SetField<string>(new Guid(IntegrationPointFieldGuids.SecuredConfiguration), decryptedSecret);
						return rdo;
					}
				}
			}
			return rdo;
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
			var fieldsAsString = queryRequest?.Fields?.Select(x => $"({x.Name}: {x.Guid})");
			return string.Join(", ", fieldsAsString);
		}

		private string GetRdoType(BaseRdo rdo)
		{
			return rdo?.GetType().Name ?? "[UnknownObjectType]";
		}
	}
}
