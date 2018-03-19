using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.SourceProviderInstaller;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Update FTP Configuration to use Secret Store")]
	[RunOnce(true)]
	[Guid("E1D2614D-4488-4EBC-A197-DA85CD6F6207")]
	public class UpdateFtpSecuredConfiguration : PostInstallEventHandlerBase, IEventHandler
	{
		protected override string SuccessMessage => "Updating LDAP Configuration completed";

		protected override string GetFailureMessage(Exception ex)
		{
			return "Updating FTP configuration failed";
		}

		public IEHContext Context => new EHContext
		{
			Helper = Helper
		};

		public Type CommandType => typeof(UpdateFtpConfigurationCommand);

		protected override IAPILog CreateLogger()
		{
			return Helper.GetLoggerFactory().GetLogger().ForContext<UpdateFtpConfigurationCommand>();
		}

		protected override void Run()
		{
			var executor = new EventHandlerExecutor();
			executor.Execute(this);
		}
	}
}