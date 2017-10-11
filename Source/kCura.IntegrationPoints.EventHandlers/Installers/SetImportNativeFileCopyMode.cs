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

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[RunOnce(true)]
	[Description("Sets ImportNativeFile flag depending on source and destination configuration")]
	[Guid("8DA02F00-480C-437B-BACF-9839CE83042D")]
	public class SetImportNativeFileCopyMode : PostInstallEventHandler, IEventHandler
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

		public string SuccessMessage => $"ImportNativeFiles flag set successfuly for all integration points. Workspace ID: {Helper.GetActiveCaseID()}";
		public string FailureMessage => $"Problem with setting ImportNativesFlag. Workspace ID: {Helper.GetActiveCaseID()}";
		public Type CommandType => typeof(SetImportNativeFileCopyModeCommand);
	}
}
