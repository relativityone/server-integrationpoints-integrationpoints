using System;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.MyFirstProvider.Provider.FunctionalTests
{
	[TestFixture]
	[Category("TestType.CI")]
	public class MyFirstProviderTests
	{
		private const string _MYFIRSTPROVIDER_APP_NAME = "MyFirstProvider";
		private const string _RIP_APP_NAME = "Integration Points";

		private ILibraryApplicationService _libraryApplicationService;

		[OneTimeSetUp]
		public void SetUpOnce()
		{
			_libraryApplicationService = SetUpFixture
				.Relativity
				.Resolve<ILibraryApplicationService>();
		}

		[IdentifiedTest("ceb4a72f-fb34-427f-a03d-5bdbdf571aed")]
		public void MyFirstProvider_ShouldInstallProperlyIntoWorkspace()
		{
			//arrange
			string jsonLoaderRapPath = $"{TestContext.Parameters.Get("RAPDirectory")}\\MyFirstProvider.rap";

			LibraryApplication ripApplicationResponse = _libraryApplicationService.Get(_RIP_APP_NAME);

			_libraryApplicationService.InstallToWorkspace(
				SetUpFixture.Workspace.ArtifactID,
				ripApplicationResponse.ArtifactID
			);

			_libraryApplicationService.InstallToLibrary(jsonLoaderRapPath);

			LibraryApplication jsonLoaderApplicationResponse = _libraryApplicationService
				.Get(_MYFIRSTPROVIDER_APP_NAME);

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
