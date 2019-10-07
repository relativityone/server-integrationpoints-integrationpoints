using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

//It is intended that this fixture is not surrounded by namespace
//since NUnit requires it to execute such SetUpFixture for whole assembly
[SetUpFixture]
public class FunctionalTestsSetupFixture
{
	private ITestHelper _testHelper;

	[OneTimeSetUp]
	public void InitializeFixture()
	{
		_testHelper = new TestHelper();
		CreateTemplateWorkspace();
	}

	private void CreateTemplateWorkspace()
	{
		bool templateExists = Workspace.CheckIfWorkspaceExists(
			WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME
		);

		if (templateExists)
		{
			return;
		}

		int workspaceTemplateID = Workspace.CreateWorkspace(
			WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME,
			WorkspaceTemplateNames.RELATIVITY_STARTER_TEMPLATE_NAME
		);

		var applicationManager = new RelativityApplicationManager(_testHelper);
		await applicationManager.InstallRipFromLibraryAsync(workspaceTemplateID).ConfigureAwait(false);
	}
}
