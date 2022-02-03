using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Helpers
{
	[TestFixture, Category("Unit")]
	public class ButtonStateBuilderTests : TestBase
	{
		private ButtonStateBuilder _buttonStateBuilder;
		private IIntegrationPointPermissionValidator _permissionValidator;
		private IIntegrationPointRepository _integrationPointRepository;
		private IJobHistoryManager _jobHistoryManager;
		private IPermissionRepository _permissionRepository;
		private IProviderTypeService _providerTypeService;
		private IQueueManager _queueManager;
		private IStateManager _stateManager;

		private const int _WORKSPACE_ID = 100;
		private const int _INTEGRATION_POINT_ID = 200;

		[SetUp]
		public override void SetUp()
		{
			_providerTypeService = Substitute.For<IProviderTypeService>();
			_jobHistoryManager = Substitute.For<IJobHistoryManager>();
			_queueManager = Substitute.For<IQueueManager>();
			_stateManager = Substitute.For<IStateManager>();
			_permissionRepository = Substitute.For<IPermissionRepository>();
			_permissionValidator = Substitute.For<IIntegrationPointPermissionValidator>();
			_integrationPointRepository = Substitute.For<IIntegrationPointRepository>();

			_buttonStateBuilder = new ButtonStateBuilder(_providerTypeService, _queueManager, _jobHistoryManager, _stateManager,
				_permissionRepository, _permissionValidator, _integrationPointRepository);
		}

		[TestCase(ProviderType.Relativity, true, true, true, true,false)]
		[TestCase(ProviderType.Other, true, true, true, true, false)]
		[TestCase(ProviderType.FTP, true, true, true, true, false)]
		[TestCase(ProviderType.LDAP, true, true, true, true, false)]
		[TestCase(ProviderType.LoadFile, true, true, true, true, false)]
		[TestCase(ProviderType.Relativity, false, true, true, true, false)]
		[TestCase(ProviderType.Relativity, true, false, true, true, false)]
		[TestCase(ProviderType.Relativity, true, true, true, true, false)]
		[TestCase(ProviderType.Relativity, true, true, false, true, false)]
		[TestCase(ProviderType.Relativity, true, true, true, false, false)]
		[TestCase(ProviderType.Relativity, true, true, true, false, true)]
		public void BuildButtonState_GoldWorkflow(
			ProviderType providerType, bool hasErrorViewPermission, bool hasJobsExecutingOrInQueue,
			bool hasErrors, bool hasAddProfilePermission,bool imageImport)
		{
			// Arrange
			SetupIntegrationPoint(providerType, imageImport, hasErrors);

			_permissionValidator.ValidateViewErrors(_WORKSPACE_ID).Returns(
				hasErrorViewPermission ? new ValidationResult() : new ValidationResult(new[] {"error"}));

			_queueManager.HasJobsExecutingOrInQueue(_WORKSPACE_ID, _INTEGRATION_POINT_ID).Returns(hasJobsExecutingOrInQueue);

			_jobHistoryManager.GetStoppableJobHistory(_WORKSPACE_ID, _INTEGRATION_POINT_ID)
				.Returns(GetJobHistoryCollection(true, false));

			_queueManager.HasJobsExecuting(_WORKSPACE_ID, _INTEGRATION_POINT_ID).Returns(false);

			_permissionRepository.UserHasArtifactTypePermission(Arg.Any<Guid>(), ArtifactPermission.Create).Returns(hasAddProfilePermission);

			// Act
			_buttonStateBuilder.CreateButtonState(_WORKSPACE_ID, _INTEGRATION_POINT_ID);

			// Assert
			_stateManager.Received()
				.GetButtonState(
					Arg.Is(providerType),
					Arg.Is(hasJobsExecutingOrInQueue),
					Arg.Is(hasErrors),
					Arg.Is(hasErrorViewPermission),
					Arg.Any<bool>(),
					Arg.Is(hasAddProfilePermission));
		}

		[TestCase(ProviderType.Other, true, false, false, false, true)]
		[TestCase(ProviderType.LDAP, true, false, false, false, true)]
		[TestCase(ProviderType.FTP, true, false, false, false, true)]
		[TestCase(ProviderType.Other, false, true, false, false, false)]
		[TestCase(ProviderType.Other, true, false, true, false, false)]
		[TestCase(ProviderType.Relativity, false, true, true, false, true)]
		[TestCase(ProviderType.Relativity, false, true, true, true, false)]
		[TestCase(ProviderType.LoadFile, false, true, true, true, true)]
		public void CreateButtonState_IntegrationPointIsStoppableBasedOnCriteria(
			ProviderType providerType,bool hasPendingJobHistory, bool hasProcessingJobHistory,
			bool hasJobsExecuting, bool isImageImport, bool expectedIsStoppable)
        {
			// Arrange
			SetupIntegrationPoint(providerType, isImageImport: isImageImport);

			_permissionValidator.ValidateViewErrors(_WORKSPACE_ID).Returns(new ValidationResult());

			_jobHistoryManager.GetStoppableJobHistory(_WORKSPACE_ID, _INTEGRATION_POINT_ID)
				.Returns(GetJobHistoryCollection(hasPendingJobHistory, hasProcessingJobHistory));

			_queueManager.HasJobsExecuting(_WORKSPACE_ID, _INTEGRATION_POINT_ID).Returns(hasJobsExecuting);

			// Act
			_buttonStateBuilder.CreateButtonState(_WORKSPACE_ID, _INTEGRATION_POINT_ID);

			// Assert
			_stateManager.Received()
				.GetButtonState(
					Arg.Is(providerType),
					Arg.Any<bool>(),
					Arg.Any<bool>(),
					Arg.Any<bool>(),
					Arg.Is(expectedIsStoppable),
					Arg.Any<bool>());
		}

		private void SetupIntegrationPoint(ProviderType providerType, bool isImageImport = false, bool hasErrors = false)
        {
			const int sourceProviderArtifactId = 210;
			const int destinationProviderArtifactId = 220;

			var settings = new ImportSettings { ImageImport = isImageImport };

			_integrationPointRepository.ReadWithFieldMappingAsync(_INTEGRATION_POINT_ID)
				.Returns(new Data.IntegrationPoint
				{
					HasErrors = hasErrors,
					SourceProvider = sourceProviderArtifactId,
					DestinationProvider = destinationProviderArtifactId,
					DestinationConfiguration = JsonConvert.SerializeObject(settings)
				});

			_providerTypeService.GetProviderType(sourceProviderArtifactId, destinationProviderArtifactId).Returns(providerType);
		}

		private StoppableJobHistoryCollection GetJobHistoryCollection(bool hasPendingJobHistory, bool hasProcessingJobHistory)
        {
			return new StoppableJobHistoryCollection
			{
				PendingJobHistory = hasPendingJobHistory ? new[] { new JobHistory() } : Array.Empty<JobHistory>(),
				ProcessingJobHistory = hasProcessingJobHistory ? new[] { new JobHistory() } : Array.Empty<JobHistory>(),
			};
        }
	}
}