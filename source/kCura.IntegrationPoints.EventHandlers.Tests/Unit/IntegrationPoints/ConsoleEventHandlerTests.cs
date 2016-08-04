﻿using System;
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
		private const string _RUN = "Run";
		private const string _STOP = "Stop";
		private const string _RETRY_ERRORS = "Retry Errors";
		private const string _VIEW_ERRORS = "View Errors";
		private const string _RUN_ENDPOINT = "IP.importNow";
		private const string _RETRY_ENDPOINT = "IP.retryJob";
		private const string _STOP_ENDPOINT = "IP.stopJob";

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

		[TestCase(false, true, false, false)]
		[TestCase(false, true, true, false)]
		[TestCase(true, true, true, false)]
		[TestCase(false, false, false, false)]
		[TestCase(false, false, true, false)]
		[TestCase(true, false, true, false)]
		[TestCase(true, true, false, true)]
		[TestCase(true, true, true, true)]
		[TestCase(true, true, true, true)]
		[TestCase(true, false, false, true)]
		[TestCase(true, false, true, true)]
		[TestCase(true, false, true, true)]
		public void GetConsole_RelativityProvider_GoldFlow(bool hasJobsExecutingOrInQueue, bool hasRunPermissions, bool hasViewErrorsPermissions, bool hasStoppableJobs)
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

			StoppableJobCollection stoppableJobCollection = null;

			if (hasStoppableJobs)
			{
				stoppableJobCollection = new StoppableJobCollection()
				{
					PendingJobArtifactIds = new[] {1232},
					ProcessingJobArtifactIds = new[] {9403},
				};
			}
			else
			{
				stoppableJobCollection = new StoppableJobCollection();
			}

			_jobHistoryManager.GetStoppableJobCollection(_APPLICATION_ID, _ARTIFACT_ID).Returns(stoppableJobCollection);

			if (!hasRunPermissions || !hasViewErrorsPermissions)
			{
				_managerFactory.CreateErrorManager(_contextContainer).Returns(_errorManager);
			}

			RelativityButtonStateDTO buttonStates = null;
			RelativityOnClickEventDTO onClickEvents = null;

			buttonStates = new RelativityButtonStateDTO()
			{
				RunNowButtonEnabled = !hasJobsExecutingOrInQueue,
				RetryErrorsButtonEnabled = !hasJobsExecutingOrInQueue,
				ViewErrorsLinkEnabled = hasViewErrorsPermissions,
				StopButtonEnabled = hasStoppableJobs
			};

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

			string actionButtonOnClickEvent;
			if (!hasJobsExecutingOrInQueue)
			{
				actionButtonOnClickEvent = $"{_RUN_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})";
			}
			else if (hasStoppableJobs)
			{
				actionButtonOnClickEvent = $"{_STOP_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})";
			}
			else
			{
				actionButtonOnClickEvent = string.Empty;
			}
			onClickEvents = new RelativityOnClickEventDTO()
			{
				RunNowOnClickEvent = actionButtonOnClickEvent,
				RetryErrorsOnClickEvent = integrationPointDto.HasErrors.Value && !hasJobsExecutingOrInQueue ? $"{_RETRY_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty,
				ViewErrorsOnClickEvent =
					integrationPointDto.HasErrors.Value && hasViewErrorsPermissions ? "Really long string" : String.Empty,
				StopOnClickEvent = actionButtonOnClickEvent
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
				int buttonCount = 3;
				Assert.AreEqual(buttonCount, console.ButtonList.Count, $"There should be {buttonCount} buttons on the console");
			}
			else
			{
				int buttonCount = 2;
				Assert.AreEqual(buttonCount, console.ButtonList.Count, $"There should be {buttonCount} buttons on the console");
			}

			int buttonIndex = 0;
			ConsoleButton runNowButton = console.ButtonList[buttonIndex++];
			

			_jobHistoryManager.Received(1).GetStoppableJobCollection(_APPLICATION_ID, _ARTIFACT_ID);

			if (!hasJobsExecutingOrInQueue)
			{
				Assert.AreEqual(_RUN, runNowButton.DisplayText);
				Assert.AreEqual($"{_RUN_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})", runNowButton.OnClickEvent);
				Assert.AreEqual(buttonStates.RunNowButtonEnabled, runNowButton.Enabled);
				Assert.AreEqual(false, runNowButton.RaisesPostBack);
			}
			else if (hasStoppableJobs)
			{
				Assert.AreEqual(_STOP, runNowButton.DisplayText);
				Assert.AreEqual($"{_STOP_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})", runNowButton.OnClickEvent);
				Assert.AreEqual(buttonStates.StopButtonEnabled, runNowButton.Enabled);
				Assert.AreEqual(false, runNowButton.RaisesPostBack);
			}
			else
			{
				Assert.AreEqual(_STOP, runNowButton.DisplayText);
				Assert.AreEqual(string.Empty, runNowButton.OnClickEvent);
				Assert.AreEqual(buttonStates.RunNowButtonEnabled, runNowButton.Enabled);
				Assert.AreEqual(false, runNowButton.RaisesPostBack);
			}
		

				ConsoleButton retryErrorsButton = console.ButtonList[buttonIndex++];
				Assert.AreEqual(_RETRY_ERRORS, retryErrorsButton.DisplayText);
				Assert.AreEqual(buttonStates.RetryErrorsButtonEnabled, retryErrorsButton.Enabled);
				Assert.AreEqual(false, retryErrorsButton.RaisesPostBack);
				Assert.AreEqual(!hasJobsExecutingOrInQueue && integrationPointDto.HasErrors.Value ? $"{_RETRY_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})" : String.Empty,
					retryErrorsButton.OnClickEvent);

				if (hasViewErrorsPermissions)
				{
					ConsoleButton viewErrorsButtonLink = console.ButtonList[buttonIndex++];
					Assert.AreEqual(_VIEW_ERRORS, viewErrorsButtonLink.DisplayText);
					Assert.AreEqual(buttonStates.ViewErrorsLinkEnabled, viewErrorsButtonLink.Enabled);
					Assert.AreEqual(false, viewErrorsButtonLink.RaisesPostBack);
					Assert.AreEqual("Really long string", viewErrorsButtonLink.OnClickEvent);
				}
		}

		[TestCase(true, true)]
		[TestCase(true, false)] 
		[TestCase(false, false)]
		public void GetConsole_NonRelativityProvider_GoldFlow(bool hasJobsExecutingOrInQueue, bool hasStoppableJobs)
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

			StoppableJobCollection stoppableJobCollection = null;

			if (hasStoppableJobs)
			{
				stoppableJobCollection = new StoppableJobCollection()
				{
					PendingJobArtifactIds = new[] { 1232 },
					ProcessingJobArtifactIds = new[] { 9403 },
				};
			}
			else
			{
				stoppableJobCollection = new StoppableJobCollection();
			}

			_jobHistoryManager.GetStoppableJobCollection(_APPLICATION_ID, _ARTIFACT_ID).Returns(stoppableJobCollection);
			_queueManager.HasJobsExecutingOrInQueue(_APPLICATION_ID, _ARTIFACT_ID).Returns(hasJobsExecutingOrInQueue);

			ButtonStateDTO buttonStates = null;
			OnClickEventDTO onClickEvents = null;

			buttonStates = new ButtonStateDTO()
			{
				RunNowButtonEnabled = !hasJobsExecutingOrInQueue,
				StopButtonEnabled = hasStoppableJobs
			};

			_stateManager.GetButtonState(hasJobsExecutingOrInQueue, hasStoppableJobs).Returns(buttonStates);

			string actionButtonOnClickEvent;
			if (!hasJobsExecutingOrInQueue)
			{
				actionButtonOnClickEvent = $"{_RUN_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})";
			}
			else if (hasStoppableJobs)
			{
				actionButtonOnClickEvent = $"{_STOP_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})";
			}
			else
			{
				actionButtonOnClickEvent = string.Empty;
			}
			onClickEvents = new OnClickEventDTO()
			{
				RunNowOnClickEvent = actionButtonOnClickEvent,
				StopOnClickEvent = actionButtonOnClickEvent
			};

			_onClickEventHelper.GetOnClickEvents(_APPLICATION_ID, _ARTIFACT_ID, buttonStates)
				.Returns(onClickEvents);

			// ACT
			kCura.EventHandler.Console console = _instance.GetConsole(ConsoleEventHandler.PageEvent.Load);

			// ASSERT
			_contextContainerFactory.Received().CreateContextContainer(_helper);
			_managerFactory.Received().CreateIntegrationPointManager(_contextContainer);
			_jobHistoryManager.Received(1).GetStoppableJobCollection(_APPLICATION_ID, _ARTIFACT_ID);

			Assert.IsNotNull(console);

			int buttonCount = 1;
			Assert.AreEqual(buttonCount, console.ButtonList.Count, $"There should be {buttonCount} buttons on the console");

			ConsoleButton actionButton = console.ButtonList[0];

			if (!hasJobsExecutingOrInQueue)
			{
				Assert.AreEqual(_RUN, actionButton.DisplayText);
				Assert.AreEqual(false, actionButton.RaisesPostBack);
				Assert.IsTrue(actionButton.Enabled);
				Assert.AreEqual($"{_RUN_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})", actionButton.OnClickEvent);
			}
			else if (hasStoppableJobs)
			{
				Assert.AreEqual(_STOP, actionButton.DisplayText);
				Assert.AreEqual(false, actionButton.RaisesPostBack);
				Assert.IsTrue(actionButton.Enabled);
				Assert.AreEqual($"{_STOP_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})", actionButton.OnClickEvent);
			}
			else
			{
				Assert.AreEqual(_STOP, actionButton.DisplayText);
				Assert.AreEqual(false, actionButton.RaisesPostBack);
				Assert.IsFalse(actionButton.Enabled);
				Assert.AreEqual(String.Empty, actionButton.OnClickEvent);
			}
		}
	}
}