using System;
using System.Runtime.InteropServices;
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

		public string SuccessMessage => "ImportNativeFilesCopyMode flag set successfuly for all integration points and integration point profiles.";
		public string FailureMessage => "Problem with setting ImportNativesFilesCopyMode.";
		public Type CommandType => typeof(SetImportNativeFileCopyModeCommand);
	}
}
