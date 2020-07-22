using System;
using System.Threading.Tasks;
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
	public async Task InitializeFixtureAsync()
	{
		_testHelper = new TestHelper();

		// REL-451648
		await ToggleHelper.SetToggleAsync("Relativity.Core.Api.TogglePerformance.ObjectManagerUseStaticStatelessValidators", false).ConfigureAwait(false);

		await CreateTemplateWorkspaceAsync().ConfigureAwait(false);
	}

	private async Task CreateTemplateWorkspaceAsync()
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
		if (SharedVariables.UseIpRapFile())
		{
			Console.WriteLine("Importing RIP RAP to Library...");
			await applicationManager.ImportRipToLibraryAsync().ConfigureAwait(false);
		}
        Console.WriteLine("Importing RIP RAP to workspace");
		await applicationManager.InstallRipFromLibraryAsync(workspaceTemplateID)
			.ContinueWith(t =>
				InstanceSetting.CreateOrUpdateAsync("kCura.IntegrationPoints", "WebAPIPath", SharedVariables.RelativityWebApiUrl))
			.ConfigureAwait(false);
	}
}
