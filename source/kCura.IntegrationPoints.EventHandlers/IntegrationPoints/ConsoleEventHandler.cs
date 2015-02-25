using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.EventHandler;
using kCura.EventHandler.PostExecuteAction;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.Repositories;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;
using Console = kCura.EventHandler.Console;


namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public class ConsoleEventHandler : ConsoleEventHandlerBase
	{
		private PermissionService permissionService;


		public PermissionService PermissionService
		{
			get
			{
				if (permissionService == null)
				{
					permissionService =
								 new PermissionService(base.GetWorkspaceDbContext());
				}
				return permissionService;

			}
			set { permissionService = value; }
		}
		public override Console GetConsole(PageEvent pageEvent)
		{

			var console = new Console();
			console.Title = "IMPORT";
			console.ButtonList = new List<ConsoleButton>();
			bool isEnabled = PermissionService.userCanImport(base.Helper.GetAuthenticationManager().UserInfo.ArtifactID);
			
			console.ButtonList.Add(new ConsoleButton
				{
					DisplayText = "Import Now",
					RaisesPostBack = false,
					Enabled = isEnabled,
					OnClickEvent = "IP.importNow(" + this.ActiveArtifact.ArtifactID + "," + this.Application.ArtifactID + ")",

				});
			
			return console;
		}

		public override void OnButtonClick(ConsoleButton consoleButton)
		{


		}

		public override FieldCollection RequiredFields
		{
			get { return new FieldCollection(); }
		}
	}
}
