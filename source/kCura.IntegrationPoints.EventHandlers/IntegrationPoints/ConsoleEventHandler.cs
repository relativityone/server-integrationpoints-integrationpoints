using System;
using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public class ConsoleEventHandler : ConsoleEventHandlerBase
	{
		private IPermissionService _permissionService;
		private readonly IManagerFactory _integrationPointManagerFactory;
		private readonly IContextContainerFactory _contextContainerFactory;

		private IPermissionService PermissionService => _permissionService ?? (_permissionService = new PermissionService(GetServicesMgr));

		public ConsoleEventHandler()
		{
			_contextContainerFactory = new ContextContainerFactory();
			_integrationPointManagerFactory = new ManagerFactory();
		}

		internal ConsoleEventHandler(IContextContainerFactory contextContainerFactory, IManagerFactory integrationPointManagerFactory, IPermissionService permissionService)
		{
			_contextContainerFactory = contextContainerFactory;
			_integrationPointManagerFactory = integrationPointManagerFactory;
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
			IIntegrationPointManager integrationPointManager = _integrationPointManagerFactory.CreateIntegrationPointManager(contextContainer);
			ISourceProviderManager sourceProviderManager =
				_integrationPointManagerFactory.CreateSourceProviderManager(contextContainer);
			IntegrationPointDTO integrationPointDto = integrationPointManager.Read(Application.ArtifactID, ActiveArtifact.ArtifactID);
			SourceProviderDTO sourceProviderDto = sourceProviderManager.Read(Application.ArtifactID,
				integrationPointDto.SourceProvider.Value);

			bool hasErrors = integrationPointDto.HasErrors.GetValueOrDefault(false);
			if (sourceProviderDto.Name == kCura.IntegrationPoints.DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_NAME)
			{
				console.ButtonList.Add(GetRetryErrorsButton(hasErrors, isEnabled));
			}
			console.ButtonList.Add(GetViewErrorsLink(hasErrors));

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
	}
}
