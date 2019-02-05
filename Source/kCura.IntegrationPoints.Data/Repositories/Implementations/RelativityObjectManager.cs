using kCura.IntegrationPoints.Data.Facades;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Data.UtilityDTO;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FieldRef = Relativity.Services.Objects.DataContracts.FieldRef;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;
using RelativityObjectRef = Relativity.Services.Objects.DataContracts.RelativityObjectRef;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RelativityObjectManager : IRelativityObjectManager
	{
		private const int _BATCH_SIZE = 1000;

		private readonly int _workspaceArtifactId;
		private readonly IAPILog _logger;
		private readonly ISecretStoreHelper _secretStoreHelper;
		private readonly IObjectManagerFacadeFactory _objectManagerFacadeFactory;

		internal RelativityObjectManager(
			int workspaceArtifactId,
			IAPILog logger,
			ISecretStoreHelper secretStoreHelper,
			IObjectManagerFacadeFactory objectManagerFacadeFactory)
		{
			_workspaceArtifactId = workspaceArtifactId;
			_logger = logger.ForContext<RelativityObjectManager>();
			_secretStoreHelper = secretStoreHelper;
			_objectManagerFacadeFactory = objectManagerFacadeFactory;
		}

		public int Create<T>(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) 
			where T : BaseRdo, new()
		{
			var createRequest = new CreateRequest
			{
				ObjectType = rdo.ToObjectType(),
				FieldValues = rdo.ToFieldValues().ToList()
			};
			SetParentArtifactId(createRequest, rdo);

			return SendCreateRequest(createRequest, executionIdentity);
		}

		public int Create(ObjectTypeRef objectType,
			List<FieldRefValuePair> fieldValues, 
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			var createRequest = new CreateRequest
			{
				ObjectType = objectType,
				FieldValues = fieldValues
			};
			return SendCreateRequest(createRequest, executionIdentity);
		}

		public int Create(ObjectTypeRef objectType, 
			RelativityObjectRef parentObject, 
			List<FieldRefValuePair> fieldValues, 
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			var createRequest = new CreateRequest
			{
				ObjectType = objectType,
				ParentObject = parentObject,
				FieldValues = fieldValues
			};
			return SendCreateRequest(createRequest, executionIdentity);
		}

		public T Read<T>(int artifactId, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) 
			where T : BaseRdo, new()
		{
			var request = new ReadRequest
			{
				Object = new RelativityObjectRef { ArtifactID = artifactId },
				Fields = new T().ToFieldList()
			};
			return SendReadRequest<T>(request,
				decryptSecuredConfiguration: true,
				executionIdentity: executionIdentity);
		}

		public T Read<T>(int artifactId, 
			IEnumerable<Guid> fieldsGuids, 
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) 
			where T : BaseRdo, new()
		{
			var request = new ReadRequest
			{
				Object = new RelativityObjectRef { ArtifactID = artifactId },
				Fields = fieldsGuids.Select(x => new FieldRef { Guid = x }).ToArray()
			};
			return SendReadRequest<T>(request,
				decryptSecuredConfiguration: true,
				executionIdentity: executionIdentity);
		}

		public bool Update(int artifactId, 
			IList<FieldRefValuePair> fieldsValues, 
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			var request = new UpdateRequest
			{
				Object = new RelativityObjectRef { ArtifactID = artifactId },
				FieldValues = fieldsValues
			};

			string rdoType = GetRdoType(rdo: null);
			return SendUpdateRequest(request, executionIdentity, rdoType);
		}

		public bool Update<T>(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) 
			where T : BaseRdo, new()
		{
			var request = new UpdateRequest
			{
				Object = rdo.ToObjectRef(),
				FieldValues = rdo.ToFieldValues().ToList()
			};
			SetEncryptedConfigurationForUpdate(rdo, executionIdentity, request);

			return SendUpdateRequest(request, executionIdentity, GetRdoType(rdo));
		}

		public bool Delete<T>(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) 
			where T : BaseRdo, new()
		{
			var request = new DeleteRequest
			{
				Object = rdo.ToObjectRef()
			};
			return SendDeleteRequest(request, executionIdentity, GetRdoType(rdo));
		}

		public bool Delete(int artifactId, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			var request = new DeleteRequest
			{
				Object = new RelativityObjectRef { ArtifactID = artifactId }
			};
			return SendDeleteRequest(request, executionIdentity, rdoType: null);
		}

		public ResultSet<T> Query<T>(QueryRequest q, 
			int start, 
			int length, 
			bool noFields = false,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) 
			where T : BaseRdo, new()
		{
			return QueryAsync<T>(q, start, length, noFields, executionIdentity)
				.GetAwaiter()
				.GetResult();
		}

		public Task<ResultSet<T>> QueryAsync<T>(QueryRequest q, 
			int start, 
			int length, 
			bool noFields = false,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) 
			where T : BaseRdo, new()
		{
			Func<IObjectManagerFacade, Task<ResultSet<T>>> func = async (client) =>
			{
				QueryResult queryResults = await client.QueryAsync(_workspaceArtifactId, q, start + 1, length)
					.ConfigureAwait(false);
				return new ResultSet<T>
				{
					ResultCount = queryResults.ResultCount,
					TotalCount = queryResults.TotalCount,
					Items = queryResults.Objects
						.Select(x => x.ToRDO<T>())
						.Select(SetDecryptedSecuredConfiguration)
						.ToList()
				};
			};

			T rdo = new T();
			BootstrapQuery(q, rdo, noFields);

			return SendQueryRequestAsync(func, q, rdo, executionIdentity);
		}

		public List<T> Query<T>(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			return QueryAsync<T>(q, noFields: false, executionIdentity: executionIdentity)
				.GetAwaiter()
				.GetResult();
		}

		public Task<List<T>> QueryAsync<T>(QueryRequest q, 
			bool noFields = false, 
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) 
			where T : BaseRdo, new()
		{
			Func<IObjectManagerFacade, Task<List<T>>> func = async (client) =>
			{
				List<T> output = null;
				int retrievedResults = 0;
				int totalResults;

				do
				{
					QueryResult partialResult = await client
						.QueryAsync(_workspaceArtifactId, q, retrievedResults + 1, _BATCH_SIZE)
						.ConfigureAwait(false);

					totalResults = partialResult.TotalCount;
					if (output == null)
					{
						output = new List<T>(totalResults);
					}

					IEnumerable<T> partialResultsAsRdo = partialResult.Objects.Select(x => x.ToRDO<T>())
						.Select(SetDecryptedSecuredConfiguration);
					output.AddRange(partialResultsAsRdo);

					retrievedResults += partialResult.Objects.Count;
				} while (retrievedResults < totalResults);

				return output;
			};

			T rdo = new T();
			BootstrapQuery(q, rdo, noFields);

			return SendQueryRequestAsync(func, q, rdo, executionIdentity);
		}

		/// <summary>
		/// This method should not be used to query Rdo
		/// </summary>
		/// <param name="q"></param>
		/// <param name="executionIdentity"></param>
		/// <returns></returns>
		public List<RelativityObject> Query(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			return QueryAsync(q, executionIdentity).GetAwaiter().GetResult();
		}

		/// <summary>
		/// This method should not be used to query Rdo
		/// </summary>
		/// <param name="q"></param>
		/// <param name="executionIdentity"></param>
		/// <returns></returns>
		public Task<List<RelativityObject>> QueryAsync(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			Func<IObjectManagerFacade, Task<List<RelativityObject>>> func = async (client) =>
			{
				List<RelativityObject> output = null;
				int retrievedResults = 0;
				int totalResults;

				do
				{
					QueryResult partialResult = await client
						.QueryAsync(_workspaceArtifactId, q, retrievedResults + 1, _BATCH_SIZE)
						.ConfigureAwait(false);

					totalResults = partialResult.TotalCount;
					if (output == null)
					{
						output = new List<RelativityObject>(totalResults);
					}

					output.AddRange(partialResult.Objects);

					retrievedResults += partialResult.Objects.Count;
				} while (retrievedResults < totalResults);

				return output;
			};

			return SendQueryRequestAsync(
				func, 
				q, 
				rdo: null, 
				executionIdentity: executionIdentity);
		}

		public ResultSet<RelativityObject> Query(QueryRequest q, int start, int length, bool noFields = false,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			return QueryAsync(q, start, length, noFields, executionIdentity)
				.GetAwaiter()
				.GetResult();
		}

		public Task<ResultSet<RelativityObject>> QueryAsync(QueryRequest q, 
			int start, 
			int length, 
			bool noFields = false,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			Func<IObjectManagerFacade, Task<ResultSet<RelativityObject>>> func = async (client) =>
			{
				QueryResult queryResult = await client.QueryAsync(_workspaceArtifactId, q, start + 1, length)
					.ConfigureAwait(false);

				return new ResultSet<RelativityObject>
				{
					ResultCount = queryResult.ResultCount,
					TotalCount = queryResult.TotalCount,
					Items = queryResult.Objects.ToList()
				};
			};

			return SendQueryRequestAsync(
				func, 
				q, 
				rdo: null, 
				executionIdentity: executionIdentity);
		}

		public int QueryTotalCount(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			return QueryTotalCountAsync(q, executionIdentity).GetAwaiter().GetResult();
		}

		public Task<int> QueryTotalCountAsync(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			Func<IObjectManagerFacade, Task<int>> func = async (client) =>
			{
				QueryResult queryResult = await client.QueryAsync(_workspaceArtifactId, q, 1, 1)
					.ConfigureAwait(false);
				return queryResult.TotalCount;
			};

			return SendQueryRequestAsync(
				func,
				q,
				rdo: null,
				executionIdentity: executionIdentity);
		}

		public Task<System.IO.Stream> StreamLongTextAsync(
			int workspaceArtifactID,
			RelativityObjectRef exportObject, 
			FieldRef longTextField,
			ExecutionIdentity executionIdentity)
		{
			return null;
		}

		private async Task<T> SendQueryRequestAsync<T>(
			Func<IObjectManagerFacade, Task<T>> queryAction,
			QueryRequest q,
			BaseRdo rdo,
			ExecutionIdentity executionIdentity)
		{
			try
			{
				using (IObjectManagerFacade client = _objectManagerFacadeFactory.Create(executionIdentity))
				{
					return await queryAction(client).ConfigureAwait(false);
				}
			}
			catch (IntegrationPointsException)
			{
				throw;
			}
			catch (Exception ex)
			{
				HandleObjectManagerException(ex, message: GetQueryErrorMessage(q, GetRdoType(rdo)));
				throw;
			}
		}

		private T SendReadRequest<T>(ReadRequest request, 
			bool decryptSecuredConfiguration, 
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) 
			where T : BaseRdo, new()
		{
			try
			{
				using (IObjectManagerFacade client = _objectManagerFacadeFactory.Create(executionIdentity))
				{
					ReadResult result = client.ReadAsync(_workspaceArtifactId, request)
						.GetAwaiter()
						.GetResult();
					T rdo = result.Object.ToRDO<T>();
					return decryptSecuredConfiguration 
						? SetDecryptedSecuredConfiguration(rdo) 
						: rdo;
				}
			}
			catch (IntegrationPointsException)
			{
				throw;
			}
			catch (Exception ex)
			{
				string rdoType = GetRdoType(new T());
				HandleObjectManagerException(ex, message: GetErrorMessage<ReadRequest>(rdoType));
				throw;
			}
		}

		private int SendCreateRequest(CreateRequest request, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			try
			{
				using (IObjectManagerFacade client = _objectManagerFacadeFactory.Create(executionIdentity))
				{
					_secretStoreHelper.SetEncryptedSecuredConfigurationForNewRdo(request.FieldValues);

					int artifactId = client.CreateAsync(_workspaceArtifactId, request)
						.GetAwaiter()
						.GetResult()
						.Object
						.ArtifactID;
					return artifactId;
				}
			}
			catch (IntegrationPointsException)
			{
				throw;
			}
			catch (Exception ex)
			{
				HandleObjectManagerException(ex, message: GetErrorMessage<CreateRequest>("[RelativityObject]"));
				throw;
			}
		}

		private bool SendUpdateRequest(UpdateRequest request, ExecutionIdentity executionIdentity, string rdoType)
		{
			try
			{
				using (IObjectManagerFacade client = _objectManagerFacadeFactory.Create(executionIdentity))
				{
					UpdateResult result = client.UpdateAsync(_workspaceArtifactId, request)
						.GetAwaiter()
						.GetResult();
					return result.EventHandlerStatuses.All(x => x.Success);
				}
			}
			catch (IntegrationPointsException)
			{
				throw;
			}
			catch (Exception ex)
			{
				HandleObjectManagerException(ex, message: GetErrorMessage<UpdateRequest>(rdoType));
				throw;
			}
		}

		private bool SendDeleteRequest(DeleteRequest request, ExecutionIdentity executionIdentity, string rdoType)
		{
			try
			{
				using (IObjectManagerFacade client = _objectManagerFacadeFactory.Create(executionIdentity))
				{
					DeleteResult result = client.DeleteAsync(_workspaceArtifactId, request)
						.GetAwaiter()
						.GetResult();
					return result.Report.DeletedItems.Any();
				}
			}
			catch (IntegrationPointsException)
			{
				throw;
			}
			catch (Exception ex)
			{
				HandleObjectManagerException(ex, message: GetErrorMessage<DeleteRequest>(rdoType));
				throw;
			}
		}

		private static void BootstrapQuery(QueryRequest q, BaseRdo rdo, bool noFields)
		{
			if (rdo == null)
			{
				return;
			}

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

		private T Read<T>(int artifactId, bool decryptSecuredConfiguration, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			var request = new ReadRequest
			{
				Object = new RelativityObjectRef { ArtifactID = artifactId },
				Fields = new T().ToFieldList()
			};
			return SendReadRequest<T>(request, decryptSecuredConfiguration, executionIdentity);
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

		private void HandleObjectManagerException(Exception ex, string message)
		{
			_logger.LogError(ex, message);
			throw new IntegrationPointsException(message, ex)
			{
				ShouldAddToErrorsTab = true,
				ExceptionSource = IntegrationPointsExceptionSource.KEPLER
			};
		}

		private string GetOperationNameForRequestType<T>()
		{
			return typeof(T).Name.Replace("Request", string.Empty).ToUpper();
		}

		private string GetErrorMessage<T>(string rdoType)
		{
			string operationName = GetOperationNameForRequestType<T>();
			return $"Cannot {operationName} object of type {rdoType} with ObjectManager (Workspace: {_workspaceArtifactId})";
		}

		private string GetQueryErrorMessage(QueryRequest q, string rdoType)
		{
			string queryCondition = q?.Condition;
			string queryObjectType = $"({q?.ObjectType?.Name}: {q?.ObjectType?.Guid})";
			string fields = ConvertFieldsToStringRepresentation(q);

			return $"Cannot QUERY object of type {rdoType} with ObjectManager (Workspace: {_workspaceArtifactId}). Condition: {queryCondition}, Fields: {fields}, ObjectType: {queryObjectType}";
		}

		private string ConvertFieldsToStringRepresentation(QueryRequest queryRequest)
		{
			IEnumerable<string> fieldsAsString = queryRequest?.Fields?.Select(x => $"({x.Name}: {x.Guid})");
			return fieldsAsString != null 
				? string.Join(", ", fieldsAsString) 
				: string.Empty;
		}

		private string GetRdoType(IBaseRdo rdo)
		{
			return rdo?.GetType().Name ?? "[UnknownObjectType]";
		}
	}
}
