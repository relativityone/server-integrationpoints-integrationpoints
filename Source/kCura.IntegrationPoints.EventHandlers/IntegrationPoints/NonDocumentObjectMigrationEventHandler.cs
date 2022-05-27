using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    [Description("Performs tasks related to non-document object introduction in RIP Sync, such as migrating existing Integration Points configuration.")]
    [RunOnce(true)]
	[Guid("3F606321-8B7E-4850-825E-08DBC598A348")]
    public class NonDocumentObjectMigrationEventHandler : PostInstallEventHandlerBase, IEventHandler
    {
		public IEHContext Context => new EHContext
		{
			Helper = Helper
		};

		public Type CommandType => typeof(NonDocumentObjectMigrationCommand);

		protected override string SuccessMessage => "Success";

		protected override string GetFailureMessage(Exception ex)
		{
			return $"Failed to execute {nameof(NonDocumentObjectMigrationEventHandler)} in workspace {Helper.GetActiveCaseID()} due to error: {ex}";
		}

		protected override void Run()
		{
			var executor = new EventHandlerExecutor();
			executor.Execute(this);
		}
	}
}
