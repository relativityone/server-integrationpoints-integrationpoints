using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Services.Protocols;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Tests.Integration.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Web.Tests.Integration.Controllers
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[NotWorkingOnTrident]
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
			_workspaceId = Workspace.CreateWorkspaceAsync(_WORKSPACE_NAME)
				.GetAwaiter().GetResult().ArtifactID;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			Workspace.DeleteWorkspaceAsync(_workspaceId).GetAwaiter().GetResult();
		}

		[SetUp]
		public void SetUp()
		{
			_container = ContainerInstaller.CreateContainer();
		}

		[IdentifiedTest("d8f1bfa1-e21d-42e9-9d7a-d21d27396923")]
		[SmokeTest]
		public async Task ShouldReturnProperDefaultFileRepoWhenWorkspaceExists()
		{
			//arrange
			CancellationToken token = new CancellationTokenSource().Token;


			var defaultWorkspaceFileShareServer =
				await Workspace.GetDefaultWorkspaceFileShareServerIDAsync(_workspaceId).ConfigureAwait(false);

			// act
			IHttpActionResult actionResult = _sut.GetDefaultFileRepo(_workspaceId);
			HttpResponseMessage response = await actionResult.ExecuteAsync(token).ConfigureAwait(false);
			string responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			string resultDefaultFileRepo = JsonConvert.DeserializeObject<string>(responseText);

			// assert
			response.IsSuccessStatusCode.Should().BeTrue();
			resultDefaultFileRepo.Should().Be(defaultWorkspaceFileShareServer.Name);
		}

		[IdentifiedTest("b93abcde-8826-449b-8a0a-671e090285c1")]
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

		[IdentifiedTest("06ac6bf5-36a5-4c8f-a6f8-718e964fba25")]
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
