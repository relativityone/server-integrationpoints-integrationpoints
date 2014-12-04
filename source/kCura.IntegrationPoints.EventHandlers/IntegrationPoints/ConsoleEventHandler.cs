using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.EventHandler;
using kCura.Relativity.Client;
using Console = kCura.EventHandler.Console;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public class ConsoleEventHandler : ConsoleEventHandlerBase
	{
		public override Console GetConsole(PageEvent pageEvent)
		{
			var console = new Console();
			console.Title = "IMPORT";
			console.ButtonList = new List<ConsoleButton>();
			console.ButtonList.Add(new ConsoleButton
			{
				DisplayText = "Import Data Now",
				RaisesPostBack = false,
				Enabled = true,
				OnClickEvent = "IP.importNow(" + this.ActiveArtifact.ArtifactID + ")"
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
