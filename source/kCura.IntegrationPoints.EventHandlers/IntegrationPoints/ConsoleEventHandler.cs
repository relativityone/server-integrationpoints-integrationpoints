using System;
using System.Collections.Generic;
using Castle.Components.DictionaryAdapter;
using kCura.EventHandler;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public class ConsoleEventHandler : ConsoleEventHandlerBase
	{
		private readonly IManagerFactory _managerFactory;
		private readonly IContextContainerFactory _contextContainerFactory;
		
		public ConsoleEventHandler()
		{
			_contextContainerFactory = new ContextContainerFactory();
			_managerFactory = new ManagerFactory();
		}

		internal ConsoleEventHandler(IContextContainerFactory contextContainerFactory, IManagerFactory managerFactory)
		{
			_contextContainerFactory = contextContainerFactory;
			_managerFactory = managerFactory;
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
			IntegrationPointDTO integrationPointDto = integrationPointManager.Read(Application.ArtifactID, ActiveArtifact.ArtifactID);

			bool integrationPointHasErrors = integrationPointDto.HasErrors.GetValueOrDefault(false);
			bool sourceProviderIsRelativity = integrationPointManager.IntegrationPointSourceProviderIsRelativity(Application.ArtifactID, integrationPointDto);
			bool userHasPermissions = true;

			var buttonList = new List<ConsoleButton>();

			if (sourceProviderIsRelativity)
			{
				PermissionCheckDTO permissionCheck = integrationPointManager.UserHasPermissions(Application.ArtifactID,
					integrationPointDto);
				userHasPermissions = permissionCheck.Success;

				ConsoleButton runNowButton = GetRunNowButton(userHasPermissions);
				ConsoleButton retryErrorsButton = GetRetryErrorsButton(userHasPermissions && integrationPointHasErrors);
				ConsoleButton viewErrorsLink = GetViewErrorsLink(contextContainer, integrationPointHasErrors);

				buttonList.Add(runNowButton);
				buttonList.Add(retryErrorsButton);
				buttonList.Add(viewErrorsLink);

				if (!userHasPermissions)
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
				ConsoleButton runNowButton = GetRunNowButton(userHasPermissions);
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

		private ConsoleButton GetRetryErrorsButton(bool isEnabled)
		{
			return new ConsoleButton
			{
				DisplayText = "Retry Errors",
				RaisesPostBack = false,
				Enabled = isEnabled,
				OnClickEvent = isEnabled ? $"IP.retryJob({ActiveArtifact.ArtifactID},{Application.ArtifactID})" : String.Empty
			};
		}

		private ConsoleButton GetViewErrorsLink(IContextContainer contextContainer, bool hasErrors)
		{
			string onClickEvent = String.Empty;

			if (hasErrors)
			{
				IFieldManager fieldManager = _managerFactory.CreateFieldManager(contextContainer);
				IJobHistoryManager jobHistoryManager = _managerFactory.CreateJobHistoryManager(contextContainer);
				IArtifactGuidManager artifactGuidManager = _managerFactory.CreateArtifactGuidManager(contextContainer);
				IObjectTypeManager objectTypeManager = _managerFactory.CreateObjectTypeManager(contextContainer);

				var errorErrorStatusFieldGuid = new Guid(JobHistoryErrorDTO.FieldGuids.ErrorStatus);
				var jobHistoryFieldGuid = new Guid(JobHistoryErrorDTO.FieldGuids.JobHistory);

				Dictionary<Guid, int> guidsAndArtifactIds = artifactGuidManager.GetArtifactIdsForGuids(Application.ArtifactID, new[]
				{
					JobHistoryErrorDTO.Choices.ErrorStatus.Guids.New,
					errorErrorStatusFieldGuid,
					jobHistoryFieldGuid
				});

				int jobHistoryErrorStatusArtifactViewFieldId = fieldManager.RetrieveArtifactViewFieldId(Application.ArtifactID, guidsAndArtifactIds[errorErrorStatusFieldGuid]).GetValueOrDefault();
				int jobHistoryErrorStatusNewChoiceArtifactId = guidsAndArtifactIds[JobHistoryErrorDTO.Choices.ErrorStatus.Guids.New];
				int jobHistoryErrorDescriptorArtifactTypeId = objectTypeManager.RetrieveObjectTypeDescriptorArtifactTypeId(Application.ArtifactID, new Guid(JobHistoryErrorDTO.ArtifactTypeGuid));
				int jobHistoryArtifactViewFieldId = fieldManager.RetrieveArtifactViewFieldId(Application.ArtifactID, guidsAndArtifactIds[jobHistoryFieldGuid]).GetValueOrDefault();
				int jobHistoryInstanceArtifactId = jobHistoryManager.GetLastJobHistoryArtifactId(Application.ArtifactID, ActiveArtifact.ArtifactID);

				onClickEvent = $"window.location='../../Case/IntegrationPoints/ErrorsRedirect.aspx?ErrorStatusArtifactViewFieldID={jobHistoryErrorStatusArtifactViewFieldId}"
						+ $"&ErrorStatusNewChoiceArtifactId={jobHistoryErrorStatusNewChoiceArtifactId}&JobHistoryErrorArtifactTypeId={jobHistoryErrorDescriptorArtifactTypeId}"
						+ $"&JobHistoryArtifactViewFieldID={jobHistoryArtifactViewFieldId}&JobHistoryInstanceArtifactId={jobHistoryInstanceArtifactId}'; return false;";
			}

			return new ConsoleLinkButton
			{
				DisplayText = "View Errors",
				Enabled = hasErrors,
				RaisesPostBack = false,
				OnClickEvent = onClickEvent
			};
		}
	}
}
