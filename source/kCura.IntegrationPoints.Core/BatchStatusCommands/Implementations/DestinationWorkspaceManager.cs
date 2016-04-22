using System;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class DestinationWorkspaceManager : IConsumeScratchTableBatchStatus
	{
		private readonly ITempDocTableHelper _tempDocHelper;
		private readonly IDestinationWorkspaceRepository _destinationWorkspaceRepository;
		private readonly string _tableSuffix;
		private readonly int _sourceWorkspaceId;
		private readonly int _jobHistoryInstanceId;
		private IScratchTableRepository _scratchTableRepository;
		private int? _destinationWorkspaceRdoId;
		private bool _errorOccurDuringJobStart;

		public DestinationWorkspaceManager(ITempDocumentTableFactory tempDocumentTableFactory, IRepositoryFactory repositoryFactory, SourceConfiguration sourceConfig, string tableSuffix, int jobHistoryInstanceId)
		{
			_tempDocHelper = tempDocumentTableFactory.GetDocTableHelper(tableSuffix, sourceConfig.SourceWorkspaceArtifactId);
			_destinationWorkspaceRepository = repositoryFactory.GetDestinationWorkspaceRepository(sourceConfig.SourceWorkspaceArtifactId, sourceConfig.TargetWorkspaceArtifactId);
			_tableSuffix = tableSuffix;
			_sourceWorkspaceId = sourceConfig.SourceWorkspaceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
		}

		public void JobStarted(Job job)
		{
			try
			{
				_destinationWorkspaceRdoId = _destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance();
				if (_destinationWorkspaceRdoId == -1)
				{
					_destinationWorkspaceRdoId = _destinationWorkspaceRepository.CreateDestinationWorkspaceRdoInstance();
				}

				_destinationWorkspaceRepository.LinkDestinationWorkspaceToJobHistory(_destinationWorkspaceRdoId,
					_jobHistoryInstanceId);
			}
			catch (Exception)
			{
				_errorOccurDuringJobStart = true;
				throw;
			}
		}

		public void JobComplete(Job job)
		{
			try
			{
				if (!_errorOccurDuringJobStart)
				{
					int documentCount = ScratchTableRepository.Count;
					_destinationWorkspaceRepository.TagDocsWithDestinationWorkspace(documentCount, _destinationWorkspaceRdoId, _tableSuffix, _sourceWorkspaceId);
				}
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
				if (_scratchTableRepository == null)
				{
					_scratchTableRepository = new ScratchTableRepository(Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS, _tempDocHelper);
				}
				return _scratchTableRepository;
			}
		}
	}
}