using System;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Helpers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using Moq;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.BatchStatusCommands
{
    [TestFixture, Category("Unit")]
    public class JobHistoryErrorBatchUpdateManagerTest : TestBase
    {
        private JobHistoryErrorBatchUpdateManager _sut;
        private IRepositoryFactory _repositoryFactoryMock;
        private IJobHistoryErrorRepository _jobHistoryErrorRepositoryMock;
        private IHelper _helperMock;
        private Mock<IMassUpdateHelper> _massUpdateHelperMock;
        private JobHistoryErrorDTO.UpdateStatusType _updateStatusTypeMock;
        private const int _sourceWorkspaceId = 1357475;
        private readonly Job _job = null;
        private IJobHistoryErrorManager _jobHistoryErrorManagerMock;
        private IJobStopManager _jobStopManagerMock;
        private const string _SCRATCHTABLE_ITEMCOMPLETE = "IntegrationPoint_Relativity_JobHistoryErrors_ItemComplete";
        private const string _SCRATCHTABLE_JOBCOMPLETE = "IntegrationPoint_Relativity_JobHistoryErrors_JobComplete";

        [SetUp]
        public override void SetUp()
        {
            _massUpdateHelperMock = new Mock<IMassUpdateHelper>();

            _jobStopManagerMock = Substitute.For<IJobStopManager>();
            _jobHistoryErrorRepositoryMock = Substitute.For<IJobHistoryErrorRepository>();
            _repositoryFactoryMock = Substitute.For<IRepositoryFactory>();
            _updateStatusTypeMock = Substitute.For<JobHistoryErrorDTO.UpdateStatusType>();
            _jobHistoryErrorManagerMock = Substitute.For<IJobHistoryErrorManager>();
            _helperMock = Substitute.For<IHelper>();

            _jobHistoryErrorManagerMock.JobHistoryErrorItemStart.Returns(Substitute.For<IScratchTableRepository>());
            _jobHistoryErrorManagerMock.JobHistoryErrorItemComplete.Returns(Substitute.For<IScratchTableRepository>());
            _jobHistoryErrorManagerMock.JobHistoryErrorJobStart.Returns(Substitute.For<IScratchTableRepository>());
            _jobHistoryErrorManagerMock.JobHistoryErrorJobComplete.Returns(Substitute.For<IScratchTableRepository>());

            _jobHistoryErrorManagerMock.JobHistoryErrorItemComplete.GetTempTableName().Returns(_SCRATCHTABLE_ITEMCOMPLETE);
            _jobHistoryErrorManagerMock.JobHistoryErrorJobComplete.GetTempTableName().Returns(_SCRATCHTABLE_JOBCOMPLETE);

            _sut = new JobHistoryErrorBatchUpdateManager(
                _jobHistoryErrorManagerMock,
                _helperMock.GetLoggerFactory().GetLogger(),
                _repositoryFactoryMock,
                _jobStopManagerMock,
                _sourceWorkspaceId,
                _updateStatusTypeMock,
                _massUpdateHelperMock.Object);

            _repositoryFactoryMock.GetJobHistoryErrorRepository(_sourceWorkspaceId).Returns(_jobHistoryErrorRepositoryMock);
        }

        [Test]
        public void OnJobStart_RunNow_NoErrors()
        {
            // Arrange
            _updateStatusTypeMock.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run;
            _updateStatusTypeMock.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.None;

            // Act
            _sut.OnJobStart(_job);

            // Assert
            VerifyErrorsStatusWasNotUpdated();
        }

        [Test]
        public void OnJobStart_RunNow_JobError()
        {
            // Arrange
            _updateStatusTypeMock.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run;
            _updateStatusTypeMock.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly;

            // Act
            _sut.OnJobStart(_job);

            // Assert
            VerifyErrorStatusWasUpdatedAndScratchTableDisposed(
                _jobHistoryErrorManagerMock.JobHistoryErrorJobStart,
                ErrorStatusChoices.JobHistoryErrorExpiredGuid);
        }

        [Test]
        public void OnJobStart_RunNow_JobAndItemErrors()
        {
            // Arrange
            _updateStatusTypeMock.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run;
            _updateStatusTypeMock.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem;

            // Act
            _sut.OnJobStart(_job);

            // Assert
            VerifyErrorStatusWasUpdatedAndScratchTableDisposed(
                _jobHistoryErrorManagerMock.JobHistoryErrorJobStart,
                ErrorStatusChoices.JobHistoryErrorExpiredGuid);
            VerifyErrorStatusWasUpdatedAndScratchTableDisposed(
                _jobHistoryErrorManagerMock.JobHistoryErrorItemStart,
                ErrorStatusChoices.JobHistoryErrorExpiredGuid);
        }

        [Test]
        public void OnJobStart_RunNow_ItemErrors()
        {
            // Arrange
            _updateStatusTypeMock.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run;
            _updateStatusTypeMock.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly;

            // Act
            _sut.OnJobStart(_job);

            // Assert
            VerifyErrorStatusWasUpdatedAndScratchTableDisposed(
                _jobHistoryErrorManagerMock.JobHistoryErrorItemStart,
                ErrorStatusChoices.JobHistoryErrorExpiredGuid);
        }

        [Test]
        public void OnJobStart_RetryErrors_NoErrors()
        {
            // Arrange
            _updateStatusTypeMock.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
            _updateStatusTypeMock.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.None;

            // Act
            _sut.OnJobStart(_job);

            // Assert
            VerifyErrorsStatusWasNotUpdated();

        }

        [Test]
        public void OnJobStart_RetryErrors_JobError()
        {
            // Arrange
            _updateStatusTypeMock.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
            _updateStatusTypeMock.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly;

            // Act
            _sut.OnJobStart(_job);

            // Assert
            _jobHistoryErrorManagerMock.JobHistoryErrorJobStart.Received(1).CopyTempTable(_SCRATCHTABLE_JOBCOMPLETE);
            VerifyErrorStatusWasUpdatedAndScratchTableDisposed(
                _jobHistoryErrorManagerMock.JobHistoryErrorJobStart,
                ErrorStatusChoices.JobHistoryErrorInProgressGuid);
        }

        [Test]
        public void OnJobStart_RetryErrors_JobAndItemErrors()
        {
            // Arrange
            _updateStatusTypeMock.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
            _updateStatusTypeMock.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem;

            // Act
            _sut.OnJobStart(_job);

            // Assert
            _jobHistoryErrorManagerMock.JobHistoryErrorJobStart.Received(1).CopyTempTable(_SCRATCHTABLE_JOBCOMPLETE);

            VerifyErrorStatusWasUpdatedAndScratchTableDisposed(
                _jobHistoryErrorManagerMock.JobHistoryErrorJobStart,
                ErrorStatusChoices.JobHistoryErrorInProgressGuid);
            VerifyErrorStatusWasUpdatedAndScratchTableDisposed(
                _jobHistoryErrorManagerMock.JobHistoryErrorItemStart,
                ErrorStatusChoices.JobHistoryErrorExpiredGuid);
        }

        [Test]
        public void OnJobStart_RetryErrors_ItemErrors()
        {
            // Arrange
            _updateStatusTypeMock.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
            _updateStatusTypeMock.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly;

            // Act
            _sut.OnJobStart(_job);

            // Assert
            _jobHistoryErrorManagerMock.JobHistoryErrorItemStart.Received(1).CopyTempTable(_SCRATCHTABLE_ITEMCOMPLETE);
            VerifyErrorStatusWasUpdatedAndScratchTableDisposed(
                _jobHistoryErrorManagerMock.JobHistoryErrorItemStart,
                ErrorStatusChoices.JobHistoryErrorInProgressGuid);
        }

        [Test]
        public void OnJobComplete_RunNow_NoErrors()
        {
            // Arrange
            _updateStatusTypeMock.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run;
            _updateStatusTypeMock.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.None;

            // Act
            _sut.OnJobComplete(_job);

            // Assert
            VerifyErrorsStatusWasNotUpdated();
        }

        [Test]
        public void OnJobComplete_RunNow_JobError()
        {
            // Arrange
            _updateStatusTypeMock.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run;
            _updateStatusTypeMock.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly;

            // Act
            _sut.OnJobComplete(_job);

            // Assert
            VerifyErrorsStatusWasNotUpdated();
        }

        [Test]
        public void OnJobComplete_RunNow_JobAndItemErrors()
        {
            // Arrange
            _updateStatusTypeMock.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run;
            _updateStatusTypeMock.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem;

            // Act
            _sut.OnJobComplete(_job);

            // Assert
            VerifyErrorsStatusWasNotUpdated();
        }

        [Test]
        public void OnJobComplete_RunNow_ItemErrors()
        {
            // Arrange
            _updateStatusTypeMock.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run;
            _updateStatusTypeMock.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly;

            // Act
            _sut.OnJobComplete(_job);

            // Assert
            VerifyErrorsStatusWasNotUpdated();
        }

        [Test]
        public void OnJobComplete_RetryErrors_NoErrors()
        {
            // Arrange
            _updateStatusTypeMock.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
            _updateStatusTypeMock.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.None;

            // Act
            _sut.OnJobComplete(_job);

            // Assert
            VerifyErrorsStatusWasNotUpdated();
        }

        [Test]
        public void OnJobComplete_RetryErrors_JobError()
        {
            // Arrange
            _updateStatusTypeMock.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
            _updateStatusTypeMock.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly;

            // Act
            _sut.OnJobComplete(_job);

            // Assert
            VerifyErrorStatusWasUpdatedAndScratchTableDisposed(
                _jobHistoryErrorManagerMock.JobHistoryErrorJobComplete,
                ErrorStatusChoices.JobHistoryErrorRetriedGuid);
        }

        [Test]
        public void OnJobComplete_RetryErrors_JobAndItemErrors()
        {
            // Arrange
            _updateStatusTypeMock.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
            _updateStatusTypeMock.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem;

            // Act
            _sut.OnJobComplete(_job);

            // Assert
            VerifyErrorStatusWasUpdatedAndScratchTableDisposed(
                _jobHistoryErrorManagerMock.JobHistoryErrorJobComplete,
                ErrorStatusChoices.JobHistoryErrorRetriedGuid);
        }

        [Test]
        public void OnJobComplete_RetryErrors_ItemErrors()
        {
            // Arrange
            _updateStatusTypeMock.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
            _updateStatusTypeMock.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly;

            // Act
            _sut.OnJobComplete(_job);

            // Assert
            VerifyErrorStatusWasUpdatedAndScratchTableDisposed(
                _jobHistoryErrorManagerMock.JobHistoryErrorItemComplete,
                ErrorStatusChoices.JobHistoryErrorRetriedGuid);
        }

        [Test]
        public void OnJobComplete_RetryErrors_StopRequested()
        {
            // Arrange
            _jobStopManagerMock.IsStopRequested().Returns(true);

            // Act
            _sut.OnJobComplete(_job);

            // Assert
            VerifyErrorStatusWasUpdatedAndScratchTableDisposed(
                _jobHistoryErrorManagerMock.JobHistoryErrorItemComplete,
                ErrorStatusChoices.JobHistoryErrorExpiredGuid);
        }

        private void VerifyErrorsStatusWasNotUpdated()
        {
            _massUpdateHelperMock.Verify(
                x => x.UpdateArtifactsAsync(
                    It.IsAny<IScratchTableRepository>(),
                    It.IsAny<FieldUpdateRequestDto[]>(),
                    It.IsAny<IRepositoryWithMassUpdate>()),
                Times.Never);
        }

        private void VerifyErrorStatusWasUpdatedAndScratchTableDisposed(
            IScratchTableRepository scratchTableRepository,
            Guid expectedErrorStatusGuid)
        {
            _massUpdateHelperMock.Verify(
                x => x.UpdateArtifactsAsync(
                    scratchTableRepository,
                    It.Is<FieldUpdateRequestDto[]>(fields => ValidateErrorStatusFieldValue(fields, expectedErrorStatusGuid)),
                    _jobHistoryErrorRepositoryMock
                ));
            scratchTableRepository.Received(1).Dispose();
        }

        private bool ValidateErrorStatusFieldValue(FieldUpdateRequestDto[] fields, Guid expectedChoiceValue)
        {
            FieldUpdateRequestDto errorStatusField = fields.SingleOrDefault(x => x.FieldIdentifier == JobHistoryErrorFieldGuids.ErrorStatusGuid);
            return errorStatusField.NewValue is SingleChoiceReferenceDto signleChoiceReference
                   && signleChoiceReference.ChoiceValueGuid == expectedChoiceValue;
        }
    }
}
