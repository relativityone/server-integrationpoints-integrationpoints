using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.Testing.Identification;
using Rip.E2ETests.Constants;
using CoreConstants = kCura.IntegrationPoints.Core.Constants;

namespace Rip.E2ETests.Installation
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class RipInstallationTests
	{
		private RelativityApplicationManager ApplicationManager { get; }
		private RelativityObjectManagerFactory ObjectManagerFactory { get; }

		private readonly string[] _ripInternalSourceProviders = { "LDAP", "FTP (CSV File)", "Load File", "Relativity" };
		private readonly string[] _ripInternalDestinationProviders = { "Relativity", "Load File" };
		private const string _JSON_LOADER_APPLICATION_NAME = "JsonLoader";
		private const string _JSON_LOADER_SOURCE_PROVIDER_NAME = "JSON";
		private const string _JSON_LOADER_GUID = "57151c17-cd92-4a6e-800c-a75bf807d097";
		private const string _RIP_GUID = CoreConstants.IntegrationPoints.APPLICATION_GUID_STRING;

		private const string _WORKSPACE_TEMPLATE_WITHOUT_RIP = WorkspaceTemplateNames.NEW_CASE_TEMPLATE_NAME;
		private readonly string _mainWorkspaceName = $"RipInstallTest{Guid.NewGuid()}";

		private int? _mainWorkspaceID;
		private readonly List<int> _createdWorkspaces = new List<int>();

		public RipInstallationTests()
		{
			var helper = new TestHelper();
			ApplicationManager = new RelativityApplicationManager(helper);
			ObjectManagerFactory = new RelativityObjectManagerFactory(helper);
		}

		[OneTimeSetUp]
		public async Task OneTimeSetUp()
		{
			_mainWorkspaceID = await CreateWorkspaceAsync(_mainWorkspaceName, _WORKSPACE_TEMPLATE_WITHOUT_RIP).ConfigureAwait(false);

			if (SharedVariables.UseIpRapFile())
			{
				await ApplicationManager.ImportRipToLibraryAsync().ConfigureAwait(false);
			}
			await ImportMyFirstProviderToLibraryAsync().ConfigureAwait(false);
			await ImportJsonLoaderToLibraryAsync().ConfigureAwait(false);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			foreach (int workspaceID in _createdWorkspaces)
			{
				Workspace.DeleteWorkspace(workspaceID);
			}
		}

		[IdentifiedTest("73fe58a5-087e-4071-8976-e54960dcbef1")]
		[Order(10)]
		public void ShouldInstallRipToWorkspaceCreatedFromTemplateWithoutRip()
		{
			// arrange
			int mainWorkspaceID = ValidateAndGetMainWorkspaceID();

			// act
			ApplicationManager.InstallApplicationFromLibrary(mainWorkspaceID, _RIP_GUID);

			// assert
			VerifyRipIsInstalledCorrectlyInWorkspace(mainWorkspaceID);
		}

		[IdentifiedTest("db7f5e5b-6495-4b2a-88b3-44e8a641a759")]
		[Order(20)]
		public async Task ShouldInstallRipToWorkspaceCreatedFromTemplateWithRip()
		{
			// arrange
			ValidateAndGetMainWorkspaceID();
			string workspaceName = "RipInstallTest-CreatedFromTemplateWithRip";

			// act
			int workspaceID = await CreateWorkspaceAsync(workspaceName, _mainWorkspaceName).ConfigureAwait(false);

			// assert
			VerifyRipIsInstalledCorrectlyInWorkspace(workspaceID);
		}

		[IdentifiedTest("a785ab16-a6f4-4e70-941c-d4fe1d026982")]
		[Order(30)]
		public void ShouldInstallMyFirstProviderToWorkspaceWithRipInstalled()
		{
			// arrange
			int mainWorkspaceID = ValidateAndGetMainWorkspaceID();

			// act
			ApplicationManager.InstallApplicationFromLibrary(mainWorkspaceID, CustomProvidersConstants.MY_FIRST_PROVIDER_GUID);

			// assert
			IEnumerable<string> expectedSourceProviders = _ripInternalSourceProviders.Concat(new[] { CustomProvidersConstants.MY_FIRST_PROVIDER_SOURCE_PROVIDER_NAME });
			VerifyRipIsInstalledCorrectlyInWorkspace(mainWorkspaceID, expectedSourceProviders);
		}

		[IdentifiedTest("cd114d25-c1b5-4549-a670-1f95a2b4d24a")]
		[Order(40)]
		public void ShouldInstallJsonLoaderToWorkspaceWithRipInstalled()
		{
			// arrange
			int mainWorkspaceID = ValidateAndGetMainWorkspaceID();

			// act
			ApplicationManager.InstallApplicationFromLibrary(mainWorkspaceID, _JSON_LOADER_GUID);

			// assert
			IEnumerable<string> expectedCustomSourceProviders = new[]
			{
				CustomProvidersConstants.MY_FIRST_PROVIDER_SOURCE_PROVIDER_NAME,
				_JSON_LOADER_SOURCE_PROVIDER_NAME
			};
			IEnumerable<string> expectedSourceProviders = _ripInternalSourceProviders.Concat(expectedCustomSourceProviders);
			VerifyRipIsInstalledCorrectlyInWorkspace(mainWorkspaceID, expectedSourceProviders);
		}

		[IdentifiedTest("846b2a03-48d5-4ca2-8707-17eb8313eecb")]
		[Order(50)]
		public async Task ShouldCopySourceProviderToWorkspaceCreatedFromTemplateWithCustomProviders()
		{
			// arrange
			ValidateAndGetMainWorkspaceID();
			string workspaceName = "RipInstallTest-CopySourceProviderToWorkspace";

			// act
			int workspaceID = await CreateWorkspaceAsync(workspaceName, _mainWorkspaceName).ConfigureAwait(false);

			// assert
			IEnumerable<string> expectedCustomSourceProviders = new[]
			{
				CustomProvidersConstants.MY_FIRST_PROVIDER_SOURCE_PROVIDER_NAME,
				_JSON_LOADER_SOURCE_PROVIDER_NAME
			};
			IEnumerable<string> expectedSourceProviders = _ripInternalSourceProviders.Concat(expectedCustomSourceProviders);
			VerifyRipIsInstalledCorrectlyInWorkspace(workspaceID, expectedSourceProviders);
		}

		private async Task<int> CreateWorkspaceAsync(string workspaceName, string templateName)
		{
			int workspaceID = await Workspace.CreateWorkspaceAsync(workspaceName, templateName).ConfigureAwait(false);
			_createdWorkspaces.Add(workspaceID);
			return workspaceID;
		}

		private int ValidateAndGetMainWorkspaceID()
		{
			return _mainWorkspaceID ?? throw new TestSetupException("Workspace with RIP installed is not available.");
		}

		private void VerifyRipIsInstalledCorrectlyInWorkspace(int workspaceID)
		{
			VerifyRipIsInstalledCorrectlyInWorkspace(workspaceID, _ripInternalSourceProviders);
		}

		private void VerifyRipIsInstalledCorrectlyInWorkspace(int workspaceID, IEnumerable<string> expectedSourceProviders)
		{
			var loggerMock = new Mock<IAPILog>
			{
				DefaultValue = DefaultValue.Mock
			};

			IRelativityObjectManager objectManager = ObjectManagerFactory.CreateRelativityObjectManager(workspaceID);
			var sourceProviderRepository = new SourceProviderRepository(objectManager);
			var destinationProviderRepository = new DestinationProviderRepository(loggerMock.Object, objectManager);

			IList<SourceProvider> sourceProviders = sourceProviderRepository.GetAll().ToList();
			IList<DestinationProvider> destinationProviders = destinationProviderRepository.GetAll().ToList();

			sourceProviders.Select(x => x.Name).Should().BeEquivalentTo(expectedSourceProviders);
			destinationProviders.Select(x => x.Name).Should().BeEquivalentTo(_ripInternalDestinationProviders);
		}

		private Task ImportMyFirstProviderToLibraryAsync()
		{
			string applicationFilePath = SharedVariables.MyFirstProviderRapFilePath;
			return ApplicationManager.ImportApplicationToLibraryAsync(CustomProvidersConstants.MY_FIRST_PROVIDER_APPLICATION_NAME, applicationFilePath);
		}

		private Task ImportJsonLoaderToLibraryAsync()
		{
			string applicationFilePath = SharedVariables.JsonLoaderRapFilePath;
			return ApplicationManager.ImportApplicationToLibraryAsync(_JSON_LOADER_APPLICATION_NAME, applicationFilePath);
		}
	}
}
