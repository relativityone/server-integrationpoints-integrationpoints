using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class JobHistoryManager : IJobHistoryManager, IConsumeScratchTableBatchStatus
	{
		private readonly ITempDocTableHelper _tempDocHelper;
		private readonly IJobHistoryRepository _jobHistoryRepository;
		private readonly int _jobHistoryInstanceId;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly string _uniqueJobId;
		private ScratchTableRepository _scratchTable;

		public JobHistoryManager(ITempDocumentTableFactory tempDocumentTableFactory, IRepositoryFactory repositoryFactory, int jobHistoryInstanceId, int sourceWorkspaceArtifactId, string uniqueJobId)
		{
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
			_uniqueJobId = uniqueJobId;
			_tempDocHelper = tempDocumentTableFactory.GetDocTableHelper(_uniqueJobId, _sourceWorkspaceArtifactId);
			_jobHistoryRepository = repositoryFactory.GetJobHistoryRepository();
		}

		public void JobStarted(Job job)
		{
		}

		public void JobComplete(Job job)
		{
			try
			{
				_jobHistoryRepository.TagDocsWithJobHistory(ScratchTableRepository.Count, _jobHistoryInstanceId, _sourceWorkspaceArtifactId, _uniqueJobId);
			}
			finally
			{
				ScratchTableRepository.Dispose();
			}
		}

		public IScratchTableRepository ScratchTableRepository
		{
			get
			{
				if (_scratchTable == null)
				{
					_scratchTable = new ScratchTableRepository(Data.Constants.TEMPORARY_DOC_TABLE_JOB_HIST, _tempDocHelper, true);
				}
				return _scratchTable;
			}
		}
	}
}