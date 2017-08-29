using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;

namespace kCura.IntegrationPoints.Management.Tasks
{
	public class CleanUnlinkedJobHistoryErrorsTask : IManagementTask
	{
		private readonly IUnlinkedJobHistoryService _unlinkedJobHistoryService;
		private readonly IDeleteHistoryErrorService _deleteHistoryErrorService;

		public CleanUnlinkedJobHistoryErrorsTask(IDeleteHistoryErrorService deleteHistoryErrorService, IUnlinkedJobHistoryService unlinkedJobHistoryService)
		{
			_deleteHistoryErrorService = deleteHistoryErrorService;
			_unlinkedJobHistoryService = unlinkedJobHistoryService;
		}

		public void Run(IList<int> workspaceArtifactIds)
		{
			foreach (var workspaceArtifactId in workspaceArtifactIds)
			{
				CleanInWorkspace(workspaceArtifactId);
			}
		}

		private void CleanInWorkspace(int workspaceArtifactId)
		{
			List<int> jobHistories = _unlinkedJobHistoryService.FindUnlinkedJobHistories(workspaceArtifactId);

			if (jobHistories.Count > 0)
			{
				_deleteHistoryErrorService.DeleteErrorAssociatedWithHistories(jobHistories, workspaceArtifactId);
			}
		}
	}
}