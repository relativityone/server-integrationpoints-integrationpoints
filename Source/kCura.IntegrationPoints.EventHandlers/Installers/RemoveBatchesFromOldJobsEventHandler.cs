using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Removes all sync batches in a given workspace, that are older than 7 days.")]
	[RunOnce(false)]
	[Guid("65218C51-3A8C-4BCB-9EE7-4A147B24CBFE")]
	public class RemoveBatchesFromOldJobsEventHandler : PostInstallEventHandlerBase, IEventHandler
	{
		public IEHContext Context => new EHContext
		{
			Helper = Helper
		};

		public Type CommandType => typeof(RemoveBatchesFromOldJobsCommand);

		protected override string SuccessMessage => "Successfully removed batches from old jobs";

		protected override string GetFailureMessage(Exception ex) =>
			$"Failed to delete sync batches that are older than 7 days from workspace {Helper.GetActiveCaseID()} due to: {ex}";

		protected override void Run()
		{
			var executor = new EventHandlerExecutor();
			executor.Execute(this);
		}
	}
}