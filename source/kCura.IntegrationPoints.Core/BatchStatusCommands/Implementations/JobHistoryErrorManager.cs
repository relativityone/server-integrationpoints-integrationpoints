using System.Collections.Generic;
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

		public JobHistoryErrorManager(IRepositoryFactory repositoryFactory, IOnBehalfOfUserClaimsPrincipalFactory userClaimsPrincipalFactory, int jobHistoryInstanceId, 
			int sourceWorkspaceArtifactId, string uniqueJobId, int submittedBy)
		{
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
			_claimsPrincipal = userClaimsPrincipalFactory.CreateClaimsPrincipal(submittedBy);
			_uniqueJobId = uniqueJobId;
			_jobHistoryErrorRepository = repositoryFactory.GetJobHistoryErrorRepository(_sourceWorkspaceArtifactId);
		}

		public void JobStarted(Job job)
		{
			_jobHistoryErrorRepository.UpdateErrorStatuses(_claimsPrincipal, _jobHistoryInstanceId, ErrorStatusChoices.JobHistoryErrorInProgress, _uniqueJobId);
		}

		public void JobComplete(Job job)
		{
			List<JobHistoryError> jobHistoryErrors = _jobHistoryErrorRepository.RetreiveJobHistoryErrors(int jobHistoryArtifactId)
			
			_jobHistoryErrorRepository.UpdateErrorStatuses(_claimsPrincipal, _jobHistoryInstanceId, ErrorStatusChoices.JobHistoryErrorRetried, _uniqueJobId);
		}
	}
}