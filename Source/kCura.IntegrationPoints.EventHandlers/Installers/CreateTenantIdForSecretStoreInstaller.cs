using System;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Create TenantID for RIP's Secret Store")]
	[RunOnce(true)]
	[Guid("24A52901-3B2E-4DB7-B45F-562A64FC4386")]
	public class CreateTenantIdForSecretStoreInstaller : PostInstallEventHandler, IEventHandler
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

		public string SuccessMessage => "SecretStore successfully initialized.";
		public string FailureMessage => "Failed to initialize SecretStore.";
		public Type CommandType => typeof(CreateTenantIdForSecretStoreCommand);
	}
}