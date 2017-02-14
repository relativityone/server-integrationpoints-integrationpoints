﻿using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
	public class ButtonStateBuilder : IButtonStateBuilder
	{
		private readonly IProviderTypeService _providerTypeService;
		private readonly IRSAPIService _rsapiService;
		private readonly IJobHistoryManager _jobHistoryManager;
		private readonly IPermissionRepository _permissionRepository;
		private readonly IQueueManager _queueManager;
		private readonly IStateManager _stateManager;
		private readonly IIntegrationPointPermissionValidator _permissionValidator;

		public ButtonStateBuilder(IProviderTypeService providerTypeService, IQueueManager queueManager, IJobHistoryManager jobHistoryManager,
			IStateManager stateManager,
			IPermissionRepository permissionRepository, IIntegrationPointPermissionValidator permissionValidator, IRSAPIService rsapiService)
		{
			_providerTypeService = providerTypeService;
			_queueManager = queueManager;
			_jobHistoryManager = jobHistoryManager;
			_stateManager = stateManager;
			_permissionRepository = permissionRepository;
			_permissionValidator = permissionValidator;
			_rsapiService = rsapiService;
		}

		public ButtonStateDTO CreateButtonState(int applicationArtifactId, int integrationPointArtifactId)
		{
			var integrationPoint = _rsapiService.IntegrationPointLibrary.Read(integrationPointArtifactId);
			var providerType = _providerTypeService.GetProviderType(integrationPoint.SourceProvider.Value, integrationPoint.DestinationProvider.Value);

			ValidationResult jobHistoryErrorViewPermissionCheck = _permissionValidator.ValidateViewErrors(applicationArtifactId);

			//TODO this is hack for now - remove after enabling I2I in profiles
			bool isFederatedInstance = false;
			try
			{
				isFederatedInstance = (new JSONSerializer()).Deserialize<ImportSettings>(integrationPoint.DestinationConfiguration).FederatedInstanceArtifactId.HasValue;
			}
			catch (Exception)
			{
				//ignore
			}
			bool hasAddProfilePermission = _permissionRepository.UserHasArtifactTypePermission(Guid.Parse(ObjectTypeGuids.IntegrationPointProfile),
				ArtifactPermission.Create) && !isFederatedInstance;

			bool canViewErrors = jobHistoryErrorViewPermissionCheck.IsValid;
			bool hasJobsExecutingOrInQueue = HasJobsExecutingOrInQueue(applicationArtifactId, integrationPointArtifactId);
			bool integrationPointIsStoppable = IntegrationPointIsStoppable(providerType, applicationArtifactId, integrationPointArtifactId);
			bool integrationPointHasErrors = integrationPoint.HasErrors.GetValueOrDefault(false);
			ButtonStateDTO buttonState = _stateManager.GetButtonState(providerType, hasJobsExecutingOrInQueue, integrationPointHasErrors, canViewErrors,
				integrationPointIsStoppable, hasAddProfilePermission);
			return buttonState;
		}

		private bool HasJobsExecutingOrInQueue(int applicationArtifactId, int integrationPointArtifactId)
		{
			return _queueManager.HasJobsExecutingOrInQueue(applicationArtifactId, integrationPointArtifactId);
		}

		private bool IntegrationPointIsStoppable(ProviderType providerType, int applicationArtifactId, int integrationPointArtifactId)
		{
			if (providerType != ProviderType.Relativity && providerType != ProviderType.LoadFile)
			{
				return false;
			}
			StoppableJobCollection stoppableJobCollection = _jobHistoryManager.GetStoppableJobCollection(applicationArtifactId, integrationPointArtifactId);
			bool integrationPointIsStoppable = stoppableJobCollection.HasStoppableJobs;
			return integrationPointIsStoppable;
		}
	}
}