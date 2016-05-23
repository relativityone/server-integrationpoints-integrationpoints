using System.Security.Claims;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class JobHistoryBatchUpdateManager : IConsumeScratchTableBatchStatus
	{
		private readonly ClaimsPrincipal _claimsPrincipal;
		private readonly int _jobHistoryInstanceId;
		private readonly ITempDocTableHelper _tempDocTableHelper;
		private readonly int _sourceWorkspaceArtifactId;
		private ScratchTableRepository _scratchTable;
		private readonly IRepositoryFactory _repositoryFactory;

		public JobHistoryBatchUpdateManager(ITempDocTableHelper tempDocTableHelper, IRepositoryFactory repositoryFactory,
			IOnBehalfOfUserClaimsPrincipalFactory userClaimsPrincipalFactory, int jobHistoryInstanceId, int sourceWorkspaceArtifactId, int submittedBy)
		{
			_tempDocTableHelper = tempDocTableHelper;
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
			_claimsPrincipal = userClaimsPrincipalFactory.CreateClaimsPrincipal(submittedBy);
			_repositoryFactory = repositoryFactory;
		}

		public void OnJobStart(Job job)
		{
		}

		public void OnJobComplete(Job job)
		{
			try
			{
				IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(_sourceWorkspaceArtifactId);
				jobHistoryRepository.TagDocsWithJobHistory(_claimsPrincipal, ScratchTableRepository.Count, _jobHistoryInstanceId, _sourceWorkspaceArtifactId, ScratchTableRepository.GetTempTableName());
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
					_scratchTable = new ScratchTableRepository(Data.Constants.TEMPORARY_DOC_TABLE_JOB_HIST, _tempDocTableHelper, true);
				}
				return _scratchTable;
			}
		}
	}
}