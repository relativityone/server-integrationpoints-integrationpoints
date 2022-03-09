﻿using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using System;
using kCura.IntegrationPoints.Core.Contracts.Configuration;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
	public class ButtonStateBuilder : IButtonStateBuilder
	{
		private readonly IProviderTypeService _providerTypeService;
		private readonly IIntegrationPointRepository _integrationPointRepository;
		private readonly IJobHistoryManager _jobHistoryManager;
		private readonly IPermissionRepository _permissionRepository;
		private readonly IQueueManager _queueManager;
		private readonly IStateManager _stateManager;
		private readonly IIntegrationPointPermissionValidator _permissionValidator;

		public ButtonStateBuilder(IProviderTypeService providerTypeService, 
			IQueueManager queueManager, 
			IJobHistoryManager jobHistoryManager,
			IStateManager stateManager,
			IPermissionRepository permissionRepository, 
			IIntegrationPointPermissionValidator permissionValidator,
			IIntegrationPointRepository integrationPointRepository)
		{
			_providerTypeService = providerTypeService;
			_queueManager = queueManager;
			_jobHistoryManager = jobHistoryManager;
			_stateManager = stateManager;
			_permissionRepository = permissionRepository;
			_permissionValidator = permissionValidator;
			_integrationPointRepository = integrationPointRepository;
		}

		public ButtonStateDTO CreateButtonState(int workspaceArtifactId, int integrationPointArtifactId)
		{
			IntegrationPoint integrationPoint =
				_integrationPointRepository.ReadWithFieldMappingAsync(integrationPointArtifactId).GetAwaiter().GetResult();
			ProviderType providerType = _providerTypeService.GetProviderType(integrationPoint.SourceProvider.Value,
				integrationPoint.DestinationProvider.Value);

			ValidationResult jobHistoryErrorViewPermissionCheck = _permissionValidator.ValidateViewErrors(workspaceArtifactId);

			ImportSettings settings = JsonConvert.DeserializeObject<ImportSettings>(integrationPoint.DestinationConfiguration);

            bool hasAddProfilePermission = _permissionRepository.UserHasArtifactTypePermission(Guid.Parse(ObjectTypeGuids.IntegrationPointProfile),
				ArtifactPermission.Create) && !settings.IsFederatedInstance();

			bool canViewErrors = jobHistoryErrorViewPermissionCheck.IsValid;
			bool hasJobsExecutingOrInQueue = HasJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);

            SourceConfiguration.ExportType exportType;
            try
            {
	            exportType = (SourceConfiguration.ExportType)JsonConvert
		            .DeserializeAnonymousType(integrationPoint.SourceConfiguration, new { TypeOfExport = 0 })
		            .TypeOfExport;
            }
            catch (Exception)
            {
	            exportType = 0;
            }

            bool integrationPointIsStoppable = IntegrationPointIsStoppable(providerType: providerType, workspaceArtifactId: workspaceArtifactId,
                integrationPointArtifactId: integrationPointArtifactId, exportType: exportType);
			bool integrationPointHasErrors = integrationPoint.HasErrors.GetValueOrDefault(false);
			ButtonStateDTO buttonState = _stateManager.GetButtonState(exportType, providerType, hasJobsExecutingOrInQueue, integrationPointHasErrors, canViewErrors,
				integrationPointIsStoppable, hasAddProfilePermission);
			return buttonState;
		}

		private bool HasJobsExecutingOrInQueue(int workspaceArtifactId, int integrationPointArtifactId)
		{
			return _queueManager.HasJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);
		}

		private bool IntegrationPointIsStoppable(ProviderType providerType, int workspaceArtifactId, int integrationPointArtifactId, SourceConfiguration.ExportType exportType)
		{
			StoppableJobHistoryCollection stoppableJobCollection = _jobHistoryManager.GetStoppableJobHistory(workspaceArtifactId, integrationPointArtifactId);

			bool hasExecutingJobs = _queueManager.HasJobsExecuting(workspaceArtifactId, integrationPointArtifactId);

			if(stoppableJobCollection.HasOnlyPendingJobHistory && !hasExecutingJobs)
            {
				return true;
            }

			if (IsNonStoppableBasedOnProviderType(providerType, exportType))
			{
				return false;
			}

			return stoppableJobCollection.HasStoppableJobHistory;
		}

		private static bool IsNonStoppableBasedOnProviderType(ProviderType providerType, SourceConfiguration.ExportType exportType)
		{
			return (providerType != ProviderType.Relativity && providerType != ProviderType.LoadFile) ||
				(providerType == ProviderType.Relativity && exportType == SourceConfiguration.ExportType.ProductionSet);
		}
	}
}