using kCura.EventHandler;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Unit.IntegrationPoints
{
	[TestFixture]
	public class ConsoleEventHandlerTests
	{
		private const int _ARTIFACT_ID = 100300;
		private const int _APPLICATION_ID = 100101;

		private IManagerFactory _integrationPointManagerFactory;
		private IIntegrationPointManager _integrationPointManager;
		private IContextContainerFactory _contextContainerFactory;
		private IContextContainer _contextContainer;
		private IEHHelper _helper;

		private ConsoleEventHandler _instance;

		[SetUp]
		public void Setup()
		{
			_integrationPointManagerFactory = Substitute.For<IManagerFactory>();
			_integrationPointManager = Substitute.For<IIntegrationPointManager>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_contextContainer = Substitute.For<IContextContainer>();
			_helper = Substitute.For<IEHHelper>();
			_helper.GetActiveCaseID().Returns(_APPLICATION_ID);

			var activeArtifact = new Artifact(_ARTIFACT_ID, null, 0, "", false, null);
			var application = new Application(_APPLICATION_ID, "", "");

			_instance = new EventHandlers.IntegrationPoints.ConsoleEventHandler(_contextContainerFactory, _integrationPointManagerFactory)
			{
				ActiveArtifact = activeArtifact,
				Application = application,
				Helper = _helper
			};
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void GetConsole_GoldFlow(bool isRelativitySourceProvider)
		{
			// ARRANGE
			_integrationPointManager.UserHasPermissions(_APPLICATION_ID).Returns(true);
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
			_integrationPointManagerFactory.CreateIntegrationPointManager(_contextContainer).Returns(_integrationPointManager);

			var integrationPointDto = new Contracts.Models.IntegrationPointDTO()
			{
				HasErrors = true,
				SourceProvider = 8392
			};

			_integrationPointManager.Read(_APPLICATION_ID, _ARTIFACT_ID).Returns(integrationPointDto);
			_integrationPointManager.IntegrationPointTypeIsRetriable(Arg.Is(_APPLICATION_ID), Arg.Is(integrationPointDto))
				.Returns(isRelativitySourceProvider);

			// ACT
			Console console = _instance.GetConsole(ConsoleEventHandler.PageEvent.Load);

			// ASSERT
			_contextContainerFactory.Received().CreateContextContainer(_helper);
			_integrationPointManager.Received(1).UserHasPermissions(_APPLICATION_ID);
			_integrationPointManagerFactory.Received().CreateIntegrationPointManager(_contextContainer);

			Assert.IsNotNull(console);
			Assert.AreEqual(isRelativitySourceProvider ? 3 : 2, console.ButtonList.Count);

			int buttonIndex = 0;
			ConsoleButton runNowButton = console.ButtonList[buttonIndex++];
			Assert.AreEqual("Run Now", runNowButton.DisplayText);
			Assert.AreEqual(true, runNowButton.Enabled);
			Assert.AreEqual(false, runNowButton.RaisesPostBack);
			Assert.AreEqual($"IP.importNow({_ARTIFACT_ID},{_APPLICATION_ID})", runNowButton.OnClickEvent);

			if (isRelativitySourceProvider)
			{
				ConsoleButton retryErrorsButton = console.ButtonList[buttonIndex++];
				Assert.AreEqual("Retry Errors", retryErrorsButton.DisplayText);
				Assert.AreEqual(true, retryErrorsButton.Enabled);
				Assert.AreEqual(false, retryErrorsButton.RaisesPostBack);
				Assert.AreEqual($"IP.retryJob({_ARTIFACT_ID},{_APPLICATION_ID})", retryErrorsButton.OnClickEvent);
			}

			ConsoleButton viewErrorsButtonLink = console.ButtonList[buttonIndex++];
			Assert.AreEqual("View Errors", viewErrorsButtonLink.DisplayText);
			Assert.AreEqual(true, viewErrorsButtonLink.Enabled);
			Assert.AreEqual(false, viewErrorsButtonLink.RaisesPostBack);
			Assert.AreEqual("alert('NOT IMPLEMENTED')", viewErrorsButtonLink.OnClickEvent);
		}
	}
}
