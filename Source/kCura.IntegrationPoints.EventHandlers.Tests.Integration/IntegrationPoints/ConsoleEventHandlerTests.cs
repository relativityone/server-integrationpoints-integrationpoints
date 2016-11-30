using System;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Console = kCura.EventHandler.Console;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration.IntegrationPoints
{
	[TestFixture]
	public class ConsoleEventHandlerTests
	{
		[SetUp]
		public void SetUp()
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
			_permissionRepository = Substitute.For<IPermissionRepository>();
			
			var activeArtifact = new Artifact(_ARTIFACT_ID, null, 0, "", false, new FieldCollection
			{
				new Field(1, "Name", "Name", 1, 1, 1, false, false, new FieldValue(_ARTIFACT_NAME), null)
			});
			var application = new Application(_APPLICATION_ID, "", "");

			_instance =
				new EventHandlers.IntegrationPoints.ConsoleEventHandler(
					new ButtonStateBuilder(_integrationPointManager, _queueManager, _jobHistoryManager, _stateManager, _permissionRepository),
					_onClickEventHelper, new ConsoleBuilder())
				{
					ActiveArtifact = activeArtifact,
					Application = application,
					Helper = _helper
				};
		}

		private const int _ARTIFACT_ID = 100300;
		private const int _APPLICATION_ID = 100101;
		private const string _ARTIFACT_NAME = "artifact_name";
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
		private IPermissionRepository _permissionRepository;

		private ConsoleEventHandler _instance;

		[TestCase(false, true, false, false, true)]
		[TestCase(false, true, true, false, true)]
		[TestCase(true, true, true, false, true)]
		[TestCase(false, false, false, false, true)]
		[TestCase(false, false, true, false, true)]
		[TestCase(true, false, true, false, true)]
		[TestCase(true, true, false, true, true)]
		[TestCase(true, true, true, true, true)]
		[TestCase(true, true, true, true, true)]
		[TestCase(true, false, false, true, true)]
		[TestCase(true, false, true, true, true)]
		[TestCase(true, false, true, true, true)]
		[TestCase(true, false, true, true, false)]
		public void GetConsole_RelativityProvider_GoldFlow(bool hasJobsExecutingOrInQueue, bool hasRunPermissions, bool hasViewErrorsPermissions, bool hasStoppableJobs,
			bool hasProfileAddPermission)
		{
			// ARRANGE
			var integrationPointDto = new IntegrationPointDTO
			{
				HasErrors = true,
				SourceProvider = 8392
			};

			string[] viewErrorMessages = {Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_NO_VIEW};
			Constants.SourceProvider sourceProvider = Constants.SourceProvider.Relativity;
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
			_managerFactory.CreateIntegrationPointManager(_contextContainer).Returns(_integrationPointManager);
			_managerFactory.CreateStateManager().Returns(_stateManager);
			_managerFactory.CreateQueueManager(_contextContainer).Returns(_queueManager);
			_managerFactory.CreateJobHistoryManager(_contextContainer).Returns(_jobHistoryManager);

			_permissionRepository.UserHasArtifactTypePermission(Arg.Any<Guid>(), ArtifactPermission.Create).Returns(hasProfileAddPermission);

			_helperClassFactory.CreateOnClickEventHelper(_managerFactory, _contextContainer).Returns(_onClickEventHelper);

			_integrationPointManager.Read(_APPLICATION_ID, _ARTIFACT_ID).Returns(integrationPointDto);
			_integrationPointManager.GetSourceProvider(Arg.Is(_APPLICATION_ID), Arg.Is(integrationPointDto))
				.Returns(sourceProvider);

			StoppableJobCollection stoppableJobCollection = null;

			if (hasStoppableJobs)
			{
				stoppableJobCollection = new StoppableJobCollection
				{
					PendingJobArtifactIds = new[] {1232},
					ProcessingJobArtifactIds = new[] {9403}
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

			ButtonStateDTO buttonStates = null;
			OnClickEventDTO onClickEvents = null;

			buttonStates = new ButtonStateDTO
			{
				RunButtonEnabled = !hasJobsExecutingOrInQueue,
				RetryErrorsButtonEnabled = !hasJobsExecutingOrInQueue,
				ViewErrorsLinkEnabled = hasViewErrorsPermissions,
				StopButtonEnabled = hasStoppableJobs,
				RetryErrorsButtonVisible = true,
				ViewErrorsLinkVisible = hasViewErrorsPermissions
			};

			_integrationPointManager.UserHasPermissionToViewErrors(_APPLICATION_ID).Returns(
				new PermissionCheckDTO
				{
					ErrorMessages = hasViewErrorsPermissions ? null : viewErrorMessages
				});

			_queueManager.HasJobsExecutingOrInQueue(_APPLICATION_ID, _ARTIFACT_ID).Returns(hasJobsExecutingOrInQueue);

			_stateManager.GetButtonState(
					Constants.SourceProvider.Relativity,
					hasJobsExecutingOrInQueue,
					integrationPointDto.HasErrors.Value,
					hasViewErrorsPermissions,
					hasStoppableJobs,
					hasProfileAddPermission)
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
			onClickEvents = new OnClickEventDTO
			{
				RunOnClickEvent = actionButtonOnClickEvent,
				RetryErrorsOnClickEvent = integrationPointDto.HasErrors.Value && !hasJobsExecutingOrInQueue ? $"{_RETRY_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})" : string.Empty,
				ViewErrorsOnClickEvent =
					integrationPointDto.HasErrors.Value && hasViewErrorsPermissions ? "Really long string" : string.Empty,
				StopOnClickEvent = actionButtonOnClickEvent
			};

			_onClickEventHelper.GetOnClickEvents(_APPLICATION_ID, _ARTIFACT_ID, _ARTIFACT_NAME, buttonStates)
				.Returns(onClickEvents);

			// ACT
			Console console = _instance.GetConsole(ConsoleEventHandler.PageEvent.Load);

			// ASSERT
			_integrationPointManager.Received(1).UserHasPermissionToViewErrors(_APPLICATION_ID);

			Assert.IsNotNull(console);
			if (hasViewErrorsPermissions)
			{
				int buttonCount = 3;
				Assert.AreEqual(buttonCount, console.Items.Count, $"There should be {buttonCount} buttons on the console");
			}
			else
			{
				int buttonCount = 2;
				Assert.AreEqual(buttonCount, console.Items.Count, $"There should be {buttonCount} buttons on the console");
			}

			int buttonIndex = 0;
			ConsoleButton runButton = (ConsoleButton) console.Items[buttonIndex++];


			_jobHistoryManager.Received(1).GetStoppableJobCollection(_APPLICATION_ID, _ARTIFACT_ID);

			if (!hasJobsExecutingOrInQueue)
			{
				Assert.AreEqual(_RUN, runButton.DisplayText);
				Assert.AreEqual($"{_RUN_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})", runButton.OnClickEvent);
				Assert.AreEqual(buttonStates.RunButtonEnabled, runButton.Enabled);
				Assert.AreEqual(false, runButton.RaisesPostBack);
			}
			else if (hasStoppableJobs)
			{
				Assert.AreEqual(_STOP, runButton.DisplayText);
				Assert.AreEqual($"{_STOP_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})", runButton.OnClickEvent);
				Assert.AreEqual(buttonStates.StopButtonEnabled, runButton.Enabled);
				Assert.AreEqual(false, runButton.RaisesPostBack);
			}
			else
			{
				Assert.AreEqual(_STOP, runButton.DisplayText);
				Assert.AreEqual(string.Empty, runButton.OnClickEvent);
				Assert.AreEqual(buttonStates.RunButtonEnabled, runButton.Enabled);
				Assert.AreEqual(false, runButton.RaisesPostBack);
			}


			ConsoleButton retryErrorsButton = (ConsoleButton) console.Items[buttonIndex++];
			Assert.AreEqual(_RETRY_ERRORS, retryErrorsButton.DisplayText);
			Assert.AreEqual(buttonStates.RetryErrorsButtonEnabled, retryErrorsButton.Enabled);
			Assert.AreEqual(false, retryErrorsButton.RaisesPostBack);
			Assert.AreEqual(!hasJobsExecutingOrInQueue && integrationPointDto.HasErrors.Value ? $"{_RETRY_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})" : string.Empty,
				retryErrorsButton.OnClickEvent);

			if (hasViewErrorsPermissions)
			{
				ConsoleButton viewErrorsButtonLink = (ConsoleButton) console.Items[buttonIndex++];
				Assert.AreEqual(_VIEW_ERRORS, viewErrorsButtonLink.DisplayText);
				Assert.AreEqual(buttonStates.ViewErrorsLinkEnabled, viewErrorsButtonLink.Enabled);
				Assert.AreEqual(false, viewErrorsButtonLink.RaisesPostBack);
				Assert.AreEqual("Really long string", viewErrorsButtonLink.OnClickEvent);
			}
		}

		[Category(kCura.IntegrationPoint.Tests.Core.Constants.SMOKE_TEST)]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public void GetConsole_NonRelativityProvider_GoldFlow(bool hasJobsExecutingOrInQueue, bool hasStoppableJobs)
		{
			// ARRANGE
			var integrationPointDto = new IntegrationPointDTO
			{
				HasErrors = true,
				SourceProvider = 8392
			};

			Constants.SourceProvider sourceProvider = Constants.SourceProvider.Other;
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
			_managerFactory.CreateIntegrationPointManager(_contextContainer).Returns(_integrationPointManager);
			_managerFactory.CreateStateManager().Returns(_stateManager);
			_managerFactory.CreateQueueManager(_contextContainer).Returns(_queueManager);
			_managerFactory.CreateJobHistoryManager(_contextContainer).Returns(_jobHistoryManager);

			_helperClassFactory.CreateOnClickEventHelper(_managerFactory, _contextContainer).Returns(_onClickEventHelper);

			_permissionRepository.UserHasArtifactTypePermission(Arg.Any<Guid>(), ArtifactPermission.Create).Returns(true);

			_integrationPointManager.Read(_APPLICATION_ID, _ARTIFACT_ID).Returns(integrationPointDto);
			_integrationPointManager.GetSourceProvider(Arg.Is(_APPLICATION_ID), Arg.Is(integrationPointDto))
				.Returns(sourceProvider);
			_integrationPointManager.UserHasPermissionToViewErrors(_APPLICATION_ID).Returns(new PermissionCheckDTO());

			StoppableJobCollection stoppableJobCollection = null;

			if (hasStoppableJobs)
			{
				stoppableJobCollection = new StoppableJobCollection
				{
					PendingJobArtifactIds = new[] {1232},
					ProcessingJobArtifactIds = new[] {9403}
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

			buttonStates = new ButtonStateDTO
			{
				RunButtonEnabled = !hasJobsExecutingOrInQueue,
				StopButtonEnabled = hasStoppableJobs,
				ViewErrorsLinkVisible = false,
				RetryErrorsButtonVisible = false,
				ViewErrorsLinkEnabled = false,
				RetryErrorsButtonEnabled = false
			};

			_stateManager.GetButtonState(Constants.SourceProvider.Other, hasJobsExecutingOrInQueue, true, true, hasStoppableJobs, true).Returns(buttonStates);

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
			onClickEvents = new OnClickEventDTO
			{
				RunOnClickEvent = actionButtonOnClickEvent,
				StopOnClickEvent = actionButtonOnClickEvent
			};

			_onClickEventHelper.GetOnClickEvents(_APPLICATION_ID, _ARTIFACT_ID, _ARTIFACT_NAME, buttonStates)
				.Returns(onClickEvents);

			// ACT
			Console console = _instance.GetConsole(ConsoleEventHandler.PageEvent.Load);

			// ASSERT
			_jobHistoryManager.Received(1).GetStoppableJobCollection(_APPLICATION_ID, _ARTIFACT_ID);

			Assert.IsNotNull(console);

			int buttonCount = 1;
			Assert.AreEqual(buttonCount, console.Items.Count, $"There should be {buttonCount} buttons on the console");

			ConsoleButton actionButton = (ConsoleButton) console.Items[0];

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
				Assert.AreEqual(string.Empty, actionButton.OnClickEvent);
			}
		}
	}
}