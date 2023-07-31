using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using kCura.IntegrationPoints.Common.RelativitySync;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.RelativitySync;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Tests.Helpers;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
    [TestFixture]
    [Category("Unit")]
    public class IntegrationPointServiceTests
    {
        private IFixture _fxt;
        private IntegrationPointService _sut;
        private Mock<ICaseServiceContext> _contextFake;
        private Mock<IRelativityObjectManager> _objectManagerFake;
        private Mock<IIntegrationPointRepository> _integrationPointRepositoryMock;
        private Mock<IJobHistoryService> _jobHistoryServiceMock;
        private Mock<IQueueManager> _queueManagerFake;
        private Mock<IValidationExecutor> _validationExecutorMock;
        private Mock<IJobManager> _jobManagerMock;
        private Mock<IJobHistoryManager> _jobHistoryManagerFake;
        private Mock<IErrorManager> _errorManagerMock;
        private Mock<IJobHistoryErrorService> _jobHistoryErrorServiceMock;
        private Mock<IChoiceQuery> _choiceQueryFake;
        private Mock<IRelativitySyncConstrainsChecker> _relativitySyncConstrainsCheckerFake;
        private SourceProvider _sourceProvider;
        private DestinationProvider _destinationProvider;
        private IntegrationPointDto _integrationPointDto;
        private Data.IntegrationPoint _integrationPoint;
        private ImportSettings _destinationConfiguration;
        private IntegrationPointType _integrationPointType;
        private int _WORKSPACE_ID;
        private int _USER_ID;

        [SetUp]
        public void SetUp()
        {
            _fxt = new Fixture().Customize(new AutoMoqCustomization() { ConfigureMembers = true });

            _destinationConfiguration = _fxt.Build<ImportSettings>()
                .With(x => x.ArtifactTypeId, _fxt.Create<int>())
                .Create();
            _sourceProvider = _fxt.Build<SourceProvider>()
                .With(x => x.Identifier, Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID.ToString())
                .Create();
            _destinationProvider = _fxt.Build<DestinationProvider>()
                .With(x => x.Identifier, Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID.ToString())
                .Create();
            _integrationPointType = _fxt.Create<IntegrationPointType>();

            _integrationPoint = _fxt.Build<Data.IntegrationPoint>()
                .With(x => x.SourceProvider, _sourceProvider.ArtifactId)
                .With(x => x.DestinationProvider, _destinationProvider.ArtifactId)
                .With(x => x.DestinationConfiguration, JsonConvert.SerializeObject(_destinationConfiguration))
                .With(x => x.Type, _integrationPointType.ArtifactId)
                .With(x => x.ScheduleRule, (string)null)
                .With(x => x.OverwriteFields, OverwriteFieldsChoices.IntegrationPointAppendOnly)
                .With(x => x.FieldMappings, string.Empty)
                .With(x => x.EnableScheduler, false)
                .With(x => x.HasErrors, false)
                .With(x => x.LogErrors, true)
                .With(x => x.PromoteEligible, false)
                .Create();

            _integrationPointDto = _fxt.Build<IntegrationPointDto>()
                .With(x => x.ArtifactId, _integrationPoint.ArtifactId)
                .With(x => x.Name, _integrationPoint.Name)
                .With(x => x.SourceProvider, _sourceProvider.ArtifactId)
                .With(x => x.SourceConfiguration, _integrationPoint.SourceConfiguration)
                .With(x => x.DestinationProvider, _destinationProvider.ArtifactId)
                .With(x => x.DestinationConfiguration, JsonConvert.SerializeObject(_destinationConfiguration))
                .With(x => x.Type, _integrationPointType.ArtifactId)
                .With(x => x.Scheduler, new Scheduler(_integrationPoint.EnableScheduler.Value, null))
                .With(x => x.SelectedOverwrite, OverwriteFieldsChoices.IntegrationPointAppendOnly.Name)
                .With(x => x.EmailNotificationRecipients, _integrationPoint.EmailNotificationRecipients)
                .With(x => x.LastRun, _integrationPoint.LastRuntimeUTC)
                .With(x => x.NextRun, _integrationPoint.NextScheduledRuntimeUTC)
                .With(x => x.SecuredConfiguration, _integrationPoint.SecuredConfiguration)
                .With(x => x.FieldMappings, new List<FieldMap>())
                .With(x => x.HasErrors, _integrationPoint.HasErrors)
                .With(x => x.LogErrors, _integrationPoint.LogErrors)
                .With(x => x.PromoteEligible, _integrationPoint.PromoteEligible)
                .Create();

            _contextFake = _fxt.Freeze<Mock<ICaseServiceContext>>();

            _WORKSPACE_ID = _contextFake.Object.WorkspaceID;
            _USER_ID = _contextFake.Object.EddsUserID;

            _integrationPointRepositoryMock = _fxt.Freeze<Mock<IIntegrationPointRepository>>();
            _integrationPointRepositoryMock.Setup(x => x.ReadAsync(_integrationPoint.ArtifactId))
                .ReturnsAsync(_integrationPoint);
            _integrationPointRepositoryMock.Setup(x => x.GetFieldMappingAsync(_integrationPoint.ArtifactId))
                .ReturnsAsync(_integrationPoint.FieldMappings);
            _integrationPointRepositoryMock.Setup(x => x.GetSourceConfigurationAsync(_integrationPoint.ArtifactId))
                .ReturnsAsync(_integrationPoint.SourceConfiguration);
            _integrationPointRepositoryMock.Setup(x => x.GetDestinationConfigurationAsync(_integrationPoint.ArtifactId))
                .ReturnsAsync(_integrationPoint.DestinationConfiguration);

            _objectManagerFake = _fxt.Freeze<Mock<IRelativityObjectManager>>();
            _objectManagerFake.Setup(x => x.Read<SourceProvider>(_sourceProvider.ArtifactId, It.IsAny<ExecutionIdentity>()))
                .Returns(_sourceProvider);
            _objectManagerFake.Setup(x => x.Read<DestinationProvider>(_destinationProvider.ArtifactId, It.IsAny<ExecutionIdentity>()))
                .Returns(_destinationProvider);
            _objectManagerFake.Setup(x => x.Read<IntegrationPointType>(_integrationPointType.ArtifactId, It.IsAny<ExecutionIdentity>()))
                .Returns(_integrationPointType);

            _jobHistoryServiceMock = _fxt.Freeze<Mock<IJobHistoryService>>();
            _jobHistoryServiceMock.Setup(x => x.CreateRdo(
                    _integrationPointDto,
                    It.IsAny<Guid>(),
                    It.IsAny<ChoiceRef>(),
                    It.IsAny<DateTime?>()))
                .Returns((IntegrationPointSlimDto _integrationPointDto, Guid batchInstanceId, ChoiceRef jobType, DateTime? startTimeUtc) =>
                {
                    return _fxt.Build<Data.JobHistory>()
                        .With(x => x.BatchInstance, batchInstanceId.ToString())
                        .Create();
                });

            _queueManagerFake = _fxt.Create<Mock<IQueueManager>>();
            _queueManagerFake.Setup(x => x.HasJobsExecutingOrInQueue(_WORKSPACE_ID, _integrationPointDto.ArtifactId))
                .Returns(false);

            _jobHistoryManagerFake = _fxt.Create<Mock<IJobHistoryManager>>();

            _errorManagerMock = _fxt.Create<Mock<IErrorManager>>();

            Mock<IManagerFactory> managerFactory = _fxt.Freeze<Mock<IManagerFactory>>();
            managerFactory.Setup(x => x.CreateQueueManager())
                .Returns(_queueManagerFake.Object);
            managerFactory.Setup(x => x.CreateJobHistoryManager())
                .Returns(_jobHistoryManagerFake.Object);
            managerFactory.Setup(x => x.CreateErrorManager())
                .Returns(_errorManagerMock.Object);

            _validationExecutorMock = _fxt.Freeze<Mock<IValidationExecutor>>();
            _validationExecutorMock.Setup(x => x.ValidateOnRun(It.IsAny<ValidationContext>()));

            _jobManagerMock = _fxt.Freeze<Mock<IJobManager>>();
            _jobManagerMock.Setup(x => x.StopJobs(It.IsAny<IList<long>>()));

            _jobHistoryErrorServiceMock = _fxt.Freeze<Mock<IJobHistoryErrorService>>();

            _choiceQueryFake = _fxt.Freeze<Mock<IChoiceQuery>>();
            _choiceQueryFake.Setup(x => x.GetChoicesOnField(_WORKSPACE_ID, IntegrationPointFieldGuids.OverwriteFieldsGuid))
                .Returns(new List<ChoiceRef>
                {
                    new ChoiceRef { ArtifactID = _fxt.Create<int>(), Name = OverwriteFieldsChoices.IntegrationPointAppendOnly.Name },
                    new ChoiceRef { ArtifactID = _fxt.Create<int>(), Name = OverwriteFieldsChoices.IntegrationPointAppendOverlay.Name },
                    new ChoiceRef { ArtifactID = _fxt.Create<int>(), Name = OverwriteFieldsChoices.IntegrationPointOverlayOnly.Name }
                });

            _relativitySyncConstrainsCheckerFake = _fxt.Freeze<Mock<IRelativitySyncConstrainsChecker>>();
            _relativitySyncConstrainsCheckerFake.Setup(x => x.ShouldUseRelativitySyncApp(_integrationPoint.ArtifactId))
                .Returns(false);

            _sut = _fxt.Create<IntegrationPointService>();
        }

        [Test]
        public void RunIntegrationPoint_GoldFlow_RelativityProvider()
        {
            // Act
            _sut.RunIntegrationPoint(_WORKSPACE_ID, _integrationPointDto.ArtifactId, _USER_ID);

            // Assert
            _validationExecutorMock.Verify(x => x.ValidateOnRun(
                It.Is<ValidationContext>(y =>
                    y.IntegrationPointType == _integrationPointType &&
                    y.SourceProvider == _sourceProvider &&
                    y.UserId == _USER_ID &&
                    y.DestinationProvider == _destinationProvider &&
                    MatchHelper.Matches(
                        _integrationPointDto,
                        y.Model) &&
                    y.ObjectTypeGuid == ObjectTypeGuids.IntegrationPointGuid)));

            _jobManagerMock.Verify(x => x.CreateJobOnBehalfOfAUser(
                It.IsAny<TaskParameters>(),
                It.IsAny<TaskType>(),
                _WORKSPACE_ID,
                _integrationPointDto.ArtifactId,
                _USER_ID,
                It.IsAny<long?>(),
                It.IsAny<long?>()));

            _jobHistoryServiceMock.Verify(x => x.CreateRdo(
                It.IsAny<IntegrationPointDto>(),
                It.IsAny<Guid>(),
                It.IsAny<ChoiceRef>(),
                It.IsAny<DateTime?>()));
        }

        [Test]
        public void MarkIntegrationPointToStopJobs_GoldFlow()
        {
            // Arrange
            const int numberOfPendingJobs = 2;
            const int numberOfProcessingJobs = 3;

            Data.JobHistory[] pendingJobHistory = _fxt.Build<Data.JobHistory>()
                .With(x => x.BatchInstance, () => Guid.NewGuid().ToString())
                .With(x => x.JobStatus, JobStatusChoices.JobHistoryPending)
                .CreateMany(numberOfPendingJobs)
                .ToArray();

            Data.JobHistory[] processingJobHistory = _fxt.Build<Data.JobHistory>()
                .With(x => x.BatchInstance, () => Guid.NewGuid().ToString())
                .With(x => x.JobStatus, JobStatusChoices.JobHistoryProcessing)
                .CreateMany(numberOfProcessingJobs)
                .ToArray();

            _jobHistoryManagerFake.Setup(x => x.GetStoppableJobHistory(_WORKSPACE_ID, _integrationPointDto.ArtifactId))
                .Returns(new StoppableJobHistoryCollection
                {
                    PendingJobHistory = pendingJobHistory,
                    ProcessingJobHistory = processingJobHistory
                });

            Dictionary<Guid, List<Job>> processingJobs = processingJobHistory
                .ToDictionary(
                    x => Guid.Parse(x.BatchInstance),
                    x => new List<Job> { _fxt.Create<Job>() });

            Dictionary<Guid, List<Job>> pendingJobs = pendingJobHistory
                .ToDictionary(
                    x => Guid.Parse(x.BatchInstance),
                    x => new List<Job> { _fxt.Create<Job>() });

            _jobManagerMock.Setup(x => x.GetJobsByBatchInstanceId(_integrationPointDto.ArtifactId))
                .Returns(pendingJobs.Concat(processingJobs).ToDictionary(x => x.Key, x => x.Value));

            // Act
            _sut.MarkIntegrationPointToStopJobs(_WORKSPACE_ID, _integrationPointDto.ArtifactId);

            // Assert
            _jobManagerMock.Verify(x => x.StopJobs(It.IsAny<IList<long>>()), Times.Exactly(numberOfProcessingJobs));

            _jobHistoryServiceMock.Verify(
                x => x.UpdateRdoWithoutDocuments(
                    It.Is<Data.JobHistory>(
                        y => y.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryStopped))),
                Times.Exactly(numberOfPendingJobs));

            _jobManagerMock.Verify(x => x.DeleteJob(It.IsAny<long>()), Times.Exactly(numberOfPendingJobs));
        }

        [Test]
        public void MarkIntegrationPointToStopJobs_ShouldStopOnlySyncAppJobs()
        {
            // Arrange
            const int numberOfPendingJobs = 2;
            const int numberOfProcessingJobs = 3;
            const int numberOfSyncAppJobs = 4;

            Data.JobHistory[] pendingJobHistory = _fxt.Build<Data.JobHistory>()
                .With(x => x.BatchInstance, () => Guid.NewGuid().ToString())
                .With(x => x.JobStatus, JobStatusChoices.JobHistoryPending)
                .CreateMany(numberOfPendingJobs)
                .ToArray();

            Data.JobHistory[] processingJobHistory = _fxt.Build<Data.JobHistory>()
                .With(x => x.BatchInstance, () => Guid.NewGuid().ToString())
                .With(x => x.JobStatus, JobStatusChoices.JobHistoryProcessing)
                .CreateMany(numberOfProcessingJobs)
                .ToArray();

            Data.JobHistory[] syncJobHistory = _fxt.Build<Data.JobHistory>()
                .With(x => x.JobID, () => Guid.NewGuid().ToString())
                .With(x => x.BatchInstance, () => Guid.NewGuid().ToString())
                .CreateMany(numberOfSyncAppJobs)
                .ToArray();

            _jobHistoryManagerFake
                .Setup(x => x.GetStoppableJobHistory(_WORKSPACE_ID, _integrationPoint.ArtifactId))
                .Returns(new StoppableJobHistoryCollection
                {
                    PendingJobHistory = pendingJobHistory,
                    ProcessingJobHistory = processingJobHistory.Concat(syncJobHistory).ToArray()
                });

            Dictionary<Guid, List<Job>> processingJobs = processingJobHistory
                .ToDictionary(
                    x => Guid.Parse(x.BatchInstance),
                    x => new List<Job> { _fxt.Create<Job>() });

            Dictionary<Guid, List<Job>> pendingJobs = pendingJobHistory
                .ToDictionary(
                    x => Guid.Parse(x.BatchInstance),
                    x => new List<Job> { _fxt.Create<Job>() });

            Dictionary<Guid, List<Job>> syncJobs = syncJobHistory
                .ToDictionary(
                    x => Guid.Parse(x.BatchInstance),
                    x => new List<Job> { _fxt.Create<Job>() });

            _jobManagerMock
                .Setup(x => x.GetJobsByBatchInstanceId(_integrationPoint.ArtifactId))
                .Returns(pendingJobs.Concat(processingJobs).Concat(syncJobs).ToDictionary(x => x.Key, x => x.Value));

            // Act
            _sut.MarkIntegrationPointToStopJobs(_WORKSPACE_ID, _integrationPoint.ArtifactId);

            // Assert
            _jobManagerMock.Verify(x => x.StopJobs(It.IsAny<IList<long>>()), Times.Exactly(numberOfProcessingJobs));

            _jobHistoryServiceMock.Verify(
                x => x.UpdateRdoWithoutDocuments(
                    It.Is<Data.JobHistory>(
                        y => y.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryStopped))),
                Times.Exactly(numberOfPendingJobs));

            _jobManagerMock.Verify(x => x.DeleteJob(It.IsAny<long>()), Times.Exactly(numberOfPendingJobs));
        }

        [Test]
        public void MarkIntegrationPointToStopJobs_ShouldThrowAndAggregateProcessingAndPendingExceptions()
        {
            // Arrange
            const int numberOfPendingJobs = 2;
            const int numberOfProcessingJobs = 3;

            Data.JobHistory[] pendingJobHistory = _fxt.Build<Data.JobHistory>()
                .With(x => x.BatchInstance, () => Guid.NewGuid().ToString())
                .With(x => x.JobStatus, JobStatusChoices.JobHistoryPending)
                .CreateMany(numberOfPendingJobs)
                .ToArray();

            Data.JobHistory[] processingJobHistory = _fxt.Build<Data.JobHistory>()
                .With(x => x.BatchInstance, () => Guid.NewGuid().ToString())
                .With(x => x.JobStatus, JobStatusChoices.JobHistoryProcessing)
                .CreateMany(numberOfProcessingJobs)
                .ToArray();

            _jobHistoryManagerFake.Setup(x => x.GetStoppableJobHistory(_WORKSPACE_ID, _integrationPointDto.ArtifactId))
                .Returns(new StoppableJobHistoryCollection
                {
                    PendingJobHistory = pendingJobHistory,
                    ProcessingJobHistory = processingJobHistory
                });

            Dictionary<Guid, List<Job>> processingJobs = processingJobHistory
                .ToDictionary(
                    x => Guid.Parse(x.BatchInstance),
                    x => new List<Job> { _fxt.Create<Job>() });

            Dictionary<Guid, List<Job>> pendingJobs = pendingJobHistory
                .ToDictionary(
                    x => Guid.Parse(x.BatchInstance),
                    x => new List<Job> { _fxt.Create<Job>() });

            _jobManagerMock.Setup(x => x.GetJobsByBatchInstanceId(_integrationPointDto.ArtifactId))
                .Returns(pendingJobs.Concat(processingJobs).ToDictionary(x => x.Key, x => x.Value));

            _jobManagerMock.Setup(x => x.StopJobs(It.IsAny<IList<long>>()))
                .Throws<EntryPointNotFoundException>();

            _jobHistoryServiceMock.Setup(x => x.UpdateRdoWithoutDocuments(It.IsAny<Data.JobHistory>()))
                .Throws<ArgumentException>();

            // Act
            Action action = () => _sut.MarkIntegrationPointToStopJobs(_WORKSPACE_ID, _integrationPointDto.ArtifactId);

            // Assert
            action.ShouldThrow<AggregateException>()
                .Where(x =>
                    x.InnerExceptions.Any(y => y is EntryPointNotFoundException) &&
                    x.InnerExceptions.Any(y => y is ArgumentException));

            _jobManagerMock.Verify(x => x.StopJobs(It.IsAny<IList<long>>()), Times.Exactly(numberOfProcessingJobs));

            _jobHistoryServiceMock.Verify(x => x.UpdateRdoWithoutDocuments(It.IsAny<Data.JobHistory>()), Times.Exactly(numberOfPendingJobs));
        }

        [Test]
        public void MarkIntegrationPointToStopJobs_InsufficientPermission()
        {
            // Arrange
            _validationExecutorMock.Setup(x => x.ValidateOnStop(It.IsAny<ValidationContext>()))
                .Throws<PermissionException>();

            // Act
            Action action = () => _sut.MarkIntegrationPointToStopJobs(_WORKSPACE_ID, _integrationPointDto.ArtifactId);

            // Assert
            action.ShouldThrow<PermissionException>();

            _errorManagerMock.Verify(x => x.Create(It.Is<IEnumerable<ErrorDTO>>(y =>
                y.Any(e =>
                    e.Message == Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS_REL_ERROR_MESSAGE &&
                    e.WorkspaceId == _WORKSPACE_ID))));
        }

        [Test]
        public void RunIntegrationPoint_ShouldThrowException_WhenInvalidPermissionsForRelativityProvider()
        {
            // Arrange
            _validationExecutorMock.Setup(x => x.ValidateOnRun(It.IsAny<ValidationContext>()))
                .Throws<InvalidOperationException>();

            // Act
            Action action = () => _sut.RunIntegrationPoint(_WORKSPACE_ID, _integrationPointDto.ArtifactId, _USER_ID);

            // Assert
            action.ShouldThrow<InvalidOperationException>();

            VerifyJobShouldNotBeCreated();

            _jobHistoryErrorServiceMock.Verify(x =>
                x.AddError(
                    It.Is<ChoiceRef>(y => y.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()));

            _jobHistoryServiceMock.Verify(x =>
                x.UpdateRdoWithoutDocuments(
                    It.Is<Data.JobHistory>(
                        y => y.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryValidationFailed))));
        }

        [Test]
        public void RunIntegrationPoint_ShouldNotRunRelativityIntegrationPoint_WhenJobIsCurrentlyRunning()
        {
            // Arrange
            _queueManagerFake.Setup(x => x.HasJobsExecutingOrInQueue(_WORKSPACE_ID, _integrationPointDto.ArtifactId))
                .Returns(true);

            // Act
            Action action = () => _sut.RunIntegrationPoint(_WORKSPACE_ID, _integrationPointDto.ArtifactId, _USER_ID);

            // Assert
            action.ShouldThrow<Exception>().WithMessage(Constants.IntegrationPoints.JOBS_ALREADY_RUNNING);

            VerifyJobShouldNotBeCreated();
        }

        [Test]
        public void RetryIntegrationPoint_ShouldThrowException_WhenIntegrationPointHasNoErrors()
        {
            // Arrange
            _integrationPoint.HasErrors = null;

            SetupLastRunJobHistory(
                _fxt.Build<Data.JobHistory>()
                    .With(x => x.JobStatus, JobStatusChoices.JobHistoryCompletedWithErrors)
                    .Create());

            // Act
            Action action = () => _sut.RetryIntegrationPoint(_WORKSPACE_ID, _integrationPointDto.ArtifactId, _USER_ID, false);

            // Assert
            action.ShouldThrow<Exception>().WithMessage(Constants.IntegrationPoints.RETRY_NO_EXISTING_ERRORS);

            _jobHistoryServiceMock.Verify(
                x => x.CreateRdo(
                    It.IsAny<IntegrationPointDto>(),
                    It.IsAny<Guid>(),
                    It.IsAny<ChoiceRef>(),
                    It.IsAny<DateTime?>()),
                Times.Never);

            VerifyJobShouldNotBeCreated();
        }

        [Test]
        public void RetryIntegrationPoint_ShouldThrowPermissionException_WhenInsufficientPermissions()
        {
            // Arrange
            _integrationPoint.HasErrors = true;

            SetupLastRunJobHistory(
                _fxt.Build<Data.JobHistory>()
                    .With(x => x.JobStatus, JobStatusChoices.JobHistoryCompletedWithErrors)
                    .Create());

            _validationExecutorMock.Setup(x => x.ValidateOnRun(It.IsAny<ValidationContext>()))
                .Throws(new Exception(Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS_REL_ERROR_MESSAGE));

            // Act
            Action action = () => _sut.RetryIntegrationPoint(_WORKSPACE_ID, _integrationPointDto.ArtifactId, _USER_ID, false);

            // Assert
            action.ShouldThrow<Exception>().WithMessage(Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS_REL_ERROR_MESSAGE);

            VerifyJobShouldNotBeCreated();

            _jobHistoryErrorServiceMock.Verify(x =>
                x.AddError(
                    It.Is<ChoiceRef>(y => y.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()));

            _jobHistoryServiceMock.Verify(x =>
                x.UpdateRdoWithoutDocuments(
                    It.Is<Data.JobHistory>(
                        y => y.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryValidationFailed))));
        }

        [Test]
        public void RetryIntegrationPoint_ShouldThrowException_WhenSourceProviderIsNotRelativity()
        {
            // Arrange
            _sourceProvider.Identifier = "Not a Relativity Provider";

            // Act
            Action action = () => _sut.RetryIntegrationPoint(_WORKSPACE_ID, _integrationPointDto.ArtifactId, _USER_ID, false);

            // Assert
            action.ShouldThrow<Exception>().WithMessage(Constants.IntegrationPoints.RETRY_IS_NOT_RELATIVITY_PROVIDER);

            VerifyJobShouldNotBeCreated();
        }

        [Test]
        public void RetryIntegrationPoint_ShouldThrow_WhenRetrievedJobHistoryToRetryIsNull()
        {
            // Arrange
            _integrationPointDto.HasErrors = true;

            int lastJobHistoryId = _fxt.Create<int>();

            _jobHistoryManagerFake.Setup(x => x.GetLastJobHistoryArtifactId(_WORKSPACE_ID, _integrationPointDto.ArtifactId))
                .Returns(lastJobHistoryId);

            _objectManagerFake.Setup(x => x.Query<Data.JobHistory>(It.IsAny<QueryRequest>(), It.IsAny<ExecutionIdentity>()))
                .Returns((List<Data.JobHistory>)null);

            // Act
            Action action = () => _sut.RetryIntegrationPoint(_WORKSPACE_ID, _integrationPointDto.ArtifactId, _USER_ID, false);

            // Assert
            action.ShouldThrow<Exception>().WithMessage(Constants.IntegrationPoints.FAILED_TO_RETRIEVE_JOB_HISTORY);
        }

        [Test]
        public void RetryIntegrationPoint_FailToRetrieveJobHistory_ReceiveException()
        {
            // Arrange
            _integrationPointDto.HasErrors = true;

            int lastJobHistoryId = _fxt.Create<int>();

            _jobHistoryManagerFake.Setup(x => x.GetLastJobHistoryArtifactId(_WORKSPACE_ID, _integrationPointDto.ArtifactId))
                .Returns(lastJobHistoryId);

            _objectManagerFake.Setup(x => x.Query<Data.JobHistory>(It.IsAny<QueryRequest>(), It.IsAny<ExecutionIdentity>()))
                .Throws<Exception>();

            // Act
            Action action = () => _sut.RetryIntegrationPoint(_WORKSPACE_ID, _integrationPointDto.ArtifactId, _USER_ID, false);

            // Assert
            action.ShouldThrow<Exception>().WithMessage(Constants.IntegrationPoints.FAILED_TO_RETRIEVE_JOB_HISTORY);
        }

        [Test]
        public void RetryIntegrationPoint_ShouldNotRetry_WhenLastJobWasStopped()
        {
            // Arrange
            _integrationPointDto.HasErrors = true;

            SetupLastRunJobHistory(
                _fxt.Build<Data.JobHistory>()
                    .With(x => x.JobStatus, JobStatusChoices.JobHistoryStopped)
                    .Create());

            // Act
            Action action = () => _sut.RetryIntegrationPoint(_WORKSPACE_ID, _integrationPointDto.ArtifactId, _USER_ID, false);

            // Assert
            action.ShouldThrow<Exception>().WithMessage(Constants.IntegrationPoints.RETRY_ON_STOPPED_JOB);
        }

        [Test]
        public void RetryIntegrationPoint_ShouldScheduleRetryJob()
        {
            // Arrange
            _integrationPoint.HasErrors = true;

            SetupLastRunJobHistory(
                _fxt.Build<Data.JobHistory>()
                    .With(x => x.JobStatus, JobStatusChoices.JobHistoryCompletedWithErrors)
                    .Create());

            // Act
            _sut.RetryIntegrationPoint(_WORKSPACE_ID, _integrationPointDto.ArtifactId, _USER_ID, false);

            // Assert
            _jobHistoryServiceMock.Verify(
                x => x.CreateRdo(
                    It.IsAny<IntegrationPointDto>(),
                    It.IsAny<Guid>(),
                    It.Is<ChoiceRef>(y => y.EqualsToChoice(JobTypeChoices.JobHistoryRetryErrors)),
                    It.IsAny<DateTime?>()));

            _jobManagerMock.Verify(x => x.CreateJobOnBehalfOfAUser(
                It.IsAny<TaskParameters>(),
                It.IsAny<TaskType>(),
                _WORKSPACE_ID,
                _integrationPointDto.ArtifactId,
                _USER_ID,
                It.IsAny<long?>(),
                It.IsAny<long?>()));
        }

        [Test]
        public void RunIntegrationPoint_ShouldNotScheduleJob_WhenSourceProviderIsNull()
        {
            // Arrange
            _integrationPoint.SourceProvider = null;

            // Act
            Action action = () => _sut.RunIntegrationPoint(_WORKSPACE_ID, _integrationPointDto.ArtifactId, _USER_ID);

            // Assert
            action.ShouldThrow<Exception>().WithMessage(Constants.IntegrationPoints.UNABLE_TO_RUN_INTEGRATION_POINT_USER_MESSAGE);

            _errorManagerMock.Verify(x => x.Create(It.IsAny<IEnumerable<ErrorDTO>>()));
        }

        [Test]
        public void SaveIntegration_ShouldThrow_WhenSourceProviderReadFailedOnExistingIntegrationPoint()
        {
            // Arrange
            IntegrationPointDto saveDto = _fxt.Build<IntegrationPointDto>()
                .With(x => x.ArtifactId, _integrationPointDto.ArtifactId)
                .With(x => x.Name, _integrationPointDto.Name)
                .With(x => x.DestinationProvider, _integrationPointDto.DestinationProvider)
                .With(x => x.SourceProvider, _sourceProvider.ArtifactId)
                .With(x => x.DestinationConfiguration, _integrationPointDto.DestinationConfiguration)
                .Create();

            _objectManagerFake.Setup(x => x.Read<SourceProvider>(_sourceProvider.ArtifactId, It.IsAny<ExecutionIdentity>()))
                .Throws<NotFoundException>();

            // Act
            Action action = () => _sut.SaveIntegrationPoint(saveDto);

            // Assert
            action.ShouldThrow<Exception>()
                .WithMessage(Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_USER_MESSAGE);

            _errorManagerMock.Verify(x => x.Create(It.IsAny<IEnumerable<ErrorDTO>>()));
        }

        [Test]
        public void SaveIntegration_ShouldThrow_WhenExceptionIsThrownOnExistingIntegrationPointRead()
        {
            // Arrange
            IntegrationPointDto saveDto = _fxt.Build<IntegrationPointDto>()
                .With(x => x.ArtifactId, _integrationPointDto.ArtifactId)
                .With(x => x.Name, _integrationPointDto.Name)
                .With(x => x.DestinationProvider, _integrationPointDto.DestinationProvider)
                .With(x => x.SourceProvider, _sourceProvider.ArtifactId)
                .With(x => x.DestinationConfiguration, _integrationPointDto.DestinationConfiguration)
                .Create();

            _integrationPointRepositoryMock.Setup(x => x.ReadAsync(_integrationPointDto.ArtifactId))
                .Throws<ServiceException>();

            // Act
            Action action = () => _sut.SaveIntegrationPoint(saveDto);

            // Assert
            action.ShouldThrow<Exception>()
                .WithMessage(Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_USER_MESSAGE);

            _errorManagerMock.Verify(x => x.Create(It.IsAny<IEnumerable<ErrorDTO>>()));
        }

        [Test]
        public void SaveIntegration_ShouldThrowPermissionException_WhenInsufficientPermissions()
        {
            // Arrange
            _validationExecutorMock.Setup(x => x.ValidateOnSave(It.IsAny<ValidationContext>()))
                .Throws<PermissionException>();

            // Act
            Action action = () => _sut.SaveIntegrationPoint(_integrationPointDto);

            // Assert
            action.ShouldThrow<PermissionException>();

            _errorManagerMock.Verify(x => x.Create(It.IsAny<IEnumerable<ErrorDTO>>()));
        }

        [Test]
        public void SaveIntegration_ShouldUpdateIntegrationPoint()
        {
            // Arrange
            string expectedNotificationReceipient = _fxt.Create<MailAddress>().Address;

            IntegrationPointDto dtoToSave = CloneIntegrationPoint(_integrationPointDto);

            dtoToSave.EmailNotificationRecipients = expectedNotificationReceipient;

            _integrationPointRepositoryMock.Setup(x => x.CreateOrUpdate(It.IsAny<Data.IntegrationPoint>()))
                .Returns((Data.IntegrationPoint integrationPoint) => integrationPoint.ArtifactId);

            // Act
            int integrationPointId = _sut.SaveIntegrationPoint(dtoToSave);

            // Assert
            integrationPointId.Should().Be(_integrationPointDto.ArtifactId);

            _integrationPointRepositoryMock.Verify(x => x.CreateOrUpdate(It.Is<Data.IntegrationPoint>(
                y => y.EmailNotificationRecipients == expectedNotificationReceipient)));
        }

        [Test]
        public void SaveIntegration_ShouldSaveIntegrationPoint()
        {
            // Arrange
            int savedIntegrationPointId = _fxt.Create<int>();

            IntegrationPointDto dtoToSave = CloneIntegrationPoint(_integrationPointDto);

            dtoToSave.ArtifactId = 0;

            _integrationPointRepositoryMock.Setup(x => x.CreateOrUpdate(It.IsAny<Data.IntegrationPoint>()))
                .Returns(savedIntegrationPointId);

            // Act
            int integrationPointId = _sut.SaveIntegrationPoint(dtoToSave);

            // Assert
            integrationPointId.Should().Be(savedIntegrationPointId);
        }

        [Test]
        public void SaveIntegration_ShouldThrow_WhenUnableToReadIntegrationPointModel()
        {
            // Arrange
            IntegrationPointDto dtoToSave = CloneIntegrationPoint(_integrationPointDto);

            _integrationPointRepositoryMock.Setup(x => x.ReadAsync(_integrationPointDto.ArtifactId))
                .Throws<NotFoundException>();

            // Act
            Action action = () => _sut.SaveIntegrationPoint(dtoToSave);

            // Assert
            action.ShouldThrow<Exception>();
        }

        [Test]
        public void SaveIntegration_ShouldCreateTheJob_WhenIntegrationPointWereScheduled()
        {
            // Arrange
            IntegrationPointDto dtoToSave = CloneIntegrationPoint(_integrationPointDto);

            dtoToSave.Scheduler = new Scheduler(true, string.Empty);

            // Act
            int artifactId = _sut.SaveIntegrationPoint(dtoToSave);

            // Assert
            _jobManagerMock.Verify(x =>
                x.CreateJob(
                    It.IsAny<TaskParameters>(),
                    It.IsAny<TaskType>(),
                    It.IsAny<int>(),
                    artifactId,
                    It.IsAny<IScheduleRule>(),
                    It.IsAny<long?>(),
                    It.IsAny<long?>()));
        }

        [Test]
        public void SaveIntegration_ShouldThrowOnUpdate_WhenNameIsDifferent()
        {
            // Arrange
            IntegrationPointDto dtoToUpdate = CloneIntegrationPoint(_integrationPointDto);

            dtoToUpdate.Name = _fxt.Create<string>();

            // Act
            Action action = () => _sut.SaveIntegrationPoint(dtoToUpdate);

            // Assert
            action.ShouldThrow<Exception>().And
                .InnerException.Message.Should().Contain("Name");
        }

        [Test]
        public void SaveIntegration_ShouldThrowOnUpdate_WhenDestinationProviderIsDifferent()
        {
            // Arrange
            IntegrationPointDto dtoToUpdate = CloneIntegrationPoint(_integrationPointDto);

            dtoToUpdate.DestinationProvider = _fxt.Create<int>();

            // Act
            Action action = () => _sut.SaveIntegrationPoint(dtoToUpdate);

            // Assert
            action.ShouldThrow<Exception>().And
                .InnerException.Message.Should().Contain("Destination Provider");
        }

        [Test]
        public void SaveIntegration_ShouldThrowOnUpdate_WhenArtifactTypeIsDifferent()
        {
            // Arrange
            IntegrationPointDto dtoToUpdate = CloneIntegrationPoint(_integrationPointDto);

            dtoToUpdate.DestinationConfiguration = JsonConvert.SerializeObject(new
            {
                artifactTypeID = _fxt.Create<int>()
            });

            // Act
            Action action = () => _sut.SaveIntegrationPoint(dtoToUpdate);

            // Assert
            action.ShouldThrow<Exception>().And
                .InnerException.Message.Should().Contain("Destination RDO");
        }

        [Test]
        public void SaveIntegration_ShouldContinueAndThrowOnUpdate_WhenMultiplePropertiesAreDifferent()
        {
            // Arrange
            IntegrationPointDto dtoToUpdate = CloneIntegrationPoint(_integrationPointDto);

            dtoToUpdate.DestinationProvider = _fxt.Create<int>();
            dtoToUpdate.Name = _fxt.Create<string>();

            // Act
            Action action = () => _sut.SaveIntegrationPoint(dtoToUpdate);

            // Assert
            action.ShouldThrow<Exception>().And
                .InnerException.Message
                .Should().Contain("Destination Provider")
                .And.Contain("Name");
        }

        [Test]
        public void ReadIntegrationPointModel_ShouldReturnIntegrationPointModel()
        {
            // Act
            IntegrationPointDto result = _sut.Read(_integrationPointDto.ArtifactId);

            // Assert
            Assert.True(MatchHelper.Matches(_integrationPointDto, result));
        }

        [Test]
        public void ReadIntegrationPoint_ShouldReturnIntegrationPoint_WhenRepositoryReturnsIntegrationPoint()
        {
            // Act
            IntegrationPointDto result = _sut.Read(_integrationPointDto.ArtifactId);

            // assert
            Assert.True(MatchHelper.Matches(_integrationPointDto, result));
        }

        [Test]
        public void ReadIntegrationPoint_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            _integrationPointRepositoryMock.Setup(x => x.ReadAsync(_integrationPoint.ArtifactId))
                .Throws<NotFoundException>();

            // Act
            Action action = () => _sut.Read(_integrationPointDto.ArtifactId);

            // Assert
            action.ShouldThrow<NotFoundException>();
        }

        private void SetupLastRunJobHistory(Data.JobHistory jobHistory)
        {
            _jobHistoryManagerFake.Setup(x => x.GetLastJobHistoryArtifactId(_WORKSPACE_ID, _integrationPoint.ArtifactId))
                .Returns(jobHistory.ArtifactId);

            _objectManagerFake.Setup(x => x.Query<Data.JobHistory>(It.IsAny<QueryRequest>(), It.IsAny<ExecutionIdentity>()))
                .Returns(new List<Data.JobHistory> { jobHistory });
        }

        private IntegrationPointDto CloneIntegrationPoint(IntegrationPointDto template)
        {
            return _fxt.Build<IntegrationPointDto>()
                .With(x => x.ArtifactId, template.ArtifactId)
                .With(x => x.Name, template.Name)
                .With(x => x.SourceProvider, template.SourceProvider)
                .With(x => x.DestinationProvider, template.DestinationProvider)
                .With(x => x.DestinationConfiguration, template.DestinationConfiguration)
                .With(x => x.SourceConfiguration, template.SourceConfiguration)
                .With(x => x.Type, template.Type)
                .With(x => x.LogErrors, template.LogErrors)
                .With(x => x.HasErrors, template.HasErrors)
                .With(x => x.LastRun, template.LastRun)
                .With(x => x.FieldMappings, template.FieldMappings)
                .With(x => x.SecuredConfiguration, template.SecuredConfiguration)
                .With(x => x.SelectedOverwrite, template.SelectedOverwrite)
                .Create();
        }

        private void VerifyJobShouldNotBeCreated()
        {
            _jobManagerMock.Verify(
                x => x.CreateJobOnBehalfOfAUser(
                    It.IsAny<TaskParameters>(),
                    It.IsAny<TaskType>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<long?>(),
                    It.IsAny<long>()),
                Times.Never());
        }
    }
}
