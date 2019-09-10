using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.Services.ResourceServer;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using Relativity.API;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	/// <summary>
	/// This suite tests integration between RIP and ResourceServerManager kepler service
	/// </summary>
	[TestFixture]
	public class CaseRepositoryTest
	{
		private int _workspaceId;
		private ITestHelper _helper;
		private CaseRepository _sut;

		private const string _WORKSPACE_NAME = "Tests_ResourceServerManager_RIP_Integrations";

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_workspaceId = Workspace.CreateWorkspace(_WORKSPACE_NAME);
			_helper = new TestHelper();
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			Workspace.DeleteWorkspace(_workspaceId);
		}

		[SetUp]
		public void SetUp()
		{
			IAPILog logger = Substitute.For<IAPILog>();
			IExternalServiceInstrumentationProvider instrumentationProvider = 
				new ExternalServiceInstrumentationProviderWithoutJobContext(logger);
			IResourceServerManager resourceServerManager = _helper.CreateProxy<IResourceServerManager>();

			_sut = new CaseRepository(resourceServerManager, instrumentationProvider);
		}

		[IdentifiedTest("a7b270f4-f423-461d-bb56-58fb84475969")]
		[SmokeTest]
		public void ItShouldReturnWorkspaceInfo()
		{
			// act
			ICaseInfoDto caseInfo = _sut.Read(_workspaceId);

			// assert
			Assert.AreEqual(_WORKSPACE_NAME, caseInfo.Name);
		}
	}
}
