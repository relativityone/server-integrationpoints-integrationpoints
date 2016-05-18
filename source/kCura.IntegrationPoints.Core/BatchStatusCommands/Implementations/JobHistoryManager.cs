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
	public class JobHistoryManager : IConsumeScratchTableBatchStatus
	{
		private readonly ITempDocTableHelper _tempDocHelper;
		private readonly ClaimsPrincipal _claimsPrincipal;
		private readonly int _jobHistoryInstanceId;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly string _uniqueJobId;
		private ScratchTableRepository _scratchTable;
		private readonly IRepositoryFactory _repositoryFactory;

		public JobHistoryManager(ITempDocumentTableFactory tempDocumentTableFactory, IRepositoryFactory repositoryFactory,
			IOnBehalfOfUserClaimsPrincipalFactory userClaimsPrincipalFactory, int jobHistoryInstanceId, int sourceWorkspaceArtifactId, string uniqueJobId, int submittedBy)
		{
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
			_claimsPrincipal = userClaimsPrincipalFactory.CreateClaimsPrincipal(submittedBy);
			_uniqueJobId = uniqueJobId;
			_tempDocHelper = tempDocumentTableFactory.GetDocTableHelper(_uniqueJobId, _sourceWorkspaceArtifactId);
			_repositoryFactory = repositoryFactory;
		}

		public void JobStarted(Job job)
		{
		}

		public void JobComplete(Job job)
		{
			try
			{
				IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(_sourceWorkspaceArtifactId);
				jobHistoryRepository.TagDocsWithJobHistory(_claimsPrincipal, ScratchTableRepository.Count, _jobHistoryInstanceId, _sourceWorkspaceArtifactId, _uniqueJobId);
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