using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class JobHistoryManager : IJobHistoryManager, IBatchStatus
	{
		private readonly ITempDocTableHelper _tempDocHelper;
		private readonly IRSAPIClient _client;
		private readonly IJobHistoryRepository _jobHistoryRepository;
		private readonly int _jobHistoryInstanceId;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly string _uniqueJobId;

		public JobHistoryManager(IHelper helper, int jobHistoryInstanceId, int sourceWorkspaceArtifactId, string uniqueJobId)
		{
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_client = new RsapiClientFactory(helper).CreateClientForWorkspace(_sourceWorkspaceArtifactId, ExecutionIdentity.System);
			_jobHistoryInstanceId = jobHistoryInstanceId;
			_uniqueJobId = uniqueJobId;
			_tempDocHelper = new TempDocumentFactory().GetDocTableHelper(helper, _uniqueJobId, _sourceWorkspaceArtifactId);
			_jobHistoryRepository = new JobHistoryRepository();
		}

		/// <summary>
		/// For internal unit testing
		/// </summary>
		internal JobHistoryManager(ITempDocTableHelper tempDocHelper, IJobHistoryRepository jobHistoryRepository, 
			int jobHistoryInstanceId, int sourceWorkspaceArtifactId, string uniqueJobId)
		{
			_tempDocHelper = tempDocHelper;
			_jobHistoryRepository = jobHistoryRepository;
			_jobHistoryInstanceId = jobHistoryInstanceId;
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_uniqueJobId = uniqueJobId;	
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
