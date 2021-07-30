﻿using kCura.IntegrationPoints.Data;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync.Tests
{
	[TestFixture, Category("Unit")]
	public class JobHistorySyncServiceTests
	{
		private JobHistorySyncService _sut;

		private Mock<IExtendedJob> _jobFake;
		private Mock<IObjectManager> _objectManagerMock;

		private const int _JOB_ID = 1;
		private const int _JOB_HISTORY_ID = 10;
		private const int _WORKSPACE_ID = 100;
		private const int _INTEGRATION_POINT_ID = 110;

		[SetUp]
		public void SetUp()
		{
			_jobFake = new Mock<IExtendedJob>();
			_jobFake.SetupGet(x => x.JobHistoryId).Returns(_JOB_HISTORY_ID);
			_jobFake.SetupGet(x => x.WorkspaceId).Returns(_WORKSPACE_ID);
			_jobFake.SetupGet(x => x.IntegrationPointId).Returns(_INTEGRATION_POINT_ID);
			_jobFake.SetupGet(x => x.JobId).Returns(_JOB_ID);

			_objectManagerMock = new Mock<IObjectManager>();

			Mock<IServicesMgr> servicesMgr = new Mock<IServicesMgr>();
			servicesMgr.Setup(x => x.CreateProxy<IObjectManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_objectManagerMock.Object);

			Mock<IHelper> helper = new Mock<IHelper>();
			helper.Setup(x => x.GetServicesManager()).Returns(servicesMgr.Object);

			_sut = new JobHistorySyncService(helper.Object);
		}

		[TestCase("validating")]
		[TestCase("checking permissions")]
		public async Task UpdateJobStatusAsync_ShouldUpdateStatus_WhenStepsAreValidationRelated(string status)
		{
			// Act
			await _sut.UpdateJobStatusAsync(status, _jobFake.Object).ConfigureAwait(false);

			// Assert
			VerifyJobHistoryStatus(JobStatusChoices.JobHistoryValidatingGuid);
		}

		[TestCase("processing")]
		[TestCase("synchronizing")]
		[TestCase("creating tags")]
		public async Task UpdateJobStatusAsync_ShouldUpdateStatus_WhenStepsAreProcessingRelated(string status)
		{
			// Act
			await _sut.UpdateJobStatusAsync(status, _jobFake.Object).ConfigureAwait(false);

			// Assert
			VerifyJobHistoryStatus(JobStatusChoices.JobHistoryProcessingGuid);
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task MarkJobAsStoppedAsync_ShouldUpdateStatusToStopped(bool hasErrors)
		{
			// Arrange
			SetupHasErrors(hasErrors);

			// Act
			await _sut.MarkJobAsStoppedAsync(_jobFake.Object).ConfigureAwait(false);

			// Assert
			VerifyJobHistoryStatus(JobStatusChoices.JobHistoryStoppedGuid);

			VerifyIntegrationPointWasUpdated(hasErrors);
		}

		[Test]
		public async Task MarkJobAsValidationFailedAsync_ShouldUpdateStatusToValidationFailedAndAddErrors()
		{
			// Arrange
			ValidationException exception = new ValidationException(new ValidationResult() { IsValid = false });

			// Act
			await _sut.MarkJobAsValidationFailedAsync(exception, _jobFake.Object).ConfigureAwait(false);

			// Assert
			VerifyJobHistoryStatus(JobStatusChoices.JobHistoryValidationFailedGuid);

			VerifyErrorsWereAdded();
		}

		[Test]
		public async Task MarkJobAsStartedAsync_ShouldUpdateStatusToStarted()
		{
			// Act
			await _sut.MarkJobAsStartedAsync(_jobFake.Object).ConfigureAwait(false);

			// Assert
			VerifyJobHistoryWasUpdatedWithJobId();
		}

		[Test]
		public async Task MarkJobAsCompletedAsync_ShouldUpdateStatusToCompleted_WhenNoErrorsFound()
		{
			// Arrange
			const bool hasErrors = false;

			SetupHasErrors(hasErrors);

			// Act
			await _sut.MarkJobAsCompletedAsync(_jobFake.Object).ConfigureAwait(false);

			// Assert
			VerifyJobHistoryStatus(JobStatusChoices.JobHistoryCompletedGuid);

			VerifyIntegrationPointWasUpdated(hasErrors);
		}

		[Test]
		public async Task MarkJobAsCompletedAsync_ShouldUpdateStatusToCompletedWithErrors_WhenErrorsFound()
		{
			// Arrange
			const bool hasErrors = true;

			SetupHasErrors(hasErrors);

			// Act
			await _sut.MarkJobAsCompletedAsync(_jobFake.Object).ConfigureAwait(false);

			// Assert
			VerifyJobHistoryStatus(JobStatusChoices.JobHistoryCompletedWithErrorsGuid);

			VerifyIntegrationPointWasUpdated(hasErrors);
		}

		[Test]
		public async Task MarkJobAsFailedAsync_ShouldUpdateStatusToFailed()
		{
			// Arrange
			InvalidOperationException ex = new InvalidOperationException();

			// Act
			await _sut.MarkJobAsFailedAsync(_jobFake.Object, ex).ConfigureAwait(false);

			// Assert
			VerifyJobHistoryStatus(JobStatusChoices.JobHistoryErrorJobFailedGuid);

			VerifyErrorsWereAdded();
		}

		private void VerifyJobHistoryStatus(Guid expectedStatusGuid)
		{
			_objectManagerMock.Verify(x => x.UpdateAsync(_WORKSPACE_ID,
				It.Is<UpdateRequest>(r => r.Object.ArtifactID == _JOB_HISTORY_ID &&
										r.FieldValues.Any(f => 
											f.Field.Guid == JobHistoryFieldGuids.JobStatusGuid && 
											((ChoiceRef)f.Value).Guid == expectedStatusGuid))));
		}

		private void VerifyIntegrationPointWasUpdated(bool hasErrors)
		{
			_objectManagerMock.Verify(x => x.UpdateAsync(_WORKSPACE_ID,
				It.Is<UpdateRequest>(r => r.Object.ArtifactID == _INTEGRATION_POINT_ID &&
										r.FieldValues.Any(f =>
											f.Field.Guid == IntegrationPointFieldGuids.HasErrorsGuid &&
											(bool)f.Value == hasErrors))));

			_objectManagerMock.Verify(x => x.UpdateAsync(_WORKSPACE_ID,
				It.Is<UpdateRequest>(r => r.Object.ArtifactID == _INTEGRATION_POINT_ID &&
											r.FieldValues.Any(f =>
												f.Field.Guid == IntegrationPointFieldGuids.LastRuntimeUTCGuid &&
												f.Value != null))));
		}

		private void VerifyErrorsWereAdded()
		{
			_objectManagerMock.Setup(x => x.CreateAsync(_WORKSPACE_ID, 
				It.Is<CreateRequest>(r => r.ObjectType.Guid == ObjectTypeGuids.JobHistoryErrorGuid &&
										r.ParentObject.ArtifactID == _JOB_HISTORY_ID && 
										r.FieldValues.Select(fv => fv.Field.Guid).SequenceEqual(ExpectedJobHistoryErrorGuids()))));
		}

		private void VerifyJobHistoryWasUpdatedWithJobId()
		{
			_objectManagerMock.Verify(x => x.UpdateAsync(_WORKSPACE_ID,
				It.Is<UpdateRequest>(r => r.Object.ArtifactID == _JOB_HISTORY_ID &&
										r.FieldValues.Any(f =>
											f.Field.Guid == JobHistoryFieldGuids.JobIDGuid &&
											f.Value.ToString() == _JOB_ID.ToString()))));
		}

		private void SetupHasErrors(bool hasErrors)
		{
			_objectManagerMock.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(
				q => q.ObjectType.Guid == ObjectTypeGuids.JobHistoryErrorGuid), It.IsAny<int>(), It.IsAny<int>()))
					.ReturnsAsync(new QueryResult
					{
						ResultCount = hasErrors ? 1 : 0
					});
		}

		private IEnumerable<Guid?> ExpectedJobHistoryErrorGuids()
		{
			return new Guid?[]
			{
				JobHistoryErrorFieldGuids.ErrorGuid,
				JobHistoryErrorFieldGuids.ErrorStatusGuid,
				JobHistoryErrorFieldGuids.ErrorTypeGuid,
				JobHistoryErrorFieldGuids.StackTraceGuid
			};
		}
	}
}