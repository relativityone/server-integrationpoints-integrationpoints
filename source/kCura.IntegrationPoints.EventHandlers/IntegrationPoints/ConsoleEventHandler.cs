using System;
using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Managers;

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

		public override void OnButtonClick(ConsoleButton consoleButton) { }
		
		public override EventHandler.Console GetConsole(PageEvent pageEvent)
		{
			var console = new EventHandler.Console
			{
				Title = "RUN",
			};
			
			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(Helper);
			IIntegrationPointManager integrationPointManager = _managerFactory.CreateIntegrationPointManager(contextContainer);
			IStateManager stateManager = _managerFactory.CreateStateManager(contextContainer);

			IntegrationPointDTO integrationPointDto = integrationPointManager.Read(Application.ArtifactID, ActiveArtifact.ArtifactID);

			bool integrationPointHasErrors = integrationPointDto.HasErrors.GetValueOrDefault(false);
			bool sourceProviderIsRelativity = integrationPointManager.IntegrationPointSourceProviderIsRelativity(Application.ArtifactID, integrationPointDto);
			PermissionCheckDTO permissionCheck = integrationPointManager.UserHasPermissions(Application.ArtifactID, integrationPointDto, sourceProviderIsRelativity);

			IOnClickEventHelper onClickEventHelper = _helperClassFactory.CreateOnClickEventHelper(_managerFactory,
				contextContainer);

			var buttonList = new List<ConsoleButton>();

			if (sourceProviderIsRelativity)
			{
				ButtonStateDTO buttonState = stateManager.GetButtonState(Application.ArtifactID, ActiveArtifact.ArtifactID,
					permissionCheck.Success, integrationPointHasErrors);
				OnClickEventDTO onClickEvents = onClickEventHelper.GetOnClickEventsForRelativityProvider(Application.ArtifactID, ActiveArtifact.ArtifactID, buttonState);

				ConsoleButton runNowButton = GetRunNowButtonRelativityProvider(buttonState.RunNowButtonEnabled, onClickEvents.RunNowOnClickEvent);
				ConsoleButton retryErrorsButton = GetRetryErrorsButton(buttonState.RetryErrorsButtonEnabled, onClickEvents.RetryErrorsOnClickEvent);
				ConsoleButton viewErrorsLink = GetViewErrorsLink(buttonState.ViewErrorsLinkEnabled, onClickEvents.ViewErrorsOnClickEvent);

				buttonList.Add(runNowButton);
				buttonList.Add(retryErrorsButton);
				buttonList.Add(viewErrorsLink);

				if (!permissionCheck.Success)
				{
					string script = "<script type='text/javascript'>"
					                + "$(document).ready(function () {"
					                + "IP.message.error.raise(\""
									+ permissionCheck.ErrorMessage
									+ "\", $(\".cardContainer\"));"
					                + "});"
					                + "</script>";
					console.AddScriptBlock("IPConsoleErrorDisplayScript", script);
				}
			}
			else
			{
				ConsoleButton runNowButton = GetRunNowButton(permissionCheck.Success);
				buttonList.Add(runNowButton);
			}

			console.ButtonList = buttonList;
			
			return console;
		}

		private ConsoleButton GetRunNowButton(bool isEnabled)
		{
			return new ConsoleButton
			{
				DisplayText = "Run Now",
				RaisesPostBack = false,
				Enabled = isEnabled,
				OnClickEvent = isEnabled ? $"IP.importNow({ActiveArtifact.ArtifactID},{Application.ArtifactID})" : String.Empty
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
