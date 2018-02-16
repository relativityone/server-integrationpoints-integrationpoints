using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.SecretCatalog;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SecretStoreHelper : ISecretStoreHelper
	{
		private ISecretCatalog SecretCatalog
		{
			get
			{
				if (_secretCatalog == null)
					_secretCatalog = _secretCatalogFactory.Create(_workspaceArtifactId);
				return _secretCatalog;
			}
		}

		private int _workspaceArtifactId;
		private ISecretCatalogFactory _secretCatalogFactory;
		private ISecretManager _secretManager;
		private readonly IAPILog _logger;
		private IHelper _helper;
		private ISecretCatalog _secretCatalog;

		public SecretStoreHelper(int workspaceArtifactId, IHelper helper, ISecretManager secretManager, ISecretCatalogFactory secretCatalogFactory)
		{
			_workspaceArtifactId = workspaceArtifactId;
			_helper = helper;
			_secretCatalogFactory = secretCatalogFactory;
			_secretManager = secretManager;
			_logger = _helper.GetLoggerFactory().GetLogger().ForContext<RelativityObjectManager>();
		}

		public void SetEncryptedSecuredConfigurationForNewRdo(IEnumerable<FieldRefValuePair> fieldValues)
		{
			SetEncryptedSecuredConfiguration(fieldValues,
				(securedConfiguration) =>
				{
					return EncryptSecuredConfigurationForNewRdo(securedConfiguration);
				});
		}

		public void SetEncryptedSecuredConfigurationForExistingRdo(IntegrationPoint existingRdo, IEnumerable<FieldRefValuePair> fieldValues, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			SetEncryptedSecuredConfiguration(fieldValues,
				(securedConfiguration) =>
				{
					return EncryptSecuredConfigurationForExistingRdo(existingRdo, securedConfiguration, executionIdentity);
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

		private string EncryptSecuredConfigurationForExistingRdo(IntegrationPoint existingRdo, string securedConfiguration, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			return EncryptSecuredConfiguration(securedConfiguration,
				(sc) =>
				{
					var secretData = _secretManager.CreateSecretData(sc);
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

		public string DecryptSecuredConfiguration(string secretId)
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