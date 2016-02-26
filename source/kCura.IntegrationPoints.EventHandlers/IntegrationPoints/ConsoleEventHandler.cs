using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoints.Data;
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
			console.Title = "RUN";
			console.ButtonList = new List<ConsoleButton>();
			bool isEnabled = PermissionService.userCanImport(base.Helper.GetAuthenticationManager().UserInfo.WorkspaceUserArtifactID);
			
			console.ButtonList.Add(new ConsoleButton
				{
					DisplayText = "Run Now",
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
