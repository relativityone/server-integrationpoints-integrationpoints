using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Unit.Controllers
{
	[TestFixture]
	public class ImportNowControllerTests
	{
		private ImportNowController _controller;
		private IJobManager _jobManager;
		private IPermissionService _permissionService;
		private ImportNowController.IIntegrationPointRdoAdaptor _rdoAdaptor;
		private ImportNowController.Payload _payload;

		[SetUp]
		public void Setup()
		{
			_payload = new ImportNowController.Payload()
			{
				AppId = 1,
				ArtifactId = 123
			};

			_jobManager = Substitute.For<IJobManager>();
			_permissionService = Substitute.For<IPermissionService>();
			_rdoAdaptor = Substitute.For<ImportNowController.IIntegrationPointRdoAdaptor>();
			_controller = new ImportNowController(_jobManager, _permissionService, _rdoAdaptor);
			_controller.Request = new HttpRequestMessage();
			_controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
		}

		[Test]
		public void UserDoesNotHaveAPermissionToPushToAnotherWorkspace()
		{
			const string ExpectedErrorMessage = @"""You do not have permissions to the workspace that you are pushing documents to. Please contact your system administrator.""";

			_rdoAdaptor.SourceProviderIdentifier.Returns(DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID);
			_rdoAdaptor.SourceConfiguration.Returns("{TargetWorkspaceArtifactId : 123}");
			_permissionService.UserCanImport(123).Returns(false);

			HttpResponseMessage response = _controller.Post(_payload);

			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.AreEqual(ExpectedErrorMessage, response.Content.ReadAsStringAsync().Result);
		}


		[Test]
		public void UserDoesHaveAPermissionToPushToAnotherWorkspace()
		{
			_rdoAdaptor.SourceProviderIdentifier.Returns(DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID);
			_rdoAdaptor.SourceConfiguration.Returns("{TargetWorkspaceArtifactId : 123}");
			_permissionService.UserCanImport(123).Returns(true);

			HttpResponseMessage response = _controller.Post(_payload);

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[Test]
		public void SomethingWrongWithRsapiCalls()
		{
			const string ExpectedErrorMessage = @"""ABC : 123,456""";

			AggregateException exceptionToBeThrown = new AggregateException("ABC",
				new [] {new AccessViolationException("123"), new Exception("456")});

			_rdoAdaptor.SourceProviderIdentifier.Throws(exceptionToBeThrown);
			HttpResponseMessage response = _controller.Post(_payload);

			
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.AreEqual(ExpectedErrorMessage, response.Content.ReadAsStringAsync().Result);
		}

		[Test]
		public void NonRelativityProviderCall()
		{
			_rdoAdaptor.SourceProviderIdentifier.Returns( Guid.NewGuid().ToString() );

			HttpResponseMessage response = _controller.Post(_payload);

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}
	}
}