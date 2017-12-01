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
	[Guid("09854211-85C1-4360-ADAE-CED54096D86A")]
	public class SecretStorePostWorkspaceCreateEventHandler : PostWorkspaceCreateEventHandlerBase, IEventHandlerEx
	{
		public override Response Execute()
		{
			var executor = new EventHandlerExecutorExHandler();
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