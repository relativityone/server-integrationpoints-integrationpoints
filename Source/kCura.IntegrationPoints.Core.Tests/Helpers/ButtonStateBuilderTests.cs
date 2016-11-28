using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Core.Tests.Helpers
{
	public class ButtonStateBuilderTests : TestBase
	{
		[SetUp]
		public override void SetUp()
		{
			_integrationPointManager = Substitute.For<IIntegrationPointManager>();
			_jobHistoryManager = Substitute.For<IJobHistoryManager>();
			_queueManager = Substitute.For<IQueueManager>();
			_stateManager = Substitute.For<IStateManager>();
			_permissionRepository = Substitute.For<IPermissionRepository>();

			_buttonStateBuilder = new ButtonStateBuilder(_integrationPointManager, _queueManager, _jobHistoryManager, _stateManager, _permissionRepository);
		}

		private IIntegrationPointManager _integrationPointManager;
		private IJobHistoryManager _jobHistoryManager;
		private IQueueManager _queueManager;
		private IStateManager _stateManager;
		private IPermissionRepository _permissionRepository;

		private ButtonStateBuilder _buttonStateBuilder;

		[TestCase(Constants.SourceProvider.Relativity, true, true, true, true, true)]
		[TestCase(Constants.SourceProvider.Other, true, true, true, true, true)]
		[TestCase(Constants.SourceProvider.Relativity, false, true, true, true, true)]
		[TestCase(Constants.SourceProvider.Relativity, true, false, true, true, true)]
		[TestCase(Constants.SourceProvider.Relativity, true, true, false, true, true)]
		[TestCase(Constants.SourceProvider.Relativity, true, true, true, false, true)]
		[TestCase(Constants.SourceProvider.Relativity, true, true, true, true, false)]
		[Test]
		public void BuildButtonState_GoldWorkflow(Constants.SourceProvider sourceProvider, bool hasErrorViewPermission, bool hasJobsExecutingOrInQueue, bool hasStoppableJobs,
			bool hasErrors, bool hasAddProfilePermission)
		{
			int applicationArtifactId = 501;
			int integrationPointArtifactId = 229;

			_integrationPointManager.Read(applicationArtifactId, integrationPointArtifactId).Returns(new IntegrationPointDTO
			{
				HasErrors = hasErrors
			});
			_integrationPointManager.UserHasPermissionToViewErrors(applicationArtifactId).Returns(new PermissionCheckDTO
			{
				ErrorMessages = hasErrorViewPermission ? null : new[] {"error"}
			});
			_integrationPointManager.GetSourceProvider(applicationArtifactId, Arg.Any<IntegrationPointDTO>()).Returns(sourceProvider);

			_queueManager.HasJobsExecutingOrInQueue(applicationArtifactId, integrationPointArtifactId).Returns(hasJobsExecutingOrInQueue);

			_jobHistoryManager.GetStoppableJobCollection(applicationArtifactId, integrationPointArtifactId).Returns(new StoppableJobCollection
			{
				PendingJobArtifactIds = hasStoppableJobs ? new[] {1, 2} : null
			});

			_permissionRepository.UserHasArtifactTypePermission(Arg.Any<Guid>(), ArtifactPermission.Create).Returns(hasAddProfilePermission);

			_buttonStateBuilder.CreateButtonState(applicationArtifactId, integrationPointArtifactId);

			_stateManager.Received(1).GetButtonState(sourceProvider, hasJobsExecutingOrInQueue, hasErrors, hasErrorViewPermission, hasStoppableJobs, hasAddProfilePermission);
		}
	}
}