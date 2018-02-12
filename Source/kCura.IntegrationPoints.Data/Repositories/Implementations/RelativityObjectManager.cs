using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Data.UtilityDTO;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Kepler.Exceptions;
using Relativity.SecretCatalog;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using FieldRef = Relativity.Services.Objects.DataContracts.FieldRef;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;
using RelativityObjectRef = Relativity.Services.Objects.DataContracts.RelativityObjectRef;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RelativityObjectManager : IRelativityObjectManager
	{
		private IHelper _helper;
		private readonly IAPILog _logger;
		private int _workspaceArtifactId;
		private ISecretCatalog _secretCatalog;
		private readonly ISecretCatalogFactory _secretCatalogFactory;
		private readonly ISecretManager _secretManager;

		public RelativityObjectManager(int workspaceArtifactId, IHelper helper, ISecretCatalogFactory secretCatalogFactory, ISecretManager secretManager)
		{
			_workspaceArtifactId = workspaceArtifactId;
			_helper = helper;
			_logger = _helper.GetLoggerFactory().GetLogger().ForContext<RelativityObjectManager>();
			_secretCatalogFactory = secretCatalogFactory;
			_secretManager = secretManager;
		}

		private ISecretCatalog SecretCatalog
		{
			get
			{
				if (_secretCatalog != null)
				{
					return _secretCatalog;
				}
				_secretCatalog = _secretCatalogFactory.Create(_workspaceArtifactId);
				return _secretCatalog;
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

		public int Create<T>(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			try
			{
				using (var client = _helper.GetServicesManager().CreateProxy<IObjectManager>(executionIdentity))
				{
					CreateRequest createRequest = new CreateRequest()
					{
						ObjectType = rdo.ToObjectType(),
						FieldValues = rdo.ToFieldValues().ToList()
					};

					SetParentArtifactId(createRequest, rdo);

					SetEncryptedSecuredConfigurationForNewRdo(createRequest.FieldValues);

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
				throw LogObjectManagerException(rdo, "create", ex);
			}
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

		private T Read<T>(int artifactId, bool decryptSecuredConfiguration, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new(){
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
				using (var client = _helper.GetServicesManager().CreateProxy<IObjectManager>(executionIdentity))
				{
					var result = client.ReadAsync(_workspaceArtifactId, request)
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

		private T SetDecryptedSecuredConfiguration<T>(T rdo) where T : BaseRdo, new()
		{
			if (rdo is IntegrationPoint)
			{
				var secretId = rdo.GetField<string>(new Guid(IntegrationPointFieldGuids.SecuredConfiguration));
				if (!String.IsNullOrWhiteSpace(secretId))
				{
					var decryptedSecret = DecryptSecuredConfiguration(secretId);
					if (!String.IsNullOrWhiteSpace(decryptedSecret))
					{
						rdo.SetField<string>(new Guid(IntegrationPointFieldGuids.SecuredConfiguration), decryptedSecret);
						return rdo;
					}
				}
			}
			return rdo;
		}

		public bool Update<T>(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			try
			{
				using (var client = _helper.GetServicesManager().CreateProxy<IObjectManager>(executionIdentity))
				{
					UpdateRequest request = new UpdateRequest()
					{
						Object = rdo.ToObjectRef(),
						FieldValues = rdo.ToFieldValues().ToList()
					};

					SetEncryptedSecuredConfigurationForExistingRdo(rdo.ArtifactId, request.FieldValues);

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
				throw LogObjectManagerException(rdo, "update", ex);
			}
		}

		public bool Delete<T>(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			try
			{
				using (var client = _helper.GetServicesManager().CreateProxy<IObjectManager>(executionIdentity))
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
				using (var client = _helper.GetServicesManager().CreateProxy<IObjectManager>(executionIdentity))
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
				throw LogObjectManagerException(null, "delete", ex);
			}
		}


		public ResultSet<T> Query<T>(QueryRequest q, int start, int length, bool noFields = false, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			try
			{
				BootstrapQuery<T>(q, noFields);

				using (var client = _helper.GetServicesManager().CreateProxy<IObjectManager>(executionIdentity))
				{
					var queryResults = client.QueryAsync(_workspaceArtifactId, q, start, length).GetAwaiter().GetResult();
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

		public List<T> Query<T>(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			return QueryAsync<T>(q, false, executionIdentity).GetAwaiter().GetResult();
		}

		public async Task<List<T>> QueryAsync<T>(QueryRequest q, bool noFields = false, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser) where T : BaseRdo, new()
		{
			try
			{
				BootstrapQuery<T>(q, noFields);

				using (var client = _helper.GetServicesManager().CreateProxy<IObjectManager>(executionIdentity))
				{
					QueryResult result = await client.QueryAsync(_workspaceArtifactId, q, 0, 0).ConfigureAwait(false);

					if (result.TotalCount > result.Objects.Count)
					{
						QueryResult missingResults = await client.QueryAsync(_workspaceArtifactId, q, result.TotalCount, result.TotalCount - result.Objects.Count).ConfigureAwait(false);

						return result.Objects.Concat(missingResults.Objects).Select(x => x.ToRDO<T>()).Select(SetDecryptedSecuredConfiguration).ToList();
					}
					return result.Objects.Select(x => x.ToRDO<T>()).Select(SetDecryptedSecuredConfiguration).ToList();
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
				using (var client = _helper.GetServicesManager().CreateProxy<IObjectManager>(executionIdentity))
				{
					var result = await client.QueryAsync(_workspaceArtifactId, q, 0, 0).ConfigureAwait(false);

					if (result.TotalCount > result.Objects.Count)
					{
						QueryResult missingResults = await client.QueryAsync(_workspaceArtifactId, q, result.TotalCount, result.TotalCount - result.Objects.Count).ConfigureAwait(false);

						return result.Objects.Concat(missingResults.Objects).ToList();
					}
					return result.Objects;
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

		public int QueryTotalCount(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			return QueryTotalCountAsync(q, executionIdentity).GetAwaiter().GetResult();
		}

		public async Task<int> QueryTotalCountAsync(QueryRequest q, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			try
			{
				using (var client = _helper.GetServicesManager().CreateProxy<IObjectManager>(executionIdentity))
				{
					var result = await client.QueryAsync(_workspaceArtifactId, q, 0, 1).ConfigureAwait(false);
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

		private IntegrationPointsException LogObjectManagerException(BaseRdo rdo, string operationName, Exception ex)
		{
			string rdoType = rdo?.GetType().Name ?? "[UnknownObjectType]";
			string message = $"Cannot {operationName} object of type {rdoType} with ObjectManager (Workspace: {_workspaceArtifactId})";
			_logger.LogError(ex, "Cannot {operationName} object of type {rdoType} with ObjectManager (Workspace: {_workspaceArtifactId})", operationName, rdoType, _workspaceArtifactId);
			return new IntegrationPointsException(message, ex)
			{
				ShouldAddToErrorsTab = true,
				ExceptionSource = IntegrationPointsExceptionSource.KEPLER
			};
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

		private void SetEncryptedSecuredConfigurationForNewRdo(IEnumerable<FieldRefValuePair> fieldValues)
		{
			SetEncryptedSecuredConfiguration(fieldValues,
				(securedConfiguration) =>
				{
					return EncryptSecuredConfigurationForNewRdo(securedConfiguration);
				});
		}

		private void SetEncryptedSecuredConfigurationForExistingRdo(int artifactId, IEnumerable<FieldRefValuePair> fieldValues, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			SetEncryptedSecuredConfiguration(fieldValues,
				(securedConfiguration) =>
				{
					return EncryptSecuredConfigurationForExistingRdo(artifactId, securedConfiguration, executionIdentity);
				});
		}

		private void SetEncryptedSecuredConfiguration(IEnumerable<FieldRefValuePair> fieldValues, Func<string, string> encryptFunc)
		{
			var securedConfigurationField = fieldValues.FirstOrDefault(x =>
				x.Field.Guid == new Guid(IntegrationPointFieldGuids.SecuredConfiguration));
			if (securedConfigurationField != null)
			{
				securedConfigurationField.Value = encryptFunc(securedConfigurationField.Value as string);
			}
		}

		private string EncryptSecuredConfigurationForNewRdo(string securedConfiguration)
		{
			return EncryptSecuredConfiguration(securedConfiguration,
				(sc) =>
				{
					var secretData = _secretManager.CreateSecretData(sc);
					var secretIdentifier = _secretManager.GenerateIdentifier();
					SecretCatalog.WriteSecret(secretIdentifier, secretData);
					return secretIdentifier.SecretID;
				});
		}

		private string EncryptSecuredConfigurationForExistingRdo(int artifactId, string securedConfiguration, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			return EncryptSecuredConfiguration(securedConfiguration,
				(sc) =>
				{
					var secretData = _secretManager.CreateSecretData(sc);
					IntegrationPoint existingRdo = Read<IntegrationPoint>(artifactId, false, executionIdentity);
					var secretIdentifier = _secretManager.RetrieveIdentifier(existingRdo);
					SecretCatalog.WriteSecret(secretIdentifier, secretData);
					return secretIdentifier.SecretID;
				});
		}

		private string EncryptSecuredConfiguration(string securedConfiguration, Func<string, string> encryptionFunc)
		{
			if (securedConfiguration == null)
			{
				return null;
			}
			try
			{
				return encryptionFunc(securedConfiguration);
			}
			catch (FieldNotFoundException ex)
			{
				_logger.LogWarning(ex, "Can not write Secured Configuration for Integration Point record during encryption process (Secret config: {securedConfiguration} )", securedConfiguration);
				//Ignore as Integration Point RDO doesn't always include SecuredConfiguration
				//Any access to missing fieldGuid will throw FieldNotFoundException
				return securedConfiguration;
			}
		}


		private string DecryptSecuredConfiguration(string secretId)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(secretId))
				{
					return null;
				}
				var secretIdentifier = _secretManager.RetrieveIdentifier(secretId);
				var secretData = SecretCatalog.GetSecret(secretIdentifier);
				return _secretManager.RetrieveValue(secretData);
			}
			catch (FieldNotFoundException ex)
			{
				//Ignore as Integration Point RDO doesn't always include SecuredConfiguration
				//Any access to missing fieldGuid will throw FieldNotFoundException
				_logger.LogWarning(ex, "Can not retrieve Secured Configuration for Integration Point record during decryption process (Secret Id: {secretId} )", secretId);

				return secretId;
			}
		}
	}
}
