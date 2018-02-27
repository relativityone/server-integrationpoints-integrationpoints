using System;
using System.Security.Claims;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class SourceObjectBatchUpdateManager : IConsumeScratchTableBatchStatus
	{
		private int _destinationWorkspaceRdoId;
		private bool _errorOccurDuringJobStart;

		private readonly ClaimsPrincipal _claimsPrincipal;
		private readonly int _destinationWorkspaceId;
		private readonly IDestinationWorkspaceRepository _destinationWorkspaceRepository;
		private readonly int _jobHistoryInstanceId;
		private readonly IAPILog _logger;
		private readonly int _sourceWorkspaceId;
		private readonly int? _federatedInstanceId;
		private readonly IWorkspaceRepository _workspaceRepository;
		private readonly IFederatedInstanceManager _federatedInstanceManager;

		public SourceObjectBatchUpdateManager(IRepositoryFactory sourceRepositoryFactory, IRepositoryFactory targetRepositoryFactory,
			IOnBehalfOfUserClaimsPrincipalFactory userClaimsPrincipalFactory, IHelper helper, IFederatedInstanceManager federatedInstanceManager, SourceConfiguration sourceConfig,
			int jobHistoryInstanceId, int submittedBy, string uniqueJobId)
		{
			_federatedInstanceManager = federatedInstanceManager;
			_destinationWorkspaceRepository = sourceRepositoryFactory.GetDestinationWorkspaceRepository(sourceConfig.SourceWorkspaceArtifactId);
			_workspaceRepository = targetRepositoryFactory.GetWorkspaceRepository();
			ScratchTableRepository = sourceRepositoryFactory.GetScratchTableRepository(sourceConfig.SourceWorkspaceArtifactId, Data.Constants.TEMPORARY_DOC_TABLE_SOURCE_OBJECTS, uniqueJobId);
			_claimsPrincipal = userClaimsPrincipalFactory.CreateClaimsPrincipal(submittedBy);
			_sourceWorkspaceId = sourceConfig.SourceWorkspaceArtifactId;
			_destinationWorkspaceId = sourceConfig.TargetWorkspaceArtifactId;
			_federatedInstanceId = sourceConfig.FederatedInstanceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<SourceObjectBatchUpdateManager>();
		}

		public IScratchTableRepository ScratchTableRepository { get; }

		public void OnJobStart(Job job)
		{
			try
			{
				DestinationWorkspace destinationWorkspace = _destinationWorkspaceRepository.Query(_destinationWorkspaceId, _federatedInstanceId);
				string destinationWorkspaceName = _workspaceRepository.Retrieve(_destinationWorkspaceId).Name;
				string destinationInstanceName = _federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(_federatedInstanceId).Name;

				if (destinationWorkspace == null)
				{
					destinationWorkspace = _destinationWorkspaceRepository.Create(_destinationWorkspaceId, destinationWorkspaceName, _federatedInstanceId, destinationInstanceName);
				}
				else if (destinationWorkspaceName != destinationWorkspace.DestinationWorkspaceName || destinationInstanceName != destinationWorkspace.DestinationInstanceName)
				{
					destinationWorkspace.DestinationWorkspaceName = destinationWorkspaceName;
					destinationWorkspace.DestinationInstanceName = destinationInstanceName;
					_destinationWorkspaceRepository.Update(destinationWorkspace);
				}

				_destinationWorkspaceRdoId = destinationWorkspace.ArtifactId;
				_destinationWorkspaceRepository.LinkDestinationWorkspaceToJobHistory(_destinationWorkspaceRdoId, _jobHistoryInstanceId);
			}
			catch (Exception e)
			{
				_errorOccurDuringJobStart = true;
				throw LogAndWrapExceptionFromJobStart(e);
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
				throw LogAndWrapExceptionFromJobComplete(e);
			}
			finally
			{
				ScratchTableRepository.Dispose();
			}
		}

		#region Logging

		private IntegrationPointsException LogAndWrapExceptionFromJobStart(Exception e)
		{
			return LogAndWrapException(e,
				"Error occurred during linking destination workspace to JobHistory in SourceObjectBatchUpdateManager.");
		}

		private IntegrationPointsException LogAndWrapExceptionFromJobComplete(Exception e)
		{
			return LogAndWrapException(e, "Error occurred during job completion in SourceObjectBatchUpdateManager");
		}

		private IntegrationPointsException LogAndWrapException(Exception e, string message)
		{
			_logger.LogError(e, message);
			return new IntegrationPointsException(message, e);
		}

		#endregion
	}
}