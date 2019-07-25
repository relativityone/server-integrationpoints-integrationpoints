using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	[Ignore("It's currently impossible to instantiate any repository from Relativity.API.Foundation in integration (system) tests. " +
	        "Doing so requires using classes from Relativity.APIHelper project, which is only available via RelativityCore package " +
	        "or via Helper instances which are passed to agent or custom page from Relativity and are not available in tests. " +
	        "What we specifically lack is an ability to instantiate implementations of Relativity.API classes (which generally " +
	        "reside in Relativity.APIHelper) without using RelativityCore package (which contains Relativity.APIHelper DLL). " +
	        "We can't use RelativityCore package - we're removing it as a part of the Strangling the Monolith initiative.")]
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

		[IdentifiedTest("ee83c2e2-8d78-472d-be47-a29fe34cf9f5")]
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