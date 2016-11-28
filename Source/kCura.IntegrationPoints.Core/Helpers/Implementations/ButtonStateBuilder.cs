using System;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
	public class ButtonStateBuilder : IButtonStateBuilder
	{
		private readonly IIntegrationPointManager _integrationPointManager;
		private readonly IJobHistoryManager _jobHistoryManager;
		private readonly IPermissionRepository _permissionRepository;
		private readonly IQueueManager _queueManager;
		private readonly IStateManager _stateManager;

		public ButtonStateBuilder(IIntegrationPointManager integrationPointManager, IQueueManager queueManager, IJobHistoryManager jobHistoryManager, IStateManager stateManager,
			IPermissionRepository permissionRepository)
		{
			_integrationPointManager = integrationPointManager;
			_queueManager = queueManager;
			_jobHistoryManager = jobHistoryManager;
			_stateManager = stateManager;
			_permissionRepository = permissionRepository;
		}

		public ButtonStateDTO CreateButtonState(int applicationArtifactId, int integrationPointArtifactId)
		{
			IntegrationPointDTO integrationPointDto = _integrationPointManager.Read(applicationArtifactId, integrationPointArtifactId);

			PermissionCheckDTO jobHistoryErrorViewPermissionCheck = _integrationPointManager.UserHasPermissionToViewErrors(applicationArtifactId);

			Constants.SourceProvider sourceProvider = _integrationPointManager.GetSourceProvider(applicationArtifactId, integrationPointDto);
			
			bool hasAddProfilePermission = _permissionRepository.UserHasArtifactTypePermission(Guid.Parse(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.Create);

			bool canViewErrors = jobHistoryErrorViewPermissionCheck.Success;
			bool hasJobsExecutingOrInQueue = HasJobsExecutingOrInQueue(applicationArtifactId, integrationPointArtifactId);
			bool integrationPointIsStoppable = IntegrationPointIsStoppable(applicationArtifactId, integrationPointArtifactId);
			bool integrationPointHasErrors = integrationPointDto.HasErrors.GetValueOrDefault(false);
			ButtonStateDTO buttonState = _stateManager.GetButtonState(sourceProvider, hasJobsExecutingOrInQueue, integrationPointHasErrors, canViewErrors,
				integrationPointIsStoppable, hasAddProfilePermission);
			return buttonState;
		}

		private bool HasJobsExecutingOrInQueue(int applicationArtifactId, int integrationPointArtifactId)
		{
			return _queueManager.HasJobsExecutingOrInQueue(applicationArtifactId, integrationPointArtifactId);
		}

		private bool IntegrationPointIsStoppable(int applicationArtifactId, int integrationPointArtifactId)
		{
			StoppableJobCollection stoppableJobCollection = _jobHistoryManager.GetStoppableJobCollection(applicationArtifactId, integrationPointArtifactId);
			bool integrationPointIsStoppable = stoppableJobCollection.HasStoppableJobs;
			return integrationPointIsStoppable;
		}
	}
}