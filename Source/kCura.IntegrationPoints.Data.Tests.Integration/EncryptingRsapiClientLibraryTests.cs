using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using Relativity.Core;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.Data.Tests.Integration
{
	public class EncryptingRsapiClientLibraryTests : SourceProviderTemplate
	{
		private IList<string> _existingSecrets;
		private EncryptingRsapiClientLibrary _encryptingRsapiClientLibrary;
		private ISecretCatalog _secretCatalog;
		private ISecretManager _secretManager;

		public EncryptingRsapiClientLibraryTests() : base($"EncryptingClient_{Utils.FormatedDateTimeNow}")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			RetrieveExistingSecrets();

			_secretCatalog = SecretStoreFactory.GetSecretStore(BaseServiceContextHelper.Create().GetMasterRdgContext());
			_secretManager = new SecretManager();
			_encryptingRsapiClientLibrary = new EncryptingRsapiClientLibrary(new RsapiClientLibrary<IntegrationPoint>(Helper, WorkspaceArtifactId), _secretCatalog,
				_secretManager);
		}

		public override void SuiteTeardown()
		{
			CleanUpSecrets();
			base.SuiteTeardown();
		}

		[Test]
		public void GoldWorkflow_Create_Update_Read_Delete()
		{
			var firstSecret = "big_secret_1";
			var secondSecret = "big_secret_2";

			var rdo = new IntegrationPoint
			{
				SecuredConfiguration = firstSecret
			};

			rdo.ArtifactId = _encryptingRsapiClientLibrary.Create(rdo);
			var secretId = rdo.SecuredConfiguration;
			Assert.That(IntegrationPointHasSecretId(rdo.ArtifactId, secretId));
			Assert.That(SecretExistsInDatabase(secretId));

			rdo.SecuredConfiguration = secondSecret;
			_encryptingRsapiClientLibrary.Update(rdo);
			Assert.That(rdo.SecuredConfiguration, Is.EqualTo(secretId));

			var actualRdo = _encryptingRsapiClientLibrary.Read(rdo.ArtifactId);
			Assert.That(actualRdo.SecuredConfiguration, Is.EqualTo(secondSecret));

			_encryptingRsapiClientLibrary.Delete(rdo.ArtifactId);
			Assert.That(IntegrationPointDoesntExist(rdo.ArtifactId));
			Assert.That(SecretDoesntExistInDatabase(secretId));
		}

		[Test]
		[TestCase(null)]
		[TestCase("")]
		[TestCase(" ")]
		[TestCase("secret_secret")]
		[TestCase("{json: \"example\"}")]
		public void Create(string expectedSecuredConfiguration)
		{
			var rdo = new IntegrationPoint
			{
				SecuredConfiguration = expectedSecuredConfiguration
			};

			var integrationPointId = _encryptingRsapiClientLibrary.Create(rdo);

			Assert.That(IntegrationPointHasSecretId(integrationPointId, rdo.SecuredConfiguration));
			Assert.That(SecretExistsInDatabase(rdo.SecuredConfiguration));
			Assert.That(rdo.SecuredConfiguration, Is.Not.EqualTo(expectedSecuredConfiguration));
			Assert.That(ReadSecret(rdo.SecuredConfiguration), Is.EqualTo(expectedSecuredConfiguration));
		}

		[Test]
		[TestCase(null)]
		[TestCase("")]
		[TestCase(" ")]
		[TestCase("secret_secret")]
		[TestCase("{json: \"example\"}")]
		public void Update(string expectedSecuredConfiguration)
		{
			var rdo = new IntegrationPoint
			{
				SecuredConfiguration = "big_secret"
			};

			rdo.ArtifactId = _encryptingRsapiClientLibrary.Create(rdo);
			var securedConfigurationBeforeUpdate = rdo.SecuredConfiguration;

			rdo.SecuredConfiguration = expectedSecuredConfiguration;
			_encryptingRsapiClientLibrary.Update(rdo);
			var securedConfigurationAfterUpdate = rdo.SecuredConfiguration;

			Assert.That(IntegrationPointHasSecretId(rdo.ArtifactId, rdo.SecuredConfiguration));
			Assert.That(SecretExistsInDatabase(rdo.SecuredConfiguration));
			Assert.That(rdo.SecuredConfiguration, Is.Not.EqualTo(expectedSecuredConfiguration));
			Assert.That(ReadSecret(rdo.SecuredConfiguration), Is.EqualTo(expectedSecuredConfiguration));
			Assert.That(securedConfigurationAfterUpdate, Is.EqualTo(securedConfigurationBeforeUpdate));
		}

		[Test]
		[TestCase(null)]
		[TestCase("")]
		[TestCase(" ")]
		[TestCase("secret_secret")]
		[TestCase("{json: \"example\"}")]
		public void UpdateOldIntegrationPoint(string expectedSecuredConfiguration)
		{
			var integrationPointLibrary = new RsapiClientLibrary<IntegrationPoint>(Helper, WorkspaceArtifactId);

			var oldRdo = new IntegrationPoint
			{
				Name = "ip"
			};
			oldRdo.ArtifactId = integrationPointLibrary.Create(oldRdo);

			oldRdo.SecuredConfiguration = expectedSecuredConfiguration;
			_encryptingRsapiClientLibrary.Update(oldRdo);

			Assert.That(IntegrationPointHasSecretId(oldRdo.ArtifactId, oldRdo.SecuredConfiguration));
			Assert.That(SecretExistsInDatabase(oldRdo.SecuredConfiguration));
			Assert.That(oldRdo.SecuredConfiguration, Is.Not.EqualTo(expectedSecuredConfiguration));
			Assert.That(ReadSecret(oldRdo.SecuredConfiguration), Is.EqualTo(expectedSecuredConfiguration));
		}

		[Test]
		public void UpdateWithoutSecuredConfigurationField()
		{
			var expectedSecuredConfiguration = "secret_763";
			var oldRdo = new IntegrationPoint
			{
				Name = $"ip_{Guid.NewGuid()}",
				SecuredConfiguration = expectedSecuredConfiguration
			};

			var integrationPointId = _encryptingRsapiClientLibrary.Create(oldRdo);
			
			var newIntegrationPointName = $"ip_{Guid.NewGuid()}";
			var newRdo = new IntegrationPoint
			{
				ArtifactId = integrationPointId,
				Name = newIntegrationPointName
			};

			_encryptingRsapiClientLibrary.Update(newRdo);

			var actualRdo = _encryptingRsapiClientLibrary.Read(integrationPointId);

			Assert.That(actualRdo.Name, Is.EqualTo(newIntegrationPointName));
			Assert.That(actualRdo.SecuredConfiguration, Is.EqualTo(expectedSecuredConfiguration));
		}

		[Test]
		[TestCase(null)]
		[TestCase("")]
		[TestCase(" ")]
		[TestCase("secret_secret")]
		[TestCase("{json: \"example\"}")]
		public void Read(string expectedSecuredConfiguration)
		{
			var rdo = new IntegrationPoint
			{
				SecuredConfiguration = expectedSecuredConfiguration
			};

			var integrationPointId = _encryptingRsapiClientLibrary.Create(rdo);

			var actualRdo = _encryptingRsapiClientLibrary.Read(integrationPointId);

			Assert.That(actualRdo.SecuredConfiguration, Is.EqualTo(expectedSecuredConfiguration));
		}

		[Test]
		public void ReadOldIntegrationPoint()
		{
			var integrationPointLibrary = new RsapiClientLibrary<IntegrationPoint>(Helper, WorkspaceArtifactId);

			var rdo = new IntegrationPoint
			{
				Name = "ip"
			};

			var integrationPointId = integrationPointLibrary.Create(rdo);

			var actualRdo = _encryptingRsapiClientLibrary.Read(integrationPointId);

			Assert.That(actualRdo.SecuredConfiguration, Is.Null.Or.Empty);
		}

		[Test]
		public void Delete()
		{
			var rdo = new IntegrationPoint
			{
				SecuredConfiguration = "secret_232"
			};

			var integrationPointId = _encryptingRsapiClientLibrary.Create(rdo);

			_encryptingRsapiClientLibrary.Delete(integrationPointId);

			Assert.That(IntegrationPointDoesntExist(integrationPointId));
			Assert.That(SecretDoesntExistInDatabase(rdo.SecuredConfiguration));
		}

		[Test]
		public void DeleteOldIntegrationPoint()
		{
			var integrationPointLibrary = new RsapiClientLibrary<IntegrationPoint>(Helper, WorkspaceArtifactId);

			var rdo = new IntegrationPoint
			{
				Name = "ip"
			};
			var integrationPointId = integrationPointLibrary.Create(rdo);

			_encryptingRsapiClientLibrary.Delete(integrationPointId);

			Assert.That(IntegrationPointDoesntExist(integrationPointId));
		}

		[Test]
		[TestCase(null)]
		[TestCase("")]
		[TestCase(" ")]
		[TestCase("secret_secret")]
		[TestCase("{json: \"example\"}")]
		public void Query(string expectedSecuredConfiguration)
		{
			var rdo = new IntegrationPoint
			{
				Name = $"ip_{Guid.NewGuid()}",
				SecuredConfiguration = expectedSecuredConfiguration
			};

			_encryptingRsapiClientLibrary.Create(rdo);

			Query<RDO> query = new Query<RDO>
			{
				Fields = FieldValue.AllFields,
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.IntegrationPoint),
				Condition = new TextCondition(new Guid(IntegrationPointFieldGuids.Name), TextConditionEnum.EqualTo, rdo.Name)
			};

			var actualRdo = _encryptingRsapiClientLibrary.Query(query)[0];

			Assert.That(actualRdo.SecuredConfiguration, Is.EqualTo(expectedSecuredConfiguration));
		}

		[Test]
		public void QueryOldIntegrationPoint()
		{
			var integrationPointLibrary = new RsapiClientLibrary<IntegrationPoint>(Helper, WorkspaceArtifactId);

			var rdo = new IntegrationPoint
			{
				Name = $"ip_{Guid.NewGuid()}"
			};

			integrationPointLibrary.Create(rdo);

			Query<RDO> query = new Query<RDO>
			{
				Fields = FieldValue.AllFields,
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.IntegrationPoint),
				Condition = new TextCondition(new Guid(IntegrationPointFieldGuids.Name), TextConditionEnum.EqualTo, rdo.Name)
			};

			var actualRdo = _encryptingRsapiClientLibrary.Query(query)[0];

			Assert.That(actualRdo.SecuredConfiguration, Is.Null.Or.Empty);
		}

		[Test]
		public void QueryWithoutSecuredConfigurationField()
		{
			var rdo = new IntegrationPoint
			{
				Name = $"ip_{Guid.NewGuid()}",
				SecuredConfiguration = "secret_secret"
			};

			_encryptingRsapiClientLibrary.Create(rdo);

			Query<RDO> query = new Query<RDO>
			{
				Fields = new List<FieldValue> {new FieldValue(new Guid(IntegrationPointFieldGuids.Name))},
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.IntegrationPoint),
				Condition = new TextCondition(new Guid(IntegrationPointFieldGuids.Name), TextConditionEnum.EqualTo, rdo.Name)
			};

			var actualRdo = _encryptingRsapiClientLibrary.Query(query)[0];

			Assert.That(actualRdo.Name, Is.EqualTo(rdo.Name));
			Assert.That(() => actualRdo.SecuredConfiguration, Throws.TypeOf<FieldNotFoundException>());
		}

		private string ReadSecret(string secretId)
		{
			var secret = _secretManager.RetrieveIdentifier(secretId);
			return _secretManager.RetrieveValue(_secretCatalog.GetSecret(secret));
		}

		private bool IntegrationPointHasSecretId(int integrationPointId, string secretId)
		{
			var sqlStatement = $"SELECT COUNT(*) FROM [EDDSDBO].[IntegrationPoint] WHERE [SecuredConfiguration] = '{secretId}' AND [ArtifactId] = {integrationPointId}";
			return Helper.GetDBContext(WorkspaceArtifactId).ExecuteSqlStatementAsScalar<int>(sqlStatement) > 0;
		}

		private bool IntegrationPointDoesntExist(int integrationPointId)
		{
			var sqlStatement = $"SELECT COUNT(*) FROM [EDDSDBO].[IntegrationPoint] WHERE [ArtifactId] = {integrationPointId}";
			return Helper.GetDBContext(WorkspaceArtifactId).ExecuteSqlStatementAsScalar<int>(sqlStatement) == 0;
		}

		private bool SecretExistsInDatabase(string secretId)
		{
			var sqlStatement = $"SELECT COUNT(*) FROM [EDDS].[eddsdbo].[SQLSecretStore] WHERE [SecretID] = '{secretId}'";
			return Helper.GetDBContext(-1).ExecuteSqlStatementAsScalar<int>(sqlStatement) > 0;
		}

		private bool SecretDoesntExistInDatabase(string secretId)
		{
			var sqlStatement = $"SELECT COUNT(*) FROM [EDDS].[eddsdbo].[SQLSecretStore] WHERE [SecretID] = '{secretId}'";
			return Helper.GetDBContext(-1).ExecuteSqlStatementAsScalar<int>(sqlStatement) == 0;
		}

		private void RetrieveExistingSecrets()
		{
			//TODO change this to Tenant after fixing database schema
			_existingSecrets = new List<string>();
			var secretsDataTable = Helper.GetDBContext(-1).ExecuteSqlStatementAsDataTable("SELECT [SecretID] FROM [EDDS].[eddsdbo].[SQLSecretStore]");
			foreach (DataRow row in secretsDataTable.Rows)
			{
				_existingSecrets.Add(row[0].ToString());
			}
		}

		private void CleanUpSecrets()
		{
			if (_existingSecrets.Count == 0)
			{
				return;
			}
			var ids = _existingSecrets.Select(x => $"'{x}'");
			Helper.GetDBContext(-1).ExecuteNonQuerySQLStatement($"DELETE FROM [EDDS].[eddsdbo].[SQLSecretStore] WHERE [SecretID] NOT IN ({string.Join(",", ids)})");
		}
	}
}