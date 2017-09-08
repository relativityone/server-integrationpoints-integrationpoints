using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.EventHandlers.Commands.Factories;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Removes AgentJobLog_08C0CE2D-8191-4E8F-B037-899CEAEE493D table")]
	[RunOnce(true)]
	[Guid("BE794C48-FB6E-4279-9483-D4D602278153")]
	public class RemoveAgentJobLogTableEventHandler : PostInstallEventHandler, IEventHandler
	{
		public override Response Execute()
		{
			var executor = new EventHandlerExecutor();
			return executor.Execute(this);
		}

		public IEHContext Context => new EHContext
		{
			Helper = Helper
		};

		public string SuccessMessage => "Agent Job Log table succesfully deleted.";

		public string FailureMessage => "Could not delete Agent Job Log table";

		public Type CommandType => typeof(RemoveAgentJobLogTableCommand);

	}
}
