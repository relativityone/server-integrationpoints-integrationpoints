using System;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
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
		private IErrorManager _errorManager;
		private IJobHistoryManager _jobHistoryManager;

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
			_errorManager = Substitute.For<IErrorManager>();
			_jobHistoryManager = Substitute.For<IJobHistoryManager>();

			var activeArtifact = new Artifact(_ARTIFACT_ID, null, 0, "", false, null);
			var application = new Application(_APPLICATION_ID, "", "");

			_instance = new EventHandlers.IntegrationPoints.ConsoleEventHandler(_contextContainerFactory, _managerFactory, _helperClassFactory)
			{
				ActiveArtifact = activeArtifact,
				Application = application,
				Helper = _helper
			};
		}

		[TestCase(true, false, false)]
		[TestCase(true, true, false)]
		[TestCase(true, true, false)]
		[TestCase(false, false, false)]
		[TestCase(false, true, false)]
		[TestCase(false, true, false)]
		[TestCase(true, false, true)]
		[TestCase(true, true, true)]
		[TestCase(true, true, true)]
		[TestCase(false, false, true)]
		[TestCase(false, true, true)]
		[TestCase(false, true, true)]
		public void GetConsole_RelativityProvider_GoldFlow(bool hasRunPermissions, bool hasViewErrorsPermissions, bool hasStoppableJobs)
		{
			// ARRANGE
			var integrationPointDto = new IntegrationPointDTO()
			{
				HasErrors = true,
				SourceProvider = 8392
			};

			string[] viewErrorMessages = new[] { Core.Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_NO_VIEW };
			Core.Constants.SourceProvider sourceProvider = Core.Constants.SourceProvider.Relativity;
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
			_managerFactory.CreateIntegrationPointManager(_contextContainer).Returns(_integrationPointManager);
			_managerFactory.CreateStateManager().Returns(_stateManager);
			_managerFactory.CreateQueueManager(_contextContainer).Returns(_queueManager);
			_managerFactory.CreateJobHistoryManager(_contextContainer).Returns(_jobHistoryManager);

			_helperClassFactory.CreateOnClickEventHelper(_managerFactory, _contextContainer).Returns(_onClickEventHelper);

			_integrationPointManager.Read(_APPLICATION_ID, _ARTIFACT_ID).Returns(integrationPointDto);
			_integrationPointManager.GetSourceProvider(Arg.Is(_APPLICATION_ID), Arg.Is(integrationPointDto))
				.Returns(sourceProvider);

			_jobHistoryManager.GetIntegrationPointHasStoppableJobs(_APPLICATION_ID, _ARTIFACT_ID).Returns(hasStoppableJobs);

			if (!hasRunPermissions || !hasViewErrorsPermissions)
			{
				_managerFactory.CreateErrorManager(_contextContainer).Returns(_errorManager);
			}

			RelativityButtonStateDTO buttonStates = null;
			RelativityOnClickEventDTO onClickEvents = null;

			buttonStates = new RelativityButtonStateDTO()
			{
				RunNowButtonEnabled = hasRunPermissions,
				RetryErrorsButtonEnabled = hasRunPermissions,
				ViewErrorsLinkEnabled = hasViewErrorsPermissions,
				StopButtonEnabled = hasStoppableJobs
			};

			bool hasJobsExecutingOrInQueue = false;

			_integrationPointManager.UserHasPermissionToViewErrors(_APPLICATION_ID).Returns(
				new PermissionCheckDTO()
				{
					Success = hasViewErrorsPermissions,
					ErrorMessages = hasViewErrorsPermissions ? null : viewErrorMessages
				});

			_queueManager.HasJobsExecutingOrInQueue(_APPLICATION_ID, _ARTIFACT_ID).Returns(hasJobsExecutingOrInQueue);

			_stateManager.GetRelativityProviderButtonState(
					hasJobsExecutingOrInQueue,
					integrationPointDto.HasErrors.Value, 
					hasViewErrorsPermissions, 
					hasStoppableJobs)
				.Returns(buttonStates);

			onClickEvents = new RelativityOnClickEventDTO()
			{
				RunNowOnClickEvent = hasRunPermissions ? $"IP.importNow({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty,
				RetryErrorsOnClickEvent = hasRunPermissions ? $"IP.retryJob({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty,
				ViewErrorsOnClickEvent =
					integrationPointDto.HasErrors.Value && hasViewErrorsPermissions ? "Really long string" : String.Empty,
				StopOnClickEvent = hasStoppableJobs ? $"IP.importNow({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty
			};

			_onClickEventHelper.GetOnClickEventsForRelativityProvider(_APPLICATION_ID, _ARTIFACT_ID, buttonStates)
				.Returns(onClickEvents);

			// ACT
			kCura.EventHandler.Console console = _instance.GetConsole(ConsoleEventHandler.PageEvent.Load);

			// ASSERT
			_contextContainerFactory.Received().CreateContextContainer(_helper);
			_managerFactory.Received().CreateIntegrationPointManager(_contextContainer);
			_integrationPointManager.Received(1).UserHasPermissionToViewErrors(_APPLICATION_ID);

			Assert.IsNotNull(console);
			if (hasViewErrorsPermissions)
			{
				int buttonCount = 4;
				Assert.AreEqual(buttonCount, console.ButtonList.Count, $"There should be {buttonCount} buttons on the console");
			}
			else
			{
				int buttonCount = 3;
				Assert.AreEqual(buttonCount, console.ButtonList.Count, $"There should be {buttonCount} buttons on the console");
			}

			int buttonIndex = 0;
			ConsoleButton runNowButton = console.ButtonList[buttonIndex++];
			Assert.AreEqual("Run Now", runNowButton.DisplayText);
			Assert.AreEqual(false, runNowButton.RaisesPostBack);

			_jobHistoryManager.Received(1).GetIntegrationPointHasStoppableJobs(_APPLICATION_ID, _ARTIFACT_ID);

				Assert.AreEqual(hasRunPermissions ? $"IP.importNow({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty,
					runNowButton.OnClickEvent);
				Assert.AreEqual(buttonStates.RunNowButtonEnabled, runNowButton.Enabled);
				Assert.AreEqual(false, runNowButton.RaisesPostBack);
				Assert.AreEqual(hasRunPermissions ? $"IP.importNow({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty,
					runNowButton.OnClickEvent);

				ConsoleButton stopButton = console.ButtonList[buttonIndex++];
				Assert.AreEqual("Stop", stopButton.DisplayText);
				Assert.AreEqual(hasStoppableJobs, stopButton.Enabled);
				Assert.AreEqual(onClickEvents.StopOnClickEvent, stopButton.OnClickEvent);

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

		[TestCase(true)]
		[TestCase(false)]
		public void GetConsole_NonRelativityProvider_GoldFlow(bool hasStoppableJobs)
		{
			// ARRANGE
			var integrationPointDto = new IntegrationPointDTO()
			{
				HasErrors = true,
				SourceProvider = 8392
			};

			Core.Constants.SourceProvider sourceProvider = Core.Constants.SourceProvider.Other;
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
			_managerFactory.CreateIntegrationPointManager(_contextContainer).Returns(_integrationPointManager);
			_managerFactory.CreateStateManager().Returns(_stateManager);
			_managerFactory.CreateQueueManager(_contextContainer).Returns(_queueManager);
			_managerFactory.CreateJobHistoryManager(_contextContainer).Returns(_jobHistoryManager);

			_helperClassFactory.CreateOnClickEventHelper(_managerFactory, _contextContainer).Returns(_onClickEventHelper);

			_integrationPointManager.Read(_APPLICATION_ID, _ARTIFACT_ID).Returns(integrationPointDto);
			_integrationPointManager.GetSourceProvider(Arg.Is(_APPLICATION_ID), Arg.Is(integrationPointDto))
				.Returns(sourceProvider);

			_jobHistoryManager.GetIntegrationPointHasStoppableJobs(_APPLICATION_ID, _ARTIFACT_ID).Returns(hasStoppableJobs);

			ButtonStateDTO buttonStates = null;
			OnClickEventDTO onClickEvents = null;

			buttonStates = new ButtonStateDTO()
			{
				RunNowButtonEnabled = true,
				StopButtonEnabled = hasStoppableJobs
			};

			_stateManager.GetButtonState(hasStoppableJobs).Returns(buttonStates);

			onClickEvents = new OnClickEventDTO()
			{
				RunNowOnClickEvent = $"IP.importNow({_ARTIFACT_ID},{_APPLICATION_ID})",
				StopOnClickEvent = hasStoppableJobs ? $"IP.importNow({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty
			};

			_onClickEventHelper.GetOnClickEvents(_APPLICATION_ID, _ARTIFACT_ID, buttonStates)
				.Returns(onClickEvents);

			// ACT
			kCura.EventHandler.Console console = _instance.GetConsole(ConsoleEventHandler.PageEvent.Load);

			// ASSERT
			_contextContainerFactory.Received().CreateContextContainer(_helper);
			_managerFactory.Received().CreateIntegrationPointManager(_contextContainer);

			Assert.IsNotNull(console);
			int buttonCount = 2;
			Assert.AreEqual(buttonCount, console.ButtonList.Count, $"There should be {buttonCount} buttons on the console");

			int buttonIndex = 0;
			ConsoleButton runNowButton = console.ButtonList[buttonIndex++];
			Assert.AreEqual("Run Now", runNowButton.DisplayText);
			Assert.AreEqual(false, runNowButton.RaisesPostBack);

			_jobHistoryManager.Received(1).GetIntegrationPointHasStoppableJobs(_APPLICATION_ID, _ARTIFACT_ID);

			Assert.IsTrue(runNowButton.Enabled);
			Assert.AreEqual($"IP.importNow({_ARTIFACT_ID},{_APPLICATION_ID})", runNowButton.OnClickEvent);

			ConsoleButton stopButton = console.ButtonList[buttonIndex++];
			Assert.AreEqual("Stop", stopButton.DisplayText);
			Assert.AreEqual(hasStoppableJobs, stopButton.Enabled);
			Assert.AreEqual(onClickEvents.StopOnClickEvent, stopButton.OnClickEvent);
		}
	}
}