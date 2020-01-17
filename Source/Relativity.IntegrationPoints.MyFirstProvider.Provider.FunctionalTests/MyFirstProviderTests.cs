using System;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Interfaces.LibraryApplication.Models;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Orchestrators;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.MyFirstProvider.Provider.FunctionalTests
{
	[TestFixture]
	[Category("TestType.CI")]
	public class MyFirstProviderTests
    {
	    private const string _MYFIRSTPROVIDER_APP_NAME = "MyFirstProvider";
	    private const string _RIP_APP_NAME = "Integration Points";

		private IOrchestrateRelativityApplications _applicationOrchestrator;

		[OneTimeSetUp]
	    public void SetUpOnce()
	    {
		    _applicationOrchestrator = SetUpFixture
				.ApiComponent
				.OrchestratorFactory
				.Create<IOrchestrateRelativityApplications>();
		}

	    [OneTimeTearDown]
	    public void OneTimeTearDown()
	    {
		    _applicationOrchestrator.Dispose();
	    }

		[IdentifiedTest("ceb4a72f-fb34-427f-a03d-5bdbdf571aed")]
	    public void MyFirstProvider_ShouldInstallProperlyIntoWorkspace()
	    {
			//arrange
			string jsonLoaderRapPath = $"{TestContext.Parameters.Get("RAPDirectory")}\\MyFirstProvider.rap";

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
				.GetLibraryApplicationByName(_MYFIRSTPROVIDER_APP_NAME);

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
	}
}
