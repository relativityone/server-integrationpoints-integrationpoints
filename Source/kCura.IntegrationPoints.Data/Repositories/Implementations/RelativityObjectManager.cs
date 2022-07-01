using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Data.UtilityDTO;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Facades.ObjectManager;
using kCura.IntegrationPoints.Data.StreamWrappers;
using Relativity.Kepler.Transport;
using Relativity.Services.DataContracts.DTOs.Results;
using FieldRef = Relativity.Services.Objects.DataContracts.FieldRef;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;
using RelativityObjectRef = Relativity.Services.Objects.DataContracts.RelativityObjectRef;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RelativityObjectManager : IRelativityObjectManager
	{
		private const int _BATCH_SIZE = 1000;
		private const string _UNKNOWN_OBJECT_TYPE = "[UnknownObjectType]";
		private readonly int _workspaceArtifactId;
		private readonly IAPILog _logger;
		private readonly IObjectManagerFacadeFactory _objectManagerFacadeFactory;
		private const long QUERY_LOGGING_TIME_THRESHOLD = 5000;

		internal RelativityObjectManager(
			int workspaceArtifactId,
			IAPILog logger,
			IObjectManagerFacadeFactory objectManagerFacadeFactory)
		{
			_workspaceArtifactId = workspaceArtifactId;
			_logger = logger.ForContext<RelativityObjectManager>();
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

		public async Task<int> CreateAsync<T>(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
			where T : BaseRdo, new()
		{
			var createRequest = new CreateRequest
			{
				ObjectType = rdo.ToObjectType(),
				FieldValues = rdo.ToFieldValues().ToList()
			};
			SetParentArtifactId(createRequest, rdo);

			return await SendCreateRequestAsync(createRequest, executionIdentity).ConfigureAwait(false);
		}

		public async Task<int> CreateAsync(ObjectTypeRef objectType,
			List<FieldRefValuePair> fieldValues,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			var createRequest = new CreateRequest
			{
				ObjectType = objectType,
				FieldValues = fieldValues
			};
			return await SendCreateRequestAsync(createRequest, executionIdentity).ConfigureAwait(false);
		}

		public async Task<int> CreateAsync(ObjectTypeRef objectType,
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
			return await SendCreateRequestAsync(createRequest, executionIdentity).ConfigureAwait(false);
		}

		public T Read<T>(int artifactId, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
			where T : BaseRdo, new()
		{
			var request = new ReadRequest
			{
				Object = new RelativityObjectRef { ArtifactID = artifactId },
				Fields = new T().ToFieldList()
			};
			return SendReadRequest<T>(request, executionIdentity: executionIdentity);
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
			return SendReadRequest<T>(request, executionIdentity: executionIdentity);
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
			return SendUpdateRequest(request, executionIdentity, GetRdoType(rdo));
		}

		public async Task<bool> UpdateAsync(int artifactId,
			IList<FieldRefValuePair> fieldsValues,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			var request = new UpdateRequest
			{
				Object = new RelativityObjectRef { ArtifactID = artifactId },
				FieldValues = fieldsValues
			};

			string rdoType = GetRdoType(rdo: null);
			return await SendUpdateRequestAsync(request, executionIdentity, rdoType).ConfigureAwait(false);
		}

		public async Task<bool> UpdateAsync<T>(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
			where T : BaseRdo, new()
		{
			var request = new UpdateRequest
			{
				Object = rdo.ToObjectRef(),
				FieldValues = rdo.ToFieldValues().ToList()
			};
			return await SendUpdateRequestAsync(request, executionIdentity, GetRdoType(rdo)).ConfigureAwait(false);
		}

		public Task<bool> MassUpdateAsync(
			IEnumerable<int> objectsIDs,
			IEnumerable<FieldRefValuePair> fieldsToUpdate,
			FieldUpdateBehavior fieldUpdateBehavior,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			List<RelativityObjectRef> objectsToEdit = objectsIDs
				.Select(x => new RelativityObjectRef { ArtifactID = x })
				.ToList();

			var request = new MassUpdateByObjectIdentifiersRequest
			{
				Objects = objectsToEdit,
				FieldValues = fieldsToUpdate
			};

			var updateOptions = new MassUpdateOptions
			{
				UpdateBehavior = fieldUpdateBehavior
			};

			return SendMassUpdateRequestAsync(
				request,
				updateOptions,
				executionIdentity);
		}

		public Task<bool> MassDeleteAsync(IEnumerable<int> objectsIDs, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			List<RelativityObjectRef> objectsToDelete = objectsIDs
				.Select(x => new RelativityObjectRef { ArtifactID = x })
				.ToList();

			var massDeleteByCriteriaRequest = new MassDeleteByObjectIdentifiersRequest
			{
				Objects = objectsToDelete
			};

			return SendMassDeleteRequestAsync(massDeleteByCriteriaRequest, executionIdentity);
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
				
				Guid logGuid = Guid.NewGuid();
				
				do
				{
					Stopwatch sw = Stopwatch.StartNew();
					
					QueryResult partialResult = await client
						.QueryAsync(_workspaceArtifactId, q, retrievedResults + 1, _BATCH_SIZE)
						.ConfigureAwait(false);

					sw.Stop();

					if (sw.ElapsedMilliseconds > QUERY_LOGGING_TIME_THRESHOLD)
					{
						_logger.LogInformation("Partial query time was over the threshold: {details}", new
						{
							ElapsedTime = sw.ElapsedMilliseconds,
							StackTrace = Environment.StackTrace,
							QueryStartIndex = retrievedResults + 1,
							QueryCondition = q.Condition,
							LogGuid = logGuid
						});
					}
					
					totalResults = partialResult.TotalCount;
					if (output == null)
					{
						output = new List<T>(totalResults);
					}

					IEnumerable<T> partialResultsAsRdo = partialResult.Objects.Select(x => x.ToRDO<T>());
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

		public Stream StreamUnicodeLongText(
			int relativityObjectArtifactId,
			FieldRef longTextFieldRef,
			ExecutionIdentity executionIdentity)
		{
			try
			{
				Stream selfRecreatingStream =
					new SelfRecreatingStream(
						() => GetLongTextStreamAsync(relativityObjectArtifactId, longTextFieldRef, executionIdentity).GetAwaiter().GetResult(),
						_logger);
				var selfDisposingStream =
					new SelfDisposingStream(
						selfRecreatingStream,
						_logger);
				return selfDisposingStream;
			}
			catch (Exception ex)
			{
				string message = GetStreamLongTextErrorMessage(
					nameof(StreamUnicodeLongText),
					_workspaceArtifactId,
					relativityObjectArtifactId,
					longTextFieldRef,
					executionIdentity);

				HandleObjectManagerException(ex, message);
				throw;
			}
		}

		public Stream StreamNonUnicodeLongText(
			int relativityObjectArtifactId,
			FieldRef longTextFieldRef,
			ExecutionIdentity executionIdentity)
		{
			try
			{
				Stream selfRecreatingStream =
					new SelfRecreatingStream(
						() => GetLongTextStreamAsync(relativityObjectArtifactId, longTextFieldRef, executionIdentity).GetAwaiter().GetResult(),
						_logger);
				Stream asciiToUnicodeStream = new AsciiToUnicodeStream(selfRecreatingStream);
				var selfDisposingStream =
					new SelfDisposingStream(
						asciiToUnicodeStream,
						_logger);
				return selfDisposingStream;
			}
			catch (Exception ex)
			{
				string message = GetStreamLongTextErrorMessage(
					nameof(StreamNonUnicodeLongText),
					_workspaceArtifactId,
					relativityObjectArtifactId,
					longTextFieldRef,
					executionIdentity);

				HandleObjectManagerException(ex, message);
				throw;
			}
		}

		public async Task<ExportInitializationResults> InitializeExportAsync(
			QueryRequest queryRequest,
			int start,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			try
			{
				using (IObjectManagerFacade client = _objectManagerFacadeFactory.Create(executionIdentity))
				{
					return await client
						.InitializeExportAsync(_workspaceArtifactId, queryRequest, start)
						.ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				string message = GetInitializeExportErrorMessage(
					_workspaceArtifactId,
					queryRequest.Condition,
					start,
					executionIdentity);

				HandleObjectManagerException(ex, message);
				throw;
			}
		}

		public async Task<RelativityObjectSlim[]> RetrieveResultsBlockFromExportAsync(
			Guid runID,
			int resultsBlockSize,
			int exportIndexID,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			try
			{
				using (IObjectManagerFacade client = _objectManagerFacadeFactory.Create(executionIdentity))
				{
					var results = new List<RelativityObjectSlim>(resultsBlockSize);
					int remainingObjectsCount = resultsBlockSize;
					int startIndex = exportIndexID;
					RelativityObjectSlim[] partialResults;

					do
					{
						partialResults = await client
							.RetrieveResultsBlockFromExportAsync(_workspaceArtifactId, runID, remainingObjectsCount, startIndex)
							.ConfigureAwait(false);

						results.AddRange(partialResults);
						remainingObjectsCount -= partialResults.Length;
						startIndex += partialResults.Length;
					} while (remainingObjectsCount > 0 && partialResults.Any());

					return results.ToArray();
				}
			}
			catch (Exception ex)
			{
				string message = GetRetrieveNextResultsBlockFromExportErrorMessage(
					_workspaceArtifactId,
					runID.ToString(),
					resultsBlockSize,
					exportIndexID,
					executionIdentity);

				HandleObjectManagerException(ex, message);
				throw;
			}
		}

		public async Task<IExportQueryResult> QueryWithExportAsync(QueryRequest queryRequest, int start,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			try
			{
				ExportInitializationResults exportResult = await InitializeExportAsync(queryRequest, start, executionIdentity).ConfigureAwait(false);

				void ExceptionHandler(Exception exception, int blockSize, int exportStartIndex)
				{
					HandleObjectManagerException(exception,
							GetRetrieveNextResultsBlockFromExportErrorMessage(_workspaceArtifactId, exportResult.RunID.ToString(), blockSize, exportStartIndex, executionIdentity));
				}

				return  new ExportQueryResult(_objectManagerFacadeFactory, exportResult, _workspaceArtifactId, executionIdentity, ExceptionHandler);
			}
			catch (Exception ex)
			{
				string message = GetInitializeExportErrorMessage(
					_workspaceArtifactId,
					queryRequest.Condition,
					start,
					executionIdentity);

				HandleObjectManagerException(ex, message);
				throw;
			}
		}

		//This method was introduced during migration to SecretStore,
		//because it was really hard to decide which helper do
		//we need to use to get proper workspaceID for given case.
		//Do not use this method. It should be removed asap.
		public int GetWorkspaceID_Deprecated()
		{
			return _workspaceArtifactId;
		}

		private Task<ResultSet<T>> QueryAsync<T>(QueryRequest q,
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
						.ToList()
				};
			};

			T rdo = new T();
			BootstrapQuery(q, rdo, noFields);

			return SendQueryRequestAsync(func, q, rdo, executionIdentity);
		}

		private async Task<Stream> GetLongTextStreamAsync(
			int relativityObjectArtifactId,
			FieldRef longTextFieldRef,
			ExecutionIdentity executionIdentity)
		{
			using (IObjectManagerFacade client = _objectManagerFacadeFactory.Create(executionIdentity))
			{
				var exportObject = new RelativityObjectRef() { ArtifactID = relativityObjectArtifactId };
				IKeplerStream keplerStream = client.StreamLongTextAsync(
						_workspaceArtifactId,
						exportObject,
						longTextFieldRef)
					.GetAwaiter()
					.GetResult();
				return await keplerStream.GetStreamAsync().ConfigureAwait(false);
			}
		}

		private string GetStreamLongTextErrorMessage(
			string methodName,
			int workspaceArtifactID,
			int relativityObjectArtifactId,
			FieldRef longTextFieldRef,
			ExecutionIdentity executionIdentity)
		{
			var msgBuilder = new StringBuilder();
			msgBuilder.Append($"Error occurred when calling {methodName} method. ");
			msgBuilder.Append($"Workspace: ({workspaceArtifactID}) ");
			msgBuilder.Append($"ExportObject artifact ID: ({relativityObjectArtifactId}) ");
			msgBuilder.Append($"Long text field ({longTextFieldRef?.Name}) artifact ID: ({longTextFieldRef?.ArtifactID}) ");
			msgBuilder.Append($"Execution identity: {executionIdentity}");
			return msgBuilder.ToString();
		}

		private string GetInitializeExportErrorMessage(
			int workspaceArtifactID,
			string queryCondition,
			int startRecord,
			ExecutionIdentity executionIdentity)
		{
			var msgBuilder = new StringBuilder();
			msgBuilder.Append($"Error occured when calling {nameof(InitializeExportAsync)}. ");
			msgBuilder.Append($"Workspace: ({workspaceArtifactID}) ");
			msgBuilder.Append($"Query condition: ({queryCondition})");
			msgBuilder.Append($"Start record: ({startRecord}) ");
			msgBuilder.Append($"Execution identity: {executionIdentity}");
			return msgBuilder.ToString();
		}

		private string GetRetrieveNextResultsBlockFromExportErrorMessage(
			int workspaceArtifactID,
			string runID,
			int resultsBlockSize,
			int exportIndexID,
			ExecutionIdentity executionIdentity)
		{
			var msgBuilder = new StringBuilder();
			msgBuilder.Append($"Error occured when calling {nameof(RetrieveResultsBlockFromExportAsync)}. ");
			msgBuilder.Append($"Workspace: ({workspaceArtifactID}) ");
			msgBuilder.Append($"Run ID: ({runID}) ");
			msgBuilder.Append($"Results block size: ({resultsBlockSize}) ");
			msgBuilder.Append($"Export index ID: ({exportIndexID}) ");
			msgBuilder.Append($"Execution identity: {executionIdentity}");
			return msgBuilder.ToString();
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
			catch (Exception ex)
			{
				HandleObjectManagerException(ex, message: GetQueryErrorMessage(q, GetRdoType(rdo)));
				throw;
			}
		}

		private T SendReadRequest<T>(ReadRequest request,
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
					return result.Object.ToRDO<T>();
				}
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
					int artifactId = client.CreateAsync(_workspaceArtifactId, request)
						.GetAwaiter()
						.GetResult()
						.Object
						.ArtifactID;
					return artifactId;
				}
			}
			catch (Exception ex)
			{
				HandleObjectManagerException(ex, message: GetErrorMessage<CreateRequest>("[RelativityObject]"));
				throw;
			}
		}

		private async Task<int> SendCreateRequestAsync(CreateRequest request, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			try
			{
				using (IObjectManagerFacade client = _objectManagerFacadeFactory.Create(executionIdentity))
				{
					CreateResult result = await client.CreateAsync(_workspaceArtifactId, request).ConfigureAwait(false);
					return result.Object.ArtifactID;
				}
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
			catch (Exception ex)
			{
				HandleObjectManagerException(ex, message: GetErrorMessage<UpdateRequest>(rdoType));
				throw;
			}
		}

		private async Task<bool> SendUpdateRequestAsync(UpdateRequest request, ExecutionIdentity executionIdentity, string rdoType)
		{
			try
			{
				using (IObjectManagerFacade client = _objectManagerFacadeFactory.Create(executionIdentity))
				{
					UpdateResult result = await client.UpdateAsync(_workspaceArtifactId, request).ConfigureAwait(false);
					return result.EventHandlerStatuses.All(x => x.Success);
				}
			}
			catch (Exception ex)
			{
				HandleObjectManagerException(ex, message: GetErrorMessage<UpdateRequest>(rdoType));
				throw;
			}
		}

		private async Task<bool> SendMassUpdateRequestAsync(
			MassUpdateByObjectIdentifiersRequest request,
			MassUpdateOptions updateOptions,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			try
			{
				using (IObjectManagerFacade client = _objectManagerFacadeFactory.Create(executionIdentity))
				{
					MassUpdateResult result = await client
						.UpdateAsync(_workspaceArtifactId, request, updateOptions)
						.ConfigureAwait(false);
					return result.Success;
				}
			}
			catch (Exception ex)
			{
				HandleObjectManagerException(ex, message: GetErrorMessage<UpdateRequest>(_UNKNOWN_OBJECT_TYPE));
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
			catch (Exception ex)
			{
				HandleObjectManagerException(ex, message: GetErrorMessage<DeleteRequest>(rdoType));
				throw;
			}
		}

		private async Task<bool> SendMassDeleteRequestAsync(MassDeleteByObjectIdentifiersRequest request, ExecutionIdentity executionIdentity)
		{
			try
			{
				using (IObjectManagerFacade client = _objectManagerFacadeFactory.Create(executionIdentity))
				{
					MassDeleteResult result = await client
						.DeleteAsync(_workspaceArtifactId, request)
						.ConfigureAwait(false);
					return result.Success;
				}
			}
			catch (Exception ex)
			{
				HandleObjectManagerException(ex, message: GetErrorMessage<DeleteRequest>(_UNKNOWN_OBJECT_TYPE));
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
			if (ex is IntegrationPointsException)
			{
				return;
			}

			_logger.LogError(ex, message);
			throw new IntegrationPointsException(message, ex)
			{
				ShouldAddToErrorsTab = true,
				ExceptionSource = IntegrationPointsExceptionSource.KEPLER
			};
		}

		private string GetOperationNameForRequestType<T>()
		{
			return typeof(T).Name.Replace("Request", string.Empty).ToUpperInvariant();
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
			return rdo?.GetType().Name ?? _UNKNOWN_OBJECT_TYPE;
		}
	}
}
