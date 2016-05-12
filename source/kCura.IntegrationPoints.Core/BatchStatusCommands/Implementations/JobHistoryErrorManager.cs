using System.Security.Claims;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class JobHistoryErrorManager : IJobHistoryErrorManager, IBatchStatus
	{
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private readonly ClaimsPrincipal _claimsPrincipal;
		private readonly int _jobHistoryInstanceId;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly string _uniqueJobId;

		private readonly IRepositoryFactory _repositoryFactory;

		public JobHistoryErrorManager(IRepositoryFactory repositoryFactory, IOnBehalfOfUserClaimsPrincipalFactory userClaimsPrincipalFactory, int jobHistoryInstanceId, 
			int sourceWorkspaceArtifactId, string uniqueJobId, int submittedBy)
		{
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
			_claimsPrincipal = userClaimsPrincipalFactory.CreateClaimsPrincipal(submittedBy);
			_uniqueJobId = uniqueJobId;
			_jobHistoryErrorRepository = repositoryFactory.GetJobHistoryErrorRepository(_sourceWorkspaceArtifactId);
			_repositoryFactory = repositoryFactory;
		}

		public void JobStarted(Job job)
		{
			_jobHistoryErrorRepository.UpdateErrorStatuses(_claimsPrincipal, _jobHistoryInstanceId, ErrorStatusChoices.JobHistoryErrorInProgress, _uniqueJobId);
		}

		public void JobComplete(Job job)
		{
			_jobHistoryErrorRepository.UpdateErrorStatuses(_claimsPrincipal, _jobHistoryInstanceId, ErrorStatusChoices.JobHistoryErrorRetried, _uniqueJobId);
		}

		public int CreateItemLevelErrorsSavedSearch(int workspaceArtifactId, int savedSearchArtifactId, int jobHistoryArtifactId)
		{
			IJobHistoryErrorRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(workspaceArtifactId);
			return jobHistoryErrorRepository.CreateItemLevelErrorsSavedSearch(workspaceArtifactId, savedSearchArtifactId, jobHistoryArtifactId);
		}
	}
}