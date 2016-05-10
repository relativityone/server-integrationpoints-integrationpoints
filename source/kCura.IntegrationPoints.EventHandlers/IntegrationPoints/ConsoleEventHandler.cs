using System;
using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public class ConsoleEventHandler : ConsoleEventHandlerBase
	{
		private IPermissionService _permissionService;
		private readonly IManagerFactory _managerFactory;
		private readonly IContextContainerFactory _contextContainerFactory;

		private IPermissionService PermissionService => _permissionService ?? (_permissionService = new PermissionService(GetServicesMgr));

		public ConsoleEventHandler()
		{
			_contextContainerFactory = new ContextContainerFactory();
			_managerFactory = new ManagerFactory();
		}

		internal ConsoleEventHandler(IContextContainerFactory contextContainerFactory, IManagerFactory managerFactory, IPermissionService permissionService)
		{
			_contextContainerFactory = contextContainerFactory;
			_managerFactory = managerFactory;
			_permissionService = permissionService;
		}

		public override FieldCollection RequiredFields => new FieldCollection();

		public override void OnButtonClick(ConsoleButton consoleButton) { }
		
		public override EventHandler.Console GetConsole(PageEvent pageEvent)
		{
			var console = new EventHandler.Console
			{
				Title = "RUN",
				ButtonList = new List<ConsoleButton>()
			};

			bool isEnabled = PermissionService.UserCanImport(Helper.GetActiveCaseID());
			console.ButtonList.Add(GetRunNowButton(isEnabled));

			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(Helper);
			IIntegrationPointManager integrationPointManager = _managerFactory.CreateIntegrationPointManager(contextContainer);

			_managerFactory.CreateSourceProviderManager(contextContainer);

			IntegrationPointDTO integrationPointDto = integrationPointManager.Read(Application.ArtifactID, ActiveArtifact.ArtifactID);
			bool hasErrors = integrationPointDto.HasErrors.GetValueOrDefault(false);
			bool isRetriable = integrationPointManager.IntegrationPointTypeIsRetriable(Application.ArtifactID,
				integrationPointDto);

			if (isRetriable)
			{
				console.ButtonList.Add(GetRetryErrorsButton(hasErrors, isEnabled));
			}

			//ConsoleButton viewErrorsLink = GetViewErrorsLink(hasErrors);
			ConsoleButton viewErrorsLink = GetViewErrorsLink(contextContainer, hasErrors);

			console.ButtonList.Add(viewErrorsLink);

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

		private ConsoleButton GetRetryErrorsButton(bool hasErrors, bool isEnabled)
		{
			return new ConsoleButton
			{
				DisplayText = "Retry Errors",
				RaisesPostBack = false,
				Enabled = hasErrors && isEnabled,
				OnClickEvent = hasErrors && isEnabled ? $"IP.retryJob({ActiveArtifact.ArtifactID},{Application.ArtifactID})" : String.Empty
			};
		}

		private ConsoleButton GetViewErrorsLink(bool hasErrors)
		{
			return new ConsoleLinkButton
			{
				DisplayText = "View Errors",
				Enabled = hasErrors,
				RaisesPostBack = false,
				OnClickEvent = hasErrors ? "alert('NOT IMPLEMENTED')" : String.Empty
			};
		}

		private ConsoleButton GetViewErrorsLink(IContextContainer contextContainer, bool hasErrors)
		{
			// NOTE: TODO: Work in progress

			var managerFactory = _managerFactory.CreateFieldManager(contextContainer);
			var errorErrorStatusFieldGuid = new Guid(JobHistoryErrorDTO.FieldGuids.ErrorStatus);
			var jobHistoryFieldGuid = new Guid(JobHistoryErrorDTO.FieldGuids.JobHistory);

			Dictionary<Guid, int> fieldGuidsAndIds = managerFactory.RetrieveFieldArtifactIds(Application.ArtifactID, new [] { errorErrorStatusFieldGuid, jobHistoryFieldGuid });

			int errorStatusArtifactViewFieldId = managerFactory.RetrieveArtifactViewFieldId(Application.ArtifactID, fieldGuidsAndIds[errorErrorStatusFieldGuid]).GetValueOrDefault();
			int errorStatusNewChoiceArtifactId = 1041558;
			int jobHistoryErrorArtifactTypeId = 1000042;
			int jobHistoryArtifactViewFieldId = managerFactory.RetrieveArtifactViewFieldId(Application.ArtifactID, fieldGuidsAndIds[jobHistoryFieldGuid]).GetValueOrDefault();
			int jobHistoryInstanceArtifactId = 1041547;

			return new ConsoleLinkButton
			{
				DisplayText = "View Errors",
				Enabled = hasErrors,
				RaisesPostBack = false,
				OnClickEvent = hasErrors 
					? $"window.location='../../Case/IntegrationPoints/ErrorsRedirect.aspx?IntegrationPointArtifactID={ActiveArtifact.ArtifactID}&ErrorStatusArtifactViewFieldID={errorStatusArtifactViewFieldId}"
						+ $"&ErrorStatusNewChoiceArtifactId={errorStatusNewChoiceArtifactId}&JobHistoryErrorArtifactTypeId={jobHistoryErrorArtifactTypeId}"
						+ $"&JobHistoryArtifactViewFieldID={jobHistoryArtifactViewFieldId}&JobHistoryInstanceArtifactId={jobHistoryInstanceArtifactId}'; return false;" 
					: String.Empty
			};
		}
	}
}
