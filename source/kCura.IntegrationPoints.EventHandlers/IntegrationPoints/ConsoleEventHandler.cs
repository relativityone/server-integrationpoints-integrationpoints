using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public class ConsoleEventHandler : ConsoleEventHandlerBase
	{
		private readonly IManagerFactory _managerFactory;
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly IHelperClassFactory _helperClassFactory;
		private const string _TRANSFER_OPTIONS = "Transfer Options";
		private const string _RUN = "Run";
		private const string _RETRY_ERRORS = "Retry Errors";
		private const string _VIEW_ERRORS = "View Errors";
		private const string _STOP = "Stop";

		public ConsoleEventHandler()
		{
			_contextContainerFactory = new ContextContainerFactory();
			_managerFactory = new ManagerFactory();
			_helperClassFactory = new HelperClassFactory();
		}

		internal ConsoleEventHandler(IContextContainerFactory contextContainerFactory, IManagerFactory managerFactory, IHelperClassFactory helperClassFactory)
		{
			_contextContainerFactory = contextContainerFactory;
			_managerFactory = managerFactory;
			_helperClassFactory = helperClassFactory;
		}

		public override FieldCollection RequiredFields => new FieldCollection();

		public override void OnButtonClick(ConsoleButton consoleButton)
		{
		}

		public override EventHandler.Console GetConsole(PageEvent pageEvent)
		{
			var console = new EventHandler.Console
			{
				Title = _TRANSFER_OPTIONS,
			};

			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(Helper);
			IIntegrationPointManager integrationPointManager = _managerFactory.CreateIntegrationPointManager(contextContainer);
			IJobHistoryManager jobHistoryManager = _managerFactory.CreateJobHistoryManager(contextContainer);
			IStateManager stateManager = _managerFactory.CreateStateManager();

			IntegrationPointDTO integrationPointDto = integrationPointManager.Read(Application.ArtifactID, ActiveArtifact.ArtifactID);

			bool integrationPointHasErrors = integrationPointDto.HasErrors.GetValueOrDefault(false);
			Core.Constants.SourceProvider sourceProvider = integrationPointManager.GetSourceProvider(Application.ArtifactID, integrationPointDto);
			StoppableJobCollection stoppableJobCollection = jobHistoryManager.GetStoppableJobCollection(Application.ArtifactID, ActiveArtifact.ArtifactID);
			bool integrationPointIsStoppable = stoppableJobCollection.HasStoppableJobs;

			IOnClickEventConstructor onClickEventHelper = _helperClassFactory.CreateOnClickEventHelper(_managerFactory, contextContainer);

			var buttonList = new List<ConsoleButton>();
			IQueueManager queueManager = _managerFactory.CreateQueueManager(contextContainer);
			bool hasJobsExecutingOrInQueue = queueManager.HasJobsExecutingOrInQueue(Application.ArtifactID, ActiveArtifact.ArtifactID);

			if (sourceProvider == Core.Constants.SourceProvider.Relativity)
			{
				PermissionCheckDTO jobHistoryErrorViewPermissionCheck = integrationPointManager.UserHasPermissionToViewErrors(Application.ArtifactID);
				bool canViewErrors = jobHistoryErrorViewPermissionCheck.Success;

				RelativityButtonStateDTO buttonState = stateManager.GetRelativityProviderButtonState(hasJobsExecutingOrInQueue, integrationPointHasErrors, canViewErrors, integrationPointIsStoppable);
				RelativityOnClickEventDTO onClickEvents = onClickEventHelper.GetOnClickEventsForRelativityProvider(Application.ArtifactID, ActiveArtifact.ArtifactID, buttonState);

				ConsoleButton actionButton = GetActionButton(buttonState, onClickEvents);
				ConsoleButton retryErrorsButton = GetRetryErrorsButton(buttonState.RetryErrorsButtonEnabled, onClickEvents.RetryErrorsOnClickEvent);

				buttonList.Add(actionButton);
				buttonList.Add(retryErrorsButton);

				if (canViewErrors)
				{
					ConsoleButton viewErrorsLink = GetViewErrorsLink(buttonState.ViewErrorsLinkEnabled, onClickEvents.ViewErrorsOnClickEvent);
					buttonList.Add(viewErrorsLink);
				}
			}
			else
			{
				ButtonStateDTO buttonState = stateManager.GetButtonState(hasJobsExecutingOrInQueue, integrationPointIsStoppable);
				OnClickEventDTO onClickEvents = onClickEventHelper.GetOnClickEvents(Application.ArtifactID, ActiveArtifact.ArtifactID, buttonState);
				ConsoleButton actionButton = GetActionButton(buttonState, onClickEvents);

				buttonList.Add(actionButton);
			}

			console.ButtonList = buttonList;

			return console;
		}

		private ConsoleButton GetActionButton(ButtonStateDTO actionButtonState, OnClickEventDTO actionButtonOnClickEvents)
		{
			bool runButtonEnabled = actionButtonState.RunButtonEnabled;
			bool stopButtonEnabled = actionButtonState.StopButtonEnabled;
			string displayText;
			string cssClass;
			string onClickEvent;
			if (runButtonEnabled)
			{
				displayText = _RUN;
				cssClass = "consoleButtonEnabled";
				onClickEvent = actionButtonOnClickEvents.RunOnClickEvent;
			}
			else if (stopButtonEnabled)
			{
				displayText = _STOP;
				cssClass = "consoleButtonDestructive";
				onClickEvent = actionButtonOnClickEvents.StopOnClickEvent;
			}
			else
			{
				displayText = _STOP;
				cssClass = "consoleButtonDisabled";
				onClickEvent = string.Empty;
			}

			return new ConsoleButton
			{
				DisplayText = displayText,
				CssClass = cssClass,
				RaisesPostBack = false,
				Enabled = (runButtonEnabled || stopButtonEnabled),
				OnClickEvent = onClickEvent
			};
		}

		private ConsoleButton GetRetryErrorsButton(bool isEnabled, string onClickEvent)
		{
			return new ConsoleButton
			{
				DisplayText = _RETRY_ERRORS,
				RaisesPostBack = false,
				Enabled = isEnabled,
				OnClickEvent = onClickEvent
			};
		}

		private ConsoleButton GetViewErrorsLink(bool isEnabled, string onClickEvent)
		{
			return new ConsoleLinkButton
			{
				DisplayText = _VIEW_ERRORS,
				Enabled = isEnabled,
				RaisesPostBack = false,
				OnClickEvent = onClickEvent
			};
		}
	}
}