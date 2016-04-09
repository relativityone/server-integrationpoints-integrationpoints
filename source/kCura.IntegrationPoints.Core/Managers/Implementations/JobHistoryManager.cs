using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class JobHistoryManager : IJobHistoryManager, IBatchStatus
	{
		private readonly ITempDocTableHelper _tempDocHelper;
		private readonly IJobHistoryRepository _jobHistoryRepository;
		private readonly int _jobHistoryInstanceId;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly string _uniqueJobId;

		public JobHistoryManager(ITempDocumentTableFactory tempDocumentTableFactory, IRepositoryFactory repositoryFactory, int jobHistoryInstanceId, int sourceWorkspaceArtifactId, string uniqueJobId)
		{
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
			_uniqueJobId = uniqueJobId;
			_tempDocHelper = tempDocumentTableFactory.GetDocTableHelper(_uniqueJobId, _sourceWorkspaceArtifactId);
			_jobHistoryRepository = repositoryFactory.GetJobHistoryRepository();
		}

		public void JobStarted(Job job) { }

		public void JobComplete(Job job)
		{
			List<int> documentIds = _tempDocHelper.GetDocumentIdsFromTable(ScratchTables.JobHistory); 
			int documentCount = documentIds.Count;
			if (documentCount == 0)
			{
				_tempDocHelper.DeleteTable(ScratchTables.JobHistory); 
				return;
			}
			_jobHistoryRepository.TagDocsWithJobHistory(documentCount, _jobHistoryInstanceId, _sourceWorkspaceArtifactId, _uniqueJobId);
		}
	}
}
