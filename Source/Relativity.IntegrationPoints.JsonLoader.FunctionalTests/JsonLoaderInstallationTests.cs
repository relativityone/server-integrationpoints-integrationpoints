using System;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Interfaces.LibraryApplication.Models;
using Relativity.Testing.Framework.Orchestrators;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.JsonLoader.FunctionalTests
{
	[TestFixture]
	public class JsonLoaderInstallationTests
	{
		private IOrchestrateRelativityApplications _applicationOrchestrator;

		private const string _JSON_LOADER_APP_NAME = "JsonLoader";
		private const string _RIP_APP_NAME = "Integration Points";

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			_applicationOrchestrator = SetUpFixture
				.ApiComponent
				.OrchestratorFactory
				.Create<IOrchestrateRelativityApplications>();
		}

		[Category("TestType.CI")]
		[IdentifiedTest("197e67c7-4841-44e5-8169-fa51f2a157b6")]
		public void InstallRelativityApplicationToGivenWorkspace_ShouldInstallJsonLoaderRapFile()
		{
			//arrange
			string jsonLoaderRapPath = $"{TestContext.Parameters.Get("RAPDirectory")}\\JsonLoader.rap";

			LibraryApplicationResponse ripApplicationResponse = _applicationOrchestrator
				.GetLibraryApplicationByName(_RIP_APP_NAME);

			_applicationOrchestrator.InstallRelativityApplicationToGivenWorkspaceFromLibrary(
				new LibraryApplication
				{
					ArtifactID = ripApplicationResponse.ArtifactID
				},
				SetUpFixture.Workspace
			);

			_applicationOrchestrator.InstallRelativityApplicationToLibrary(jsonLoaderRapPath);

			LibraryApplicationResponse jsonLoaderApplicationResponse = _applicationOrchestrator
				.GetLibraryApplicationByName(_JSON_LOADER_APP_NAME);

			//act
			Action action = () => _applicationOrchestrator.InstallRelativityApplicationToGivenWorkspaceFromLibrary(
				new LibraryApplication
				{
					ArtifactID = jsonLoaderApplicationResponse.ArtifactID
				},
				SetUpFixture.Workspace
			);

			//assert
			action.Should().NotThrow();
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			_applicationOrchestrator.Dispose();
		}
	}
}