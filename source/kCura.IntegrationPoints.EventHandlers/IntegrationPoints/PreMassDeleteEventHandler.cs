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
		private DeleteHistoryErrorService deleteHistoryError;

		public DeleteHistoryErrorService DeleteHistoryError
		{
			get
			{
				return deleteHistoryError ??
							 (deleteHistoryError =
								 new DeleteHistoryErrorService(ServiceContextFactory.CreateRSAPIService(new kCura.IntegrationPoints.Core.RsapiClientFactory(base.Helper).CreateClientForWorkspace(base.Application.ArtifactID))));
			}
			set { deleteHistoryError = value; } 
		}

		public DeleteHistoryService DeleteHistoryService
		{
			get { return deleteHistoryService ?? (deleteHistoryService = new DeleteHistoryService(ServiceContextFactory.CreateRSAPIService(new kCura.IntegrationPoints.Core.RsapiClientFactory(base.Helper).CreateClientForWorkspace(base.Application.ArtifactID)),DeleteHistoryError)); }
			set { deleteHistoryService = value; }
		}

		public override Response ExecutePreDelete(List<int> artifactIDs)
		{
			DeleteHistoryService.DeleteHistoriesAssociatedWithIPs(artifactIDs);
			return new Response();
		}
	}
}
