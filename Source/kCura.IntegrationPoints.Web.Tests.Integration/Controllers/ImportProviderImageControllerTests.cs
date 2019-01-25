using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Services.Protocols;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Tests.Integration.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Integration.Controllers
{
	[TestFixture]
	public class ImportProviderImageControllerTests
	{
		private int _workspaceId;
		private WindsorContainer _container;
		private const string _WORKSPACE_NAME = "Tests_ImportProviderImageController_RIP";
		private ImportProviderImageController _sut
		{
			get
			{
				ImportProviderImageController sut = _container.Resolve<ImportProviderImageController>();
				sut.Request = new HttpRequestMessage();
				sut.Configuration = new HttpConfiguration();
				return sut;
			}
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_workspaceId = Workspace.CreateWorkspace(_WORKSPACE_NAME, SourceProviderTemplate.WorkspaceTemplates.NEW_CASE_TEMPLATE);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			Workspace.DeleteWorkspace(_workspaceId);
		}

		[SetUp]
		public void SetUp()
		{
			_container = ContainerInstaller.CreateContainer();
		}

		[SmokeTest]
		public async Task ShouldReturnProperDefaultFileRepoWhenWorkspaceExists()
		{
			//arrange
			CancellationToken token = new CancellationTokenSource().Token;
			string actualDefaultFileRepo = Workspace.GetWorkspaceDto(_workspaceId).DefaultFileLocation.Name;

			// act
			IHttpActionResult actionResult = _sut.GetDefaultFileRepo(_workspaceId);
			HttpResponseMessage response = await actionResult.ExecuteAsync(token).ConfigureAwait(false);
			string responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			string resultDefaultFileRepo = JsonConvert.DeserializeObject<string>(responseText);

			// assert
			response.IsSuccessStatusCode.Should().BeTrue();
			resultDefaultFileRepo.Should().Be(actualDefaultFileRepo);
		}

		[SmokeTest]
		public void ShouldThrowWhenTryingToGetDefaultFileRepoWithZeroWorkspaceId()
		{
			//arrange
			const int zeroWorkspaceId = 0;

			// act
			Action action = () => _sut.GetDefaultFileRepo(zeroWorkspaceId);

			// assert
			action.ShouldThrow<SoapException>()
				.WithMessage($"Could not retrieve ApplicationID #{zeroWorkspaceId}.");
		}

		[SmokeTest]
		public void ShouldThrowWhenTryingToGetDefaultFileRepoWithNegativeWorkspaceId()
		{
			//arrange
			const int negativeWorkspaceId = -100;

			// act
			Action action = () => _sut.GetDefaultFileRepo(negativeWorkspaceId);

			// assert
			action.ShouldThrow<SoapException>()
				.WithMessage($"Could not retrieve ApplicationID #{negativeWorkspaceId}.");
		}

	}
}
