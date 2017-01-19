using System;
using System.Security.Claims;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class SourceObjectBatchUpdateManager : IConsumeScratchTableBatchStatus
	{
		private readonly ClaimsPrincipal _claimsPrincipal;
		private readonly int _destinationWorkspaceId;
		private readonly IDestinationWorkspaceRepository _destinationWorkspaceRepository;
		private readonly int _jobHistoryInstanceId;
		private readonly IAPILog _logger;
		private readonly int _sourceWorkspaceId;
		private readonly IWorkspaceRepository _workspaceRepository;
		private int _destinationWorkspaceRdoId;
		private bool _errorOccurDuringJobStart;

		public SourceObjectBatchUpdateManager(IRepositoryFactory sourceRepositoryFactory, IRepositoryFactory targetRepositoryFactory, IOnBehalfOfUserClaimsPrincipalFactory userClaimsPrincipalFactory, IHelper helper,
			SourceConfiguration sourceConfig, int jobHistoryInstanceId, int submittedBy, string uniqueJobId)
		{
			_destinationWorkspaceRepository = sourceRepositoryFactory.GetDestinationWorkspaceRepository(sourceConfig.SourceWorkspaceArtifactId);
			_workspaceRepository = targetRepositoryFactory.GetWorkspaceRepository();
			ScratchTableRepository = sourceRepositoryFactory.GetScratchTableRepository(sourceConfig.SourceWorkspaceArtifactId, Data.Constants.TEMPORARY_DOC_TABLE_SOURCE_OBJECTS, uniqueJobId);
			_claimsPrincipal = userClaimsPrincipalFactory.CreateClaimsPrincipal(submittedBy);
			_sourceWorkspaceId = sourceConfig.SourceWorkspaceArtifactId;
			_destinationWorkspaceId = sourceConfig.TargetWorkspaceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<SourceObjectBatchUpdateManager>();
		}

		public IScratchTableRepository ScratchTableRepository { get; }

		public void OnJobStart(Job job)
		{
			try
			{
				DestinationWorkspaceDTO destinationWorkspace = _destinationWorkspaceRepository.Query(_destinationWorkspaceId);
				string destinationWorkspaceName = _workspaceRepository.Retrieve(_destinationWorkspaceId).Name;
				if (destinationWorkspace == null)
				{
					destinationWorkspace = _destinationWorkspaceRepository.Create(_destinationWorkspaceId, destinationWorkspaceName);
				}
				else if (destinationWorkspaceName != destinationWorkspace.WorkspaceName)
				{
					destinationWorkspace.WorkspaceName = destinationWorkspaceName;
					_destinationWorkspaceRepository.Update(destinationWorkspace);
				}

				_destinationWorkspaceRdoId = destinationWorkspace.ArtifactId;
				_destinationWorkspaceRepository.LinkDestinationWorkspaceToJobHistory(_destinationWorkspaceRdoId, _jobHistoryInstanceId);
			}
			catch (Exception e)
			{
				LogErrorDuringJobStart(e);
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
					_destinationWorkspaceRepository.TagDocsWithDestinationWorkspaceAndJobHistory(_claimsPrincipal, documentCount, _destinationWorkspaceRdoId, _jobHistoryInstanceId,
						ScratchTableRepository.GetTempTableName(), _sourceWorkspaceId);
				}
			}
			catch (Exception e)
			{
				LogErrorDuringJobComplete(e);
				throw;
			}
			finally
			{
				ScratchTableRepository.Dispose();
			}
		}

		#region Logging

		private void LogErrorDuringJobStart(Exception e)
		{
			_logger.LogError(e, "Error occurred during job start in SourceObjectBatchUpdateManager.");
		}

		private void LogErrorDuringJobComplete(Exception e)
		{
			_logger.LogError(e, "Error occurred during job completion in SourceObjectBatchUpdateManager");
		}

		#endregion
	}
}