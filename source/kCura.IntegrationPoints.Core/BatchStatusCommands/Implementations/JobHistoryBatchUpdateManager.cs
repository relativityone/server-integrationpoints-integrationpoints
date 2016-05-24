using System.Security.Claims;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class JobHistoryBatchUpdateManager : IConsumeScratchTableBatchStatus
	{
		private readonly ClaimsPrincipal _claimsPrincipal;
		private readonly int _jobHistoryInstanceId;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly IRepositoryFactory _repositoryFactory;

		public JobHistoryBatchUpdateManager(IRepositoryFactory repositoryFactory, IOnBehalfOfUserClaimsPrincipalFactory userClaimsPrincipalFactory,
			int sourceWorkspaceArtifactId, int jobHistoryInstanceId, int submittedBy, string uniqueJobId)
		{
			ScratchTableRepository = repositoryFactory.GetScratchTableRepository(sourceWorkspaceArtifactId, Data.Constants.TEMPORARY_DOC_TABLE_JOB_HIST, uniqueJobId);
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
			_claimsPrincipal = userClaimsPrincipalFactory.CreateClaimsPrincipal(submittedBy);
			_repositoryFactory = repositoryFactory;
		}

		public IScratchTableRepository ScratchTableRepository { get; }

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
	}
}