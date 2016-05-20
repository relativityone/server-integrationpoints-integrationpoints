using System;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class DestinationWorkspaceBatchUpdateManager : IConsumeScratchTableBatchStatus
	{
		private readonly ITempDocTableHelper _tempDocHelper;
		private readonly IDestinationWorkspaceRepository _destinationWorkspaceRepository;
		private readonly string _tableSuffix;
		private readonly int _sourceWorkspaceId;
		private readonly int _destinationWorkspaceId;
		private readonly int _jobHistoryInstanceId;
		private IScratchTableRepository _scratchTableRepository;
		private readonly IWorkspaceRepository _workspaceRepository;
		private readonly ClaimsPrincipal _claimsPrincipal;
		private int _destinationWorkspaceRdoId;
		private bool _errorOccurDuringJobStart;

		public DestinationWorkspaceBatchUpdateManager(ITempDocumentTableFactory tempDocumentTableFactory, IRepositoryFactory repositoryFactory,
			IOnBehalfOfUserClaimsPrincipalFactory userClaimsPrincipalFactory, SourceConfiguration sourceConfig, string tableSuffix, int jobHistoryInstanceId, int submittedBy)
		{
			_tempDocHelper = tempDocumentTableFactory.GetDocTableHelper(tableSuffix, sourceConfig.SourceWorkspaceArtifactId);
			_destinationWorkspaceRepository = repositoryFactory.GetDestinationWorkspaceRepository(sourceConfig.SourceWorkspaceArtifactId);
			_workspaceRepository = repositoryFactory.GetWorkspaceRepository();
			_claimsPrincipal = userClaimsPrincipalFactory.CreateClaimsPrincipal(submittedBy);
			_tableSuffix = tableSuffix;
			_sourceWorkspaceId = sourceConfig.SourceWorkspaceArtifactId;
			_destinationWorkspaceId = sourceConfig.TargetWorkspaceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
		}

		public void OnJobStart(Job job)
		{
			try
			{
				DestinationWorkspaceDTO destinationWorkspace = _destinationWorkspaceRepository.Query(_destinationWorkspaceId);
				string destinationWorkspaceName = GetWorkspaceName();
				if (destinationWorkspace == null)
				{
					destinationWorkspace = _destinationWorkspaceRepository.Create(_destinationWorkspaceId, destinationWorkspaceName);
				}
				else if(destinationWorkspaceName != destinationWorkspace.WorkspaceName)
				{
					destinationWorkspace.WorkspaceName = destinationWorkspaceName;
					_destinationWorkspaceRepository.Update(destinationWorkspace);
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

		public void OnJobComplete(Job job)
		{
			try
			{
				if (!_errorOccurDuringJobStart)
				{
					int documentCount = ScratchTableRepository.Count;
					_destinationWorkspaceRepository.TagDocsWithDestinationWorkspace(_claimsPrincipal, documentCount, _destinationWorkspaceRdoId, _tableSuffix, _sourceWorkspaceId);
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