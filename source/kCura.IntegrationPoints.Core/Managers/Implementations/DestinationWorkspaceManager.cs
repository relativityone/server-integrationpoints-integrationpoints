using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class DestinationWorkspaceManager : IBatchStatus
	{
		private readonly ITempDocTableHelper _tempDocHelper;
		private readonly IDestinationWorkspaceRepository _destinationWorkspaceRepository;
		private readonly string _tableSuffix;
		private readonly int _sourceWorkspaceId;
		private readonly int _jobHistoryInstanceId;

		public DestinationWorkspaceManager(ITempDocumentTableFactory tempDocumentTableFactory, IRepositoryFactory repositoryFactory, SourceConfiguration sourceConfig, string tableSuffix, int jobHistoryInstanceId)
		{
			_tempDocHelper = tempDocumentTableFactory.GetDocTableHelper(tableSuffix, sourceConfig.SourceWorkspaceArtifactId);
			_destinationWorkspaceRepository = repositoryFactory.GetDestinationWorkspaceRepository(sourceConfig.SourceWorkspaceArtifactId, sourceConfig.TargetWorkspaceArtifactId);
			_tableSuffix = tableSuffix;
			_sourceWorkspaceId = sourceConfig.SourceWorkspaceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
		}

		public void JobStarted(Job job) { }

		public void JobComplete(Job job)
		{
			List<int> documentIds = _tempDocHelper.GetDocumentIdsFromTable(ScratchTables.DestinationWorkspace);
			int documentCount = documentIds.Count;

			int? destinationWorkspaceRdoId = _destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance();
			if (destinationWorkspaceRdoId == -1)
			{
				destinationWorkspaceRdoId = _destinationWorkspaceRepository.CreateDestinationWorkspaceRdoInstance();
			}

			_destinationWorkspaceRepository.LinkDestinationWorkspaceToJobHistory(destinationWorkspaceRdoId, _jobHistoryInstanceId);

			if (documentCount == 0)
			{
				_tempDocHelper.DeleteTable(ScratchTables.DestinationWorkspace);
				return;
			}

			_destinationWorkspaceRepository.TagDocsWithDestinationWorkspace(documentCount, destinationWorkspaceRdoId, _tableSuffix, _sourceWorkspaceId);
		}
	}
}
