using System;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[kCura.EventHandler.CustomAttributes.RunOnce(true)]
	[kCura.EventHandler.CustomAttributes.Description("Removes Job History Error tab from Integration Points")]
	[System.Runtime.InteropServices.Guid("cfcecb5b-166c-4fba-b7a6-9cba816bcc3c")]
	public class RemoveJobHistoryErrorTab : kCura.EventHandler.PostInstallEventHandler
	{
		public override kCura.EventHandler.Response Execute()
		{
			int workspaceId = base.Helper.GetActiveCaseID();
			IRepositoryFactory repoFactory = new RepositoryFactory(base.Helper);
			ITabRepository tabRepository = repoFactory.GetTabRepository(workspaceId);
			IDBContext workspaceDbContext = base.Helper.GetDBContext(workspaceId);

			kCura.EventHandler.Response retVal = new kCura.EventHandler.Response();
			retVal.Message = "Succesfully removed Job History Error tab";
			retVal.Success = true;
			try
			{
				RemoveJobHistoryErrorTabIfExists(tabRepository, workspaceDbContext);
			}
			catch (Exception ex)
			{
				retVal.Success = false;
				retVal.Message = ex.Message;
			}
			return retVal;
		}

		private static void RemoveJobHistoryErrorTabIfExists(ITabRepository tabRepository, IDBContext workspaceDbContext)
		{
			string jobHistoryTabGuid = "FD585DBF-98EA-427B-8CE5-3E09A053DC14";

			int? tabArtifactId = tabRepository.RetrieveTabArtifactIdByGuid(jobHistoryTabGuid);
			if (tabArtifactId == null)
			{
				return; //tab did not exist, nothing to remove
			}

			string unlinkApplicationTabSql = $@"DELETE FROM [ApplicationTab] WHERE [TabArtifactID] = {tabArtifactId}";
			workspaceDbContext.ExecuteNonQuerySQLStatement(unlinkApplicationTabSql);

			tabRepository.Delete(tabArtifactId.Value);
		}
	}
}
