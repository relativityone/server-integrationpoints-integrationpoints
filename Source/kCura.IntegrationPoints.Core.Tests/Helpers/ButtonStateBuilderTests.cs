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
	public class ButtonStateBuilderTests : TestBase
	{
		[SetUp]
		public override void SetUp()
		{
			_providerTypeService = Substitute.For<IProviderTypeService>();
			_jobHistoryManager = Substitute.For<IJobHistoryManager>();
			_queueManager = Substitute.For<IQueueManager>();
			_stateManager = Substitute.For<IStateManager>();
			_permissionRepository = Substitute.For<IPermissionRepository>();
			_permissionValidator = Substitute.For<IIntegrationPointPermissionValidator>();
			_rsapiService = Substitute.For<IRSAPIService>();

			_buttonStateBuilder = new ButtonStateBuilder(_providerTypeService, _queueManager, _jobHistoryManager, _stateManager, _permissionRepository, _permissionValidator,
				_rsapiService);
		}

		private IRSAPIService _rsapiService;
		private IProviderTypeService _providerTypeService;
		private IJobHistoryManager _jobHistoryManager;
		private IQueueManager _queueManager;
		private IStateManager _stateManager;
		private IPermissionRepository _permissionRepository;
		private IIntegrationPointPermissionValidator _permissionValidator;

		private ButtonStateBuilder _buttonStateBuilder;

		[TestCase(ProviderType.Relativity, true, true, true, true, true,false)]
		[TestCase(ProviderType.Other, true, true, true, true, true, false)]
		[TestCase(ProviderType.FTP, true, true, true, true, true, false)]
		[TestCase(ProviderType.LDAP, true, true, true, true, true, false)]
		[TestCase(ProviderType.LoadFile, true, true, true, true, true, false)]
		[TestCase(ProviderType.Relativity, false, true, true, true, true, false)]
		[TestCase(ProviderType.Relativity, true, false, true, true, true, false)]
		[TestCase(ProviderType.Relativity, true, true, false, true, true, false)]
		[TestCase(ProviderType.Relativity, true, true, true, false, true, false)]
		[TestCase(ProviderType.Relativity, true, true, true, true, false, false)]
		[TestCase(ProviderType.Relativity, true, true, true, true, false, true)]
		[Test]
		public void BuildButtonState_GoldWorkflow(ProviderType providerType, bool hasErrorViewPermission, bool hasJobsExecutingOrInQueue, bool hasStoppableJobs,
			bool hasErrors, bool hasAddProfilePermission,bool imageImport)
		{
			int applicationArtifactId = 501;
			int integrationPointArtifactId = 229;
			int sourceProviderArtifactId = 841;
			int destinationProviderArtifactId = 273;
			var importSettings = new ImportSettings {ImageImport = imageImport};
			_rsapiService.IntegrationPointLibrary.Read(integrationPointArtifactId).Returns(new Data.IntegrationPoint
			{
				HasErrors = hasErrors,
				SourceProvider = sourceProviderArtifactId,
				DestinationProvider = destinationProviderArtifactId,
				DestinationConfiguration = JsonConvert.SerializeObject(importSettings)
			});

			_permissionValidator.ValidateViewErrors(applicationArtifactId).Returns(
				hasErrorViewPermission ? new ValidationResult() : new ValidationResult(new[] {"error"}));

			_providerTypeService.GetProviderType(sourceProviderArtifactId, destinationProviderArtifactId).Returns(providerType);

			_queueManager.HasJobsExecutingOrInQueue(applicationArtifactId, integrationPointArtifactId).Returns(hasJobsExecutingOrInQueue);

			_jobHistoryManager.GetStoppableJobCollection(applicationArtifactId, integrationPointArtifactId).Returns(new StoppableJobCollection
			{
				PendingJobArtifactIds = hasStoppableJobs ? new[] {1, 2} : null
			});

			_permissionRepository.UserHasArtifactTypePermission(Arg.Any<Guid>(), ArtifactPermission.Create).Returns(hasAddProfilePermission);

			_buttonStateBuilder.CreateButtonState(applicationArtifactId, integrationPointArtifactId);

			bool hasTrulyStoppableJobs = hasStoppableJobs && (providerType == ProviderType.Relativity || providerType == ProviderType.LoadFile) && !imageImport;

			_stateManager.Received(1).GetButtonState(providerType, hasJobsExecutingOrInQueue, hasErrors, hasErrorViewPermission, hasTrulyStoppableJobs, hasAddProfilePermission);
		}
	}
}