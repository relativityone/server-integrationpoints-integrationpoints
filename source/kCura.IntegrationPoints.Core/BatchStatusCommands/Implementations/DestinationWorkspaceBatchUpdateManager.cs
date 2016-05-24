using System;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class DestinationWorkspaceBatchUpdateManager : IConsumeScratchTableBatchStatus
	{
		private readonly IDestinationWorkspaceRepository _destinationWorkspaceRepository;
		private readonly int _sourceWorkspaceId;
		private readonly int _destinationWorkspaceId;
		private readonly int _jobHistoryInstanceId;
		private readonly IWorkspaceRepository _workspaceRepository;
		private readonly ClaimsPrincipal _claimsPrincipal;
		private int _destinationWorkspaceRdoId;
		private bool _errorOccurDuringJobStart;

		public DestinationWorkspaceBatchUpdateManager(IRepositoryFactory repositoryFactory, IOnBehalfOfUserClaimsPrincipalFactory userClaimsPrincipalFactory, 
			SourceConfiguration sourceConfig, int jobHistoryInstanceId, int submittedBy, string uniqueJobId)
		{
			_destinationWorkspaceRepository = repositoryFactory.GetDestinationWorkspaceRepository(sourceConfig.SourceWorkspaceArtifactId);
			_workspaceRepository = repositoryFactory.GetWorkspaceRepository();
			ScratchTableRepository = repositoryFactory.GetScratchTableRepository(sourceConfig.SourceWorkspaceArtifactId,Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS, uniqueJobId);
			_claimsPrincipal = userClaimsPrincipalFactory.CreateClaimsPrincipal(submittedBy);
			_sourceWorkspaceId = sourceConfig.SourceWorkspaceArtifactId;
			_destinationWorkspaceId = sourceConfig.TargetWorkspaceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
		}
		public IScratchTableRepository ScratchTableRepository { get; }

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
					_destinationWorkspaceRepository.TagDocsWithDestinationWorkspace(_claimsPrincipal, documentCount, _destinationWorkspaceRdoId, ScratchTableRepository.GetTempTableName(), _sourceWorkspaceId);
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
	}
}