using System;
using kCura.IntegrationPoints.Contracts.Models;
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
		private readonly int _destinationWorkspaceId;
		private readonly int _jobHistoryInstanceId;
		private IScratchTableRepository _scratchTableRepository;
		private readonly IWorkspaceRepository _workspaceRepository;
		private int? _destinationWorkspaceRdoId;
		private bool _errorOccurDuringJobStart;

		public DestinationWorkspaceManager(ITempDocumentTableFactory tempDocumentTableFactory, IRepositoryFactory repositoryFactory, SourceConfiguration sourceConfig, string tableSuffix, int jobHistoryInstanceId)
		{
			_tempDocHelper = tempDocumentTableFactory.GetDocTableHelper(tableSuffix, sourceConfig.SourceWorkspaceArtifactId);
			_destinationWorkspaceRepository = repositoryFactory.GetDestinationWorkspaceRepository(sourceConfig.SourceWorkspaceArtifactId);
			_workspaceRepository = repositoryFactory.GetWorkspaceRepository();
			_tableSuffix = tableSuffix;
			_sourceWorkspaceId = sourceConfig.SourceWorkspaceArtifactId;
			_destinationWorkspaceId = sourceConfig.TargetWorkspaceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
		}

		public void JobStarted(Job job)
		{
			try
			{
				DestinationWorkspaceDTO destinationWorkspace = _destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance(_destinationWorkspaceId);
				//string destinationWorkspaceName = GetWorkspaceName();
				string destinationWorkspaceName = "Wrong Name-Will Change";
				if (destinationWorkspace == null)
				{
					destinationWorkspace = _destinationWorkspaceRepository.CreateDestinationWorkspaceRdoInstance(_destinationWorkspaceId, destinationWorkspaceName);
				}
				else if(destinationWorkspaceName != destinationWorkspace.WorkspaceName)
				{
					destinationWorkspace.WorkspaceName = destinationWorkspaceName;
					_destinationWorkspaceRepository.UpdateDestinationWorkspaceRdoInstance(destinationWorkspace);
				}

				_destinationWorkspaceRdoId = destinationWorkspace.ArtifactId;
				_destinationWorkspaceRepository.LinkDestinationWorkspaceToJobHistory(_destinationWorkspaceRdoId, _jobHistoryInstanceId);
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

		internal string GetWorkspaceName()
		{
			return _workspaceRepository.Retrieve(_destinationWorkspaceId).Name;
		}

		public IScratchTableRepository ScratchTableRepository
		{
			get
			{
				if (_scratchTableRepository == null)
				{
					_scratchTableRepository = new ScratchTableRepository(Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS, _tempDocHelper, false);
				}
				return _scratchTableRepository;
			}
		}
	}
}