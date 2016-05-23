using System;
using System.Linq;
using kCura.EventHandler;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Helpers;
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

		private IManagerFactory _managerFactory;
		private IIntegrationPointManager _integrationPointManager;
		private IContextContainerFactory _contextContainerFactory;
		private IHelperClassFactory _helperClassFactory;
		private IContextContainer _contextContainer;
		private IEHHelper _helper;
		private IStateManager _stateManager;
		private IQueueManager _queueManager;
		private IOnClickEventConstructor _onClickEventHelper;

		private ConsoleEventHandler _instance;

		[SetUp]
		public void Setup()
		{
			_managerFactory = Substitute.For<IManagerFactory>();
			_integrationPointManager = Substitute.For<IIntegrationPointManager>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_helperClassFactory = Substitute.For<IHelperClassFactory>();
			_contextContainer = Substitute.For<IContextContainer>();
			_helper = Substitute.For<IEHHelper>();
			_helper.GetActiveCaseID().Returns(_APPLICATION_ID);
			_stateManager = Substitute.For<IStateManager>();
			_queueManager = Substitute.For<IQueueManager>();
			_onClickEventHelper = Substitute.For<IOnClickEventConstructor>();

			var activeArtifact = new Artifact(_ARTIFACT_ID, null, 0, "", false, null);
			var application = new Application(_APPLICATION_ID, "", "");

			_instance = new EventHandlers.IntegrationPoints.ConsoleEventHandler(_contextContainerFactory, _managerFactory, _helperClassFactory)
			{
				ActiveArtifact = activeArtifact,
				Application = application,
				Helper = _helper
			};
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public void GetConsole_GoldFlow(bool isRelativitySourceProvider, bool hasPermissions)
		{
			// ARRANGE
			var integrationPointDto = new Contracts.Models.IntegrationPointDTO()
			{
				HasErrors = true,
				SourceProvider = 8392
			};

			var permissionCheck = new PermissionCheckDTO()
			{
				Success = hasPermissions,
				ErrorMessages = hasPermissions ? null : new[] { "GOBBLYGOOK!" }
			};
			Core.Constants.SourceProvider sourceProvider = isRelativitySourceProvider ? Core.Constants.SourceProvider.Relativity : Core.Constants.SourceProvider.Other;
			_integrationPointManager.UserHasPermissionToRunJob(Arg.Is(_APPLICATION_ID), Arg.Is(integrationPointDto), Arg.Is(sourceProvider)).Returns(permissionCheck);
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
			_managerFactory.CreateIntegrationPointManager(_contextContainer).Returns(_integrationPointManager);
			_managerFactory.CreateStateManager().Returns(_stateManager);
			_managerFactory.CreateQueueManager(_contextContainer).Returns(_queueManager);

			_helperClassFactory.CreateOnClickEventHelper(_managerFactory, _contextContainer).Returns(_onClickEventHelper);

			_integrationPointManager.Read(_APPLICATION_ID, _ARTIFACT_ID).Returns(integrationPointDto);
			_integrationPointManager.GetSourceProvider(Arg.Is(_APPLICATION_ID), Arg.Is(integrationPointDto))
				.Returns(sourceProvider);

			ButtonStateDTO buttonStates = null;
			if (isRelativitySourceProvider)
			{
				bool hasJobsExecutingOrInQueue = false;
				buttonStates = new ButtonStateDTO()
				{
					RunNowButtonEnabled = true & hasPermissions,
					RetryErrorsButtonEnabled = true & hasPermissions,
					ViewErrorsLinkEnabled = true
				};
				OnClickEventDTO onClickEvents = new OnClickEventDTO()
				{
					RunNowOnClickEvent = hasPermissions ? $"IP.importNow({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty,
					RetryErrorsOnClickEvent = hasPermissions ? $"IP.retryJob({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty,
					ViewErrorsOnClickEvent = integrationPointDto.HasErrors.Value ? "Really long string" : String.Empty
				};

				_queueManager.HasJobsExecutingOrInQueue(_APPLICATION_ID, _ARTIFACT_ID).Returns(hasJobsExecutingOrInQueue);

				_stateManager.GetButtonState(_APPLICATION_ID, _ARTIFACT_ID, hasJobsExecutingOrInQueue, integrationPointDto.HasErrors.Value)
					.Returns(buttonStates);
				_onClickEventHelper.GetOnClickEventsForRelativityProvider(_APPLICATION_ID, _ARTIFACT_ID, buttonStates)
					.Returns(onClickEvents);
			}

			// ACT
			kCura.EventHandler.Console console = _instance.GetConsole(ConsoleEventHandler.PageEvent.Load);

			// ASSERT
			_contextContainerFactory.Received().CreateContextContainer(_helper);
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(Arg.Is(_APPLICATION_ID), Arg.Is(integrationPointDto), Arg.Is(sourceProvider));
			_managerFactory.Received().CreateIntegrationPointManager(_contextContainer);

			Assert.IsNotNull(console);
			Assert.AreEqual(isRelativitySourceProvider ? 3 : 1, console.ButtonList.Count);

			int buttonIndex = 0;
			ConsoleButton runNowButton = console.ButtonList[buttonIndex++];
			Assert.AreEqual("Run Now", runNowButton.DisplayText);
			Assert.AreEqual(hasPermissions, runNowButton.Enabled);
			Assert.AreEqual(false, runNowButton.RaisesPostBack);
			Assert.AreEqual(hasPermissions ? $"IP.importNow({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty, runNowButton.OnClickEvent);

			if (isRelativitySourceProvider)
			{
				buttonIndex = 0;
				ConsoleButton runNowButtonRelativityProvider = console.ButtonList[buttonIndex++];
				Assert.AreEqual("Run Now", runNowButton.DisplayText);
				Assert.AreEqual(buttonStates.RunNowButtonEnabled, runNowButton.Enabled);
				Assert.AreEqual(false, runNowButton.RaisesPostBack);
				Assert.AreEqual(hasPermissions ? $"IP.importNow({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty, runNowButton.OnClickEvent);

				ConsoleButton retryErrorsButton = console.ButtonList[buttonIndex++];
				Assert.AreEqual("Retry Errors", retryErrorsButton.DisplayText);
				Assert.AreEqual(buttonStates.RetryErrorsButtonEnabled, retryErrorsButton.Enabled);
				Assert.AreEqual(false, retryErrorsButton.RaisesPostBack);
				Assert.AreEqual(hasPermissions ? $"IP.retryJob({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty, retryErrorsButton.OnClickEvent);

				ConsoleButton viewErrorsButtonLink = console.ButtonList[buttonIndex++];
				Assert.AreEqual("View Errors", viewErrorsButtonLink.DisplayText);
				Assert.AreEqual(buttonStates.ViewErrorsLinkEnabled, viewErrorsButtonLink.Enabled);
				Assert.AreEqual(false, viewErrorsButtonLink.RaisesPostBack);
				Assert.AreEqual("Really long string", viewErrorsButtonLink.OnClickEvent);

				if (hasPermissions)
				{
					Assert.AreEqual(0, console.ScriptBlocks.Count);
				}
				else
				{
					string expectedKey = "IPConsoleErrorDisplayScript".ToLower();
					string expectedScript = "<script type='text/javascript'>"
									+ "$(document).ready(function () {"
									+ "IP.message.error.raise(\""
									+ String.Join("<br/>", permissionCheck.ErrorMessages)
									+ "\", $(\".cardContainer\"));"
									+ "});"
									+ "</script>";
					Assert.AreEqual(1, console.ScriptBlocks.Count);
					Assert.AreEqual(expectedKey, console.ScriptBlocks.First().Key);	
					Assert.AreEqual(expectedScript, console.ScriptBlocks.First().Script);	
				}
			}
		}
	}
}
