﻿using System;
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
		try
		{
			_testHelper = new TestHelper();

			ImportIntegrationPointsToLibrary();

			CreateIntegrationPointAgents();

			ConfigureWebAPI();

			ConfigureFileShareServices();

			if (!FunctionalTemplateWorkspaceExists())
			{
				SetupTemplateWorkspace();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Functional tests setup fixture failed with error: {ex.Message}");
			IsInitialized = false;
		}
	}

	private void SetupTemplateWorkspace()
	{
		int workspaceID = CreateFunctionalTemplateWorkspace();
		InstallIntegrationPointsFromLibrary(workspaceID);
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
			applicationManager.ImportRipToLibraryAsync().GetAwaiter().GetResult();
		}
	}

	public void InstallIntegrationPointsFromLibrary(int workspaceID)
	{
		Console.WriteLine($"Importing Integration Points to workspace {workspaceID}...");

		var applicationManager = new RelativityApplicationManager(_testHelper);
		applicationManager.InstallRipFromLibraryAsync(workspaceID).GetAwaiter().GetResult();
	}

	private void CreateIntegrationPointAgents()
	{
		Console.WriteLine("Creating Integration Point Agents...");

		var success = Agent.CreateMaxIntegrationPointAgentsAsync().GetAwaiter().GetResult();
		if (!success)
		{
			throw new TestException("Creating Integration Point Agents has been failed");
		}
	}

	public void ConfigureWebAPI()
	{
		Console.WriteLine("Configure Web API Path...");

		bool isValid = InstanceSetting.CreateOrUpdateAsync("kCura.IntegrationPoints", "WebAPIPath", SharedVariables.RelativityWebApiUrl).GetAwaiter().GetResult();
		if (!isValid)
		{
			throw new TestException("Upgrading Web API Path has been failed");
		}
	}

	public void ConfigureFileShareServices()
	{
		Console.WriteLine("Import Fileshare Test Services...");

		InstanceSetting.CreateOrUpdateAsync("kCura.ARM", "DevelopmentMode", "True", Relativity.Services.InstanceSetting.ValueType.TrueFalse).GetAwaiter().GetResult();

		var applicationManager = new RelativityApplicationManager(_testHelper);
		applicationManager.ImportApplicationToLibraryAsync(SharedVariables.FileShareServicesPath).GetAwaiter().GetResult();
	}
}
