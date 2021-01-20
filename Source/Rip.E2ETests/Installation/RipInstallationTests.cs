﻿using FluentAssertions;
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
using Relativity.Services.Workspace;
using Relativity.Testing.Identification;

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
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			foreach (int workspaceID in _createdWorkspaces)
			{
				Workspace.DeleteWorkspaceAsync(workspaceID).GetAwaiter().GetResult();
			}
		}

		[IdentifiedTest("73fe58a5-087e-4071-8976-e54960dcbef1")]
		[Order(10)]
		public async Task ShouldInstallRipToWorkspaceCreatedFromTemplateWithoutRip()
		{
			// arrange
			int mainWorkspaceID = ValidateAndGetMainWorkspaceID();

			// act

			await ApplicationManager.InstallRipFromLibraryAsync(mainWorkspaceID).ConfigureAwait(false);

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

		private async Task<int> CreateWorkspaceAsync(string workspaceName, string templateName)
		{
			WorkspaceRef workspace = await Workspace.CreateWorkspaceAsync(workspaceName, templateName).ConfigureAwait(false);
			_createdWorkspaces.Add(workspace.ArtifactID);
			return workspace.ArtifactID;
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
	}
}
