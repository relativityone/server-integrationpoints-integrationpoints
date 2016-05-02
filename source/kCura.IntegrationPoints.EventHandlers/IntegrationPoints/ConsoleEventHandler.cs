using System;
using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using Console = kCura.EventHandler.Console;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public class ConsoleEventHandler : ConsoleEventHandlerBase
	{
		private IPermissionService _permissionService;
		private readonly IManagerFactory _integrationPointManagerFactory;
		private readonly IContextContainerFactory _contextContainerFactory;

		public IPermissionService PermissionService
		{
			get { return _permissionService ?? (_permissionService = new PermissionService(base.GetServicesMgr)); }
			set { _permissionService = value; }
		}

		public ConsoleEventHandler() : this(new ContextContainerFactory(), new ManagerFactory())
		{
		}

		internal ConsoleEventHandler(IContextContainerFactory contextContainerFactory, IManagerFactory integrationPointManagerFactory)
		{
			_contextContainerFactory = contextContainerFactory;
			_integrationPointManagerFactory = integrationPointManagerFactory;
		}

		public override Console GetConsole(PageEvent pageEvent)
		{
			var console = new Console();
			console.Title = "RUN";
			console.ButtonList = new List<ConsoleButton>();

			bool isEnabled = PermissionService.UserCanImport(base.Helper.GetActiveCaseID());
			console.ButtonList.Add(GetRunNowButton(isEnabled));

			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(base.Helper);
			IIntegrationPointManager integrationPointManager = _integrationPointManagerFactory.CreateIntegrationPointManager(contextContainer);
			IntegrationPointDTO integrationPointDto = integrationPointManager.Read(this.Application.ArtifactID, this.ActiveArtifact.ArtifactID);

			bool hasErrors = integrationPointDto.HasErrors.GetValueOrDefault(false);
			console.ButtonList.Add(GetRetryErrorsButton(hasErrors));
			console.ButtonList.Add(GetViewErrorsLink(hasErrors));

			return console;
		}

		public override void OnButtonClick(ConsoleButton consoleButton)
		{
		}

		public override FieldCollection RequiredFields
		{
			get { return new FieldCollection(); }
		}

		private ConsoleButton GetRunNowButton(bool isEnabled)
		{
			return new ConsoleButton
			{
				DisplayText = "Run Now",
				RaisesPostBack = false,
				Enabled = isEnabled,
				OnClickEvent = "IP.importNow(" + this.ActiveArtifact.ArtifactID + "," + this.Application.ArtifactID + ")",
			};
		}

		private ConsoleButton GetRetryErrorsButton(bool hasErrors)
		{
			return new ConsoleButton()
			{
				DisplayText = "Retry Errors",
				RaisesPostBack = false,
				Enabled = hasErrors,
				OnClickEvent = hasErrors ? "alert('NOT IMPLEMENTED')" : String.Empty
			};
		}

		private ConsoleButton GetViewErrorsLink(bool hasErrors)
		{
			return new ConsoleLinkButton()
			{
				DisplayText = "View Errors",
				Enabled = hasErrors,
				RaisesPostBack = false,
				OnClickEvent = hasErrors ? "alert('NOT IMPLEMENTED')" : String.Empty
			};

		}
	}
}
