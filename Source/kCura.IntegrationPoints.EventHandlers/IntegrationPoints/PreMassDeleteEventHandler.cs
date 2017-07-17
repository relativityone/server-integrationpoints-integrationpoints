using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoint
{
	[kCura.EventHandler.CustomAttributes.Description("A description of the event handler.")]
	[System.Runtime.InteropServices.Guid("4ee07342-b4a0-45a3-a6a3-b56cc739feb7")]
	public class PreMassDeleteEventHandler : PreMassDeleteEventHandlerBase
	{
		private DeleteHistoryService deleteHistoryService;
		
		public DeleteHistoryService DeleteHistoryService
		{
			get { return deleteHistoryService ?? (deleteHistoryService = new DeleteHistoryService(ServiceContextFactory.CreateRSAPIService(base.Helper, base.Application.ArtifactID))); }
			set { deleteHistoryService = value; }
		}

		public override Response ExecutePreDelete(List<int> artifactIDs)
		{
			DeleteHistoryService.DeleteHistoriesAssociatedWithIPs(artifactIDs);
			return new Response();
		}
	}
}
