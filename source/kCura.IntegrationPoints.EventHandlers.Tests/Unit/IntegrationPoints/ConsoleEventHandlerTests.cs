using System;
using System.Collections.Generic;
using System.Linq;
using kCura.EventHandler;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Managers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
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

		[TestCase(true, true, false)]
		[TestCase(true, true, true)]
		[TestCase(false, true, true)]
		[TestCase(true, false, false)]
		[TestCase(true, false, true)]
		[TestCase(false, false, true)]
		public void GetConsole_GoldFlow(bool isRelativitySourceProvider, bool hasRunPermissions, bool hasViewErrorsPermissions)
		{
			// ARRANGE
			var integrationPointDto = new Contracts.Models.IntegrationPointDTO()
			{
				HasErrors = true,
				SourceProvider = 8392
			};

			var permissionCheck = new PermissionCheckDTO()
			{
				Success = hasRunPermissions,
				ErrorMessages = hasRunPermissions ? null : new[] { "GOBBLYGOOK!" }
			};

			string[] viewErrorMessages = new[] { Core.Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_NO_VIEW };
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
					RunNowButtonEnabled = hasRunPermissions,
					RetryErrorsButtonEnabled = hasRunPermissions,
					ViewErrorsLinkEnabled = hasViewErrorsPermissions
				};
				OnClickEventDTO onClickEvents = new OnClickEventDTO()
				{
					RunNowOnClickEvent = hasRunPermissions ? $"IP.importNow({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty,
					RetryErrorsOnClickEvent = hasRunPermissions ? $"IP.retryJob({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty,
					ViewErrorsOnClickEvent = integrationPointDto.HasErrors.Value && hasViewErrorsPermissions? "Really long string" : String.Empty,
				};

				_integrationPointManager.UserHasPermissionToViewErrors(_APPLICATION_ID).Returns(
					new PermissionCheckDTO()
					{
						Success	= hasViewErrorsPermissions,
						ErrorMessages = hasViewErrorsPermissions ? null : viewErrorMessages
					});

				_queueManager.HasJobsExecutingOrInQueue(_APPLICATION_ID, _ARTIFACT_ID).Returns(hasJobsExecutingOrInQueue);

				_stateManager.GetButtonState(_APPLICATION_ID, _ARTIFACT_ID, hasJobsExecutingOrInQueue, integrationPointDto.HasErrors.Value, hasViewErrorsPermissions)
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
			_integrationPointManager.Received(isRelativitySourceProvider ? 1 : 0).UserHasPermissionToViewErrors(_APPLICATION_ID);

			Assert.IsNotNull(console);
			if (hasViewErrorsPermissions)
			{
				int buttonCount = isRelativitySourceProvider ? 3 : 1;
				Assert.AreEqual(buttonCount, console.ButtonList.Count, $"There should be {buttonCount} buttons on the console");
			}
			else
			{
				int buttonCount = isRelativitySourceProvider ? 2 : 1;
				Assert.AreEqual(buttonCount, console.ButtonList.Count, $"There should be {buttonCount} buttons on the console");
			}

			int buttonIndex = 0;
			ConsoleButton runNowButton = console.ButtonList[buttonIndex++];
			Assert.AreEqual("Run Now", runNowButton.DisplayText);
			Assert.AreEqual(false, runNowButton.RaisesPostBack);

			if (isRelativitySourceProvider)
			{
				Assert.AreEqual(hasRunPermissions ? $"IP.importNow({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty,
					runNowButton.OnClickEvent);
				Assert.AreEqual(buttonStates.RunNowButtonEnabled, runNowButton.Enabled);
				Assert.AreEqual(false, runNowButton.RaisesPostBack);
				Assert.AreEqual(hasRunPermissions ? $"IP.importNow({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty,
					runNowButton.OnClickEvent);

				ConsoleButton retryErrorsButton = console.ButtonList[buttonIndex++];
				Assert.AreEqual("Retry Errors", retryErrorsButton.DisplayText);
				Assert.AreEqual(buttonStates.RetryErrorsButtonEnabled, retryErrorsButton.Enabled);
				Assert.AreEqual(false, retryErrorsButton.RaisesPostBack);
				Assert.AreEqual(hasRunPermissions ? $"IP.retryJob({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty,
					retryErrorsButton.OnClickEvent);

				if (hasViewErrorsPermissions)
				{
					ConsoleButton viewErrorsButtonLink = console.ButtonList[buttonIndex++];
					Assert.AreEqual("View Errors", viewErrorsButtonLink.DisplayText);
					Assert.AreEqual(buttonStates.ViewErrorsLinkEnabled, viewErrorsButtonLink.Enabled);
					Assert.AreEqual(false, viewErrorsButtonLink.RaisesPostBack);
					Assert.AreEqual("Really long string", viewErrorsButtonLink.OnClickEvent);
				}
			}
			else
			{
				Assert.IsTrue(runNowButton.Enabled);
				Assert.AreEqual($"IP.importNow({_ARTIFACT_ID},{_APPLICATION_ID})", runNowButton.OnClickEvent);
			}

			if (hasRunPermissions && hasViewErrorsPermissions)
			{
				Assert.AreEqual(0, console.ScriptBlocks.Count);
			}
			else
			{
				string expectedKey = "IPConsoleErrorDisplayScript".ToLower();
				string expectedScript = "<script type='text/javascript'>"
								+ "$(document).ready(function () {"
								+ "IP.message.error.raise(\""
								+ String.Join("<br/>", permissionCheck.ErrorMessages ?? viewErrorMessages)
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
