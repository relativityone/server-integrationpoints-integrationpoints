using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.SourceProviderInstaller;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Update LDAP Configuration to use Secret Store")]
	[RunOnce(true)]
	[Guid("C4B307AD-E34E-488E-A2DB-8AD12FED2348")]
	public class UpdateLdapSecuredConfiguration : PostInstallEventHandlerBase, IEventHandler
	{
		protected override string SuccessMessage => "Updating LDAP Configuration completed";

		protected override string GetFailureMessage(Exception ex)
		{
			return "Updating LDAP configuration failed";
		}

		public IEHContext Context => new EHContext
		{
			Helper = Helper
		};

		public Type CommandType => typeof(UpdateLdapConfigurationCommand);

		protected override IAPILog CreateLogger()
		{
			return Helper.GetLoggerFactory().GetLogger().ForContext<UpdateLdapSecuredConfiguration>();
		}

		protected override void Run()
		{
			var executor = new EventHandlerExecutor();
			executor.Execute(this);
		}
	}
}
