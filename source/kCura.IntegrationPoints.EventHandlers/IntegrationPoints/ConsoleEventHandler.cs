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
				Title = "RUN",
			};

			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(Helper);
			IIntegrationPointManager integrationPointManager = _managerFactory.CreateIntegrationPointManager(contextContainer);
			IJobHistoryManager jobHistoryManager = _managerFactory.CreateJobHistoryManager(contextContainer);
			IStateManager stateManager = _managerFactory.CreateStateManager();

			IntegrationPointDTO integrationPointDto = integrationPointManager.Read(Application.ArtifactID, ActiveArtifact.ArtifactID);

			bool integrationPointHasErrors = integrationPointDto.HasErrors.GetValueOrDefault(false);
			Core.Constants.SourceProvider sourceProvider = integrationPointManager.GetSourceProvider(Application.ArtifactID, integrationPointDto);
			bool integrationPointIsStoppable = jobHistoryManager.GetIntegrationPointHasStoppableJobs(Application.ArtifactID, ActiveArtifact.ArtifactID);

			IOnClickEventConstructor onClickEventHelper = _helperClassFactory.CreateOnClickEventHelper(_managerFactory, contextContainer);

			var buttonList = new List<ConsoleButton>();
			if (sourceProvider == Core.Constants.SourceProvider.Relativity)
			{
				IQueueManager queueManager = _managerFactory.CreateQueueManager(contextContainer);

				bool hasJobsExecutingOrInQueue = queueManager.HasJobsExecutingOrInQueue(Application.ArtifactID,
					ActiveArtifact.ArtifactID);
				PermissionCheckDTO jobHistoryErrorViewPermissionCheck = integrationPointManager.UserHasPermissionToViewErrors(Application.ArtifactID);
				bool canViewErrors = jobHistoryErrorViewPermissionCheck.Success;

				RelativityButtonStateDTO buttonState = stateManager.GetRelativityProviderButtonState(hasJobsExecutingOrInQueue, integrationPointHasErrors, canViewErrors, integrationPointIsStoppable);
				RelativityOnClickEventDTO onClickEvents = onClickEventHelper.GetOnClickEventsForRelativityProvider(Application.ArtifactID, ActiveArtifact.ArtifactID, buttonState);

				ConsoleButton runNowButton = GetRunNowButtonRelativityProvider(buttonState.RunNowButtonEnabled, onClickEvents.RunNowOnClickEvent);
				ConsoleButton stopButton = GetStopButton(integrationPointIsStoppable, onClickEvents.StopOnClickEvent);
				ConsoleButton retryErrorsButton = GetRetryErrorsButton(buttonState.RetryErrorsButtonEnabled, onClickEvents.RetryErrorsOnClickEvent);

				buttonList.Add(runNowButton);
				buttonList.Add(stopButton);
				buttonList.Add(retryErrorsButton);

				if (canViewErrors)
				{
					ConsoleButton viewErrorsLink = GetViewErrorsLink(buttonState.ViewErrorsLinkEnabled, onClickEvents.ViewErrorsOnClickEvent);
					buttonList.Add(viewErrorsLink);
				}
			}
			else
			{
				ButtonStateDTO buttonState = stateManager.GetButtonState(integrationPointIsStoppable);
				OnClickEventDTO onClickEvents = onClickEventHelper.GetOnClickEvents(Application.ArtifactID, ActiveArtifact.ArtifactID, buttonState);
				ConsoleButton runNowButton = GetRunNowButton(onClickEvents.RunNowOnClickEvent);
				ConsoleButton stopButton = GetStopButton(buttonState.StopButtonEnabled, onClickEvents.StopOnClickEvent);

				buttonList.Add(runNowButton);
				buttonList.Add(stopButton);
			}

			console.ButtonList = buttonList;

			return console;
		}

		private ConsoleButton GetStopButton(bool isEnabled, string onClickEvent)
		{
			return new ConsoleButton()
			{
				DisplayText = "Stop",
				RaisesPostBack = false,
				Enabled = isEnabled,
				OnClickEvent = onClickEvent
			};
		}

		private ConsoleButton GetRunNowButton(string onClickEvent)
		{
			return new ConsoleButton
			{
				DisplayText = "Run Now",
				RaisesPostBack = false,
				Enabled = true,
				OnClickEvent = onClickEvent
			};
		}

		private ConsoleButton GetRunNowButtonRelativityProvider(bool isEnabled, string onClickEvent)
		{
			return new ConsoleButton
			{
				DisplayText = "Run Now",
				RaisesPostBack = false,
				Enabled = isEnabled,
				OnClickEvent = onClickEvent
			};
		}

		private ConsoleButton GetRetryErrorsButton(bool isEnabled, string onClickEvent)
		{
			return new ConsoleButton
			{
				DisplayText = "Retry Errors",
				RaisesPostBack = false,
				Enabled = isEnabled,
				OnClickEvent = onClickEvent
			};
		}

		private ConsoleButton GetViewErrorsLink(bool isEnabled, string onClickEvent)
		{
			return new ConsoleLinkButton
			{
				DisplayText = "View Errors",
				Enabled = isEnabled,
				RaisesPostBack = false,
				OnClickEvent = onClickEvent
			};
		}
	}
}