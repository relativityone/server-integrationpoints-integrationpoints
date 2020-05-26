using System;
using System.IO;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

//It is intended that this fixture is not surrounded by namespace
//since NUnit requires it to execute such SetUpFixture for whole assembly
[SetUpFixture]
public class FunctionalTestsSetupFixture
{
	private ITestHelper _testHelper;

	public static bool IsInitialized = true;

	[OneTimeSetUp]
	public void InitializeFixture()
	{
		_testHelper = new TestHelper();

		if (!FunctionalTemplateWorkspaceExists())
		{
			SetupTemplateWorkspace();
		}
	}

	private void SetupTemplateWorkspace()
	{
		try
		{
			int workspaceID = CreateFunctionalTemplateWorkspace();

			ImportIntegrationPointsToLibrary();

			InstallIntegrationPointsFromLibrary(workspaceID);

			ConfigureWebAPI();

			ConfigureFileShareServices();
		}
		catch(Exception ex)
		{
			Console.WriteLine($"Setup Functional Tests Template Workspace failed with error: {ex.Message}");
			IsInitialized = false;
		}
	}

	public bool FunctionalTemplateWorkspaceExists() =>
		Workspace.CheckIfWorkspaceExists(WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME);

	public int CreateFunctionalTemplateWorkspace() =>
			Workspace.CreateWorkspace(WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME, WorkspaceTemplateNames.RELATIVITY_STARTER_TEMPLATE_NAME);

	public void ImportIntegrationPointsToLibrary()
	{
		var applicationManager = new RelativityApplicationManager(_testHelper);
		if (SharedVariables.UseIpRapFile())
		{
			Console.WriteLine("Importing Integration Points to Library...");
			applicationManager.ImportRipToLibraryAsync().Wait();
		}
	}

	public void InstallIntegrationPointsFromLibrary(int workspaceID)
	{
		Console.WriteLine($"Importing Integration Points to workspace {workspaceID}...");
		
		var applicationManager = new RelativityApplicationManager(_testHelper);
		applicationManager.InstallRipFromLibraryAsync(workspaceID).Wait();
	}

	public void ConfigureWebAPI()
	{
		Console.WriteLine("Configure Web API Path...");

		bool isValid = InstanceSetting.CreateOrUpdateAsync("kCura.IntegrationPoints", "WebAPIPath", SharedVariables.RelativityWebApiUrl).Result;
		if (!isValid)
			throw new TestException("Upgrading Web API Path has been failed");	
	}

	public void ConfigureFileShareServices()
	{
		Console.WriteLine("Import Fileshare Test Services...");

		InstanceSetting.CreateOrUpdateAsync("kCura.ARM", "DevelopmentMode", "True").Wait();

		var applicationManager = new RelativityApplicationManager(_testHelper);
		applicationManager.ImportApplicationToLibraryAsync(SharedVariables.FileShareServicesPath).Wait();
	}
}
