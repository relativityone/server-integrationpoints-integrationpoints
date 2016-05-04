using System;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Console = kCura.EventHandler.Console;
using ConsoleEventHandler = kCura.IntegrationPoints.EventHandlers.IntegrationPoints.ConsoleEventHandler;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Unit.IntegrationPoints
{
	[TestFixture]
	public class ConsoleEventHandlerTests
	{
		private ConsoleEventHandler testInstance;
		private IPermissionService _permissionService;
		private IManagerFactory _integrationPointManagerFactory;
		private IIntegrationPointManager _integrationPointManager;
		private IContextContainerFactory _contextContainerFactory;
		private IContextContainer _contextContainer;
		private IEHHelper _helper;

		private const int ARTIFACT_ID = 100300;
		private const int APPLICATION_ID = 100101;

		[SetUp]
		public void Setup()
		{
			_permissionService =Substitute.For<IPermissionService>();
			_integrationPointManagerFactory = Substitute.For<IManagerFactory>();
			_integrationPointManager = Substitute.For<IIntegrationPointManager>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_contextContainer = Substitute.For<IContextContainer>();
			_helper = Substitute.For<IEHHelper>();
			_helper.GetActiveCaseID().Returns(APPLICATION_ID);

			var activeArtifact = new Artifact(ARTIFACT_ID, null, 0, "", false, null);
			var application = new kCura.EventHandler.Application(APPLICATION_ID, "","");

			testInstance = new ConsoleEventHandler(_contextContainerFactory, _integrationPointManagerFactory)
			{
				ActiveArtifact = activeArtifact,
				Application = application,
				Helper = _helper,
				PermissionService = _permissionService
			};
		}

		[Test]
		public void GetConsoleTest()
		{
			// ARRANGE
			_permissionService.UserCanImport(APPLICATION_ID).Returns(true);
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
			_integrationPointManagerFactory.CreateIntegrationPointManager(_contextContainer).Returns(_integrationPointManager);

			var integrationPointDto = new kCura.IntegrationPoints.Contracts.Models.IntegrationPointDTO()
			{
				HasErrors = true
			};

			_integrationPointManager.Read(APPLICATION_ID, ARTIFACT_ID).Returns(integrationPointDto);

			// ACT
			Console console = testInstance.GetConsole(EventHandler.ConsoleEventHandler.PageEvent.Load);

			// ASSERT
			_contextContainerFactory.Received().CreateContextContainer(_helper);
			_permissionService.Received().UserCanImport(APPLICATION_ID);
			_integrationPointManagerFactory.Received().CreateIntegrationPointManager(_contextContainer);

			Assert.IsNotNull(console);
			Assert.AreEqual(3, console.ButtonList.Count);

			ConsoleButton runNowButton = console.ButtonList[0];
			Assert.AreEqual("Run Now", runNowButton.DisplayText);
			Assert.AreEqual(true, runNowButton.Enabled);
			Assert.AreEqual(false, runNowButton.RaisesPostBack);
			Assert.AreEqual($"IP.importNow({ARTIFACT_ID},{APPLICATION_ID})", runNowButton.OnClickEvent);

			ConsoleButton retryErrorsButton = console.ButtonList[1];
			Assert.AreEqual("Retry Errors", retryErrorsButton.DisplayText);
			Assert.AreEqual(true, retryErrorsButton.Enabled);
			Assert.AreEqual(false, retryErrorsButton.RaisesPostBack);
			Assert.AreEqual("alert('NOT IMPLEMENTED')", retryErrorsButton.OnClickEvent);

			ConsoleButton viewErrorsButtonLink = console.ButtonList[2];
			Assert.AreEqual("View Errors", viewErrorsButtonLink.DisplayText);
			Assert.AreEqual(true, viewErrorsButtonLink.Enabled);
			Assert.AreEqual(false, viewErrorsButtonLink.RaisesPostBack);
			Assert.AreEqual("alert('NOT IMPLEMENTED')", viewErrorsButtonLink.OnClickEvent);
		}
	}
}
