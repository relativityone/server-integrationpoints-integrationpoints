using System;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.JsonLoader.FunctionalTests
{
	[TestFixture]
	public class JsonLoaderInstallationTests
	{
		private const string _JSON_LOADER_APP_NAME = "JsonLoader";
		private const string _RIP_APP_NAME = "Integration Points";

		private ILibraryApplicationService _libraryApplicationService;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			_libraryApplicationService = SetUpFixture
				.Relativity
				.Resolve<ILibraryApplicationService>();
		}

		[Category("TestType.CI")]
		[IdentifiedTest("197e67c7-4841-44e5-8169-fa51f2a157b6")]
		public void InstallRelativityApplicationToGivenWorkspace_ShouldInstallJsonLoaderRapFile()
		{
			//arrange
			string jsonLoaderRapPath = $"{TestContext.Parameters.Get("RAPDirectory")}\\JsonLoader.rap";

			LibraryApplication ripApplicationResponse = _libraryApplicationService.Get(_RIP_APP_NAME);

			_libraryApplicationService.InstallToWorkspace(
				SetUpFixture.Workspace.ArtifactID,
				ripApplicationResponse.ArtifactID
			);

			_libraryApplicationService.InstallToLibrary(jsonLoaderRapPath);

			LibraryApplication jsonLoaderApplicationResponse = _libraryApplicationService
				.Get(_JSON_LOADER_APP_NAME);

			//act
			Action action = () => _libraryApplicationService.InstallToWorkspace(
				SetUpFixture.Workspace.ArtifactID,
				jsonLoaderApplicationResponse.ArtifactID
			);

			//assert
			action.Should().NotThrow();
		}
	}
}