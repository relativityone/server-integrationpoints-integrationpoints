using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	public class RelativityAuditRepositoryTests : RelativityProviderTemplate
	{
		public RelativityAuditRepositoryTests() : base("RelativityAuditRepositoryTests", null)
		{
		}

		private IAuditManager _auditManager;

		[SetUp]
		public void SetUp()
		{
			IManagerFactory factory = Container.Resolve<IManagerFactory>();
			IContextContainerFactory contextFactory = Container.Resolve<IContextContainerFactory>();
			IContextContainer contextContainer = contextFactory.CreateContextContainer(Helper);
			_auditManager = factory.CreateAuditManager(contextContainer, WorkspaceArtifactId);
		}

		[Test]
		public void CreateAuditRecord()
		{
			// arrange
			const string auditMessage = "Test audit.";
			IntegrationPointModel model = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly,
				"CreateAuditRecord ", "Append Only");
			model = CreateOrUpdateIntegrationPoint(model);

			// act
			AuditElement detail = new AuditElement()
			{
				AuditMessage = auditMessage
			};
			_auditManager.RelativityAuditRepository.CreateAuditRecord(model.ArtifactID, detail);

			// assert
			var auditHelper = new kCura.IntegrationPoint.Tests.Core.AuditHelper(Helper);
			Audit audit = auditHelper.RetrieveLastAuditsForArtifact(WorkspaceArtifactId,
				Core.Constants.IntegrationPoints.INTEGRATION_POINT_OBJECT_TYPE_NAME, model.Name)[0];

			Assert.AreEqual("Run", audit.AuditAction);
			Assert.AreEqual($"<auditElement><auditMessage>{auditMessage}</auditMessage></auditElement>", audit.AuditDetails);
			Assert.AreEqual(model.Name, audit.ArtifactName);
		}
	}
}