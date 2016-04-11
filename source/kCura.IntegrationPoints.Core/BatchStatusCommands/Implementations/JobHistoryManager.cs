using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.ScheduleQueue.Core;
using Relativity.API;

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

		//todo: remove Helper from constructor - Gonnella
		public JobHistoryManager(ITempDocumentTableFactory tempDocumentTableFactory, IRepositoryFactory repositoryFactory, int jobHistoryInstanceId, int sourceWorkspaceArtifactId, string uniqueJobId)
		{
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
			_uniqueJobId = uniqueJobId;
			_tempDocHelper = tempDocumentTableFactory.GetDocTableHelper(_uniqueJobId, _sourceWorkspaceArtifactId);
			_jobHistoryRepository = repositoryFactory.GetJobHistoryRepository();
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

		public void JobStarted(Job job)
		{
		}

		public void JobComplete(Job job)
		{
			try
			{
				List<int> documentIds = ScratchTableRepository.GetDocumentIdsFromTable();
				_jobHistoryRepository.TagDocsWithJobHistory(documentIds.Count, _jobHistoryInstanceId, _sourceWorkspaceArtifactId, _uniqueJobId);
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
					_scratchTable = new ScratchTableRepository(Data.Constants.TEMPORARY_DOC_TABLE_JOB_HIST, _tempDocHelper);
				}
				return _scratchTable;
			}
		}
	}
}