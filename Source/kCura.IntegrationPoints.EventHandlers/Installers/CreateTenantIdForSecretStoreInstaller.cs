using System;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.IntegrationPoints.SourceProviderInstaller;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Create TenantID for RIP's Secret Store")]
	[RunOnce(true)]
	[Guid("24A52901-3B2E-4DB7-B45F-562A64FC4386")]
	public class CreateTenantIdForSecretStoreInstaller : PostInstallEventHandlerBase, IEventHandler
	{
		public IEHContext Context => new EHContext
		{
			Helper = Helper
		};

		public Type CommandType => typeof(CreateTenantIdForSecretStoreCommand);


		protected override IAPILog CreateLogger()
		{
			return Helper.GetLoggerFactory().GetLogger().ForContext<CreateTenantIdForSecretStoreInstaller>();
		}

		protected override string SuccessMessage => "SecretStore successfully initialized.";

		protected override string GetFailureMessage(Exception ex)
		{
			return "Failed to initialize SecretStore.";
		}

		protected override void Run()
		{
			var executor = new EventHandlerExecutor();
			executor.Execute(this);
		}
	}
}