using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Tests.Services.JobHistory
{
    [TestFixture, Category("Unit")]
    public class JobHistoryErrorServiceTest : TestBase
    {
        private Data.IntegrationPoint _integrationPoint;
        private Data.JobHistory _jobHistory;
        private Mock<ICaseServiceContext> _caseServiceContextFake;
        private Mock<IHelper> _helperFake;
        private JobHistoryErrorService _instance;
        private Mock<IJobStopManager> _stopJobManagerFake;
        private Mock<IIntegrationPointRepository> _integrationPointRepositoryFake;
        private Mock<IObjectManager> _objectManagerFake;
        private List<JobHistoryError> _errors;

        private readonly Guid _jobHistoryErrorObject = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");
        private readonly Guid _errorMessageField = new Guid("4112B894-35B0-4E53-AB99-C9036D08269D");
        private readonly Guid _errorStatusField = new Guid("DE1A46D2-D615-427A-B9F2-C10769BC2678");
        private readonly Guid _errorTypeField = new Guid("EEFFA5D3-82E3-46F8-9762-B4053D73F973");
        private readonly Guid _nameField = new Guid("84E757CC-9DA2-435D-B288-0C21EC589E66");
        private readonly Guid _sourceUniqueIdField = new Guid("5519435E-EE82-4820-9546-F1AF46121901");
        private readonly Guid _stackTraceField = new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF");
        private readonly Guid _timestampUtcField = new Guid("B9CBA772-E7C9-493E-B7F8-8D605A6BFE1F");

        [SetUp]
        public override void SetUp()
        {
            _integrationPoint = new Data.IntegrationPoint() { LogErrors = true };
            _jobHistory = new Data.JobHistory { ArtifactId = 111 };

            _caseServiceContextFake = new Mock<ICaseServiceContext>();
            _helperFake = new Mock<IHelper>();

            _helperFake.Setup(x => x.GetLoggerFactory().GetLogger().ForContext<IJobHistoryErrorService>()).Returns(new Mock<IAPILog>().Object);

            _stopJobManagerFake = new Mock<IJobStopManager>();
            _integrationPointRepositoryFake = new Mock<IIntegrationPointRepository>();
            _objectManagerFake = new Mock<IObjectManager>();

            _helperFake.Setup(x => x.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
                .Returns(_objectManagerFake.Object);
            _errors = new List<JobHistoryError>();

            _objectManagerFake.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<MassCreateRequest>()))
                .ReturnsAsync(new MassCreateResult
                {
                    Success = true
                })
                .Callback<int, MassCreateRequest>((_, req) => _errors.AddRange(ExtractErrors(req.ValueLists)));

            _instance = new JobHistoryErrorService(_caseServiceContextFake.Object, _helperFake.Object, _integrationPointRepositoryFake.Object)
            {
                IntegrationPoint = _integrationPoint,
                JobHistory = _jobHistory,
                JobStopManager = _stopJobManagerFake.Object
            };
        }

        [Test]
        public void CommitErrors_ShouldCallObjectManagerWithCorrectParameters()
        {
            // arrange
            _instance.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyIdentifier", "Fake item error.", "stack trace");
            MassCreateRequest request = null;

            _objectManagerFake.Setup(x =>
                    x.CreateAsync(_caseServiceContextFake.Object.WorkspaceID, It.IsAny<MassCreateRequest>()))
                .ReturnsAsync(new MassCreateResult
                {
                    Success = true
                })
                .Callback<int, MassCreateRequest>((_, req) => request = req);

            // act
            _instance.CommitErrors();

            // assert
            request.ObjectType.Guid.Should().Be(_jobHistoryErrorObject);
            request.Fields.Select(x => x.Guid).Should().BeEquivalentTo(new[]
            {
                _errorMessageField,
                _errorStatusField,
                _errorTypeField,
                _nameField,
                _sourceUniqueIdField,
                _stackTraceField,
                _timestampUtcField
            });
        }

        [Test]
        public void CommitErrors_ShouldThrowWhenMassCreateFails()
        {
            // arrange
            var errorMessageFromObjectmanager = "Error message from ObjectManager";
            _objectManagerFake.Setup(x =>
                    x.CreateAsync(_caseServiceContextFake.Object.WorkspaceID, It.IsAny<MassCreateRequest>()))
                .ReturnsAsync(new MassCreateResult
                {
                    Success = false,
                    Message = errorMessageFromObjectmanager
                });

            _instance.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyIdentifier", "Fake item error.", "stack trace");


            // act & assert
            new Action(() => _instance.CommitErrors()).ShouldThrow<Exception>();
        }

        [Test]
        public void AddError_ShouldUseExceptionMessage()
        {
            // arrange
            var errorMessages = new[] { "Message 1", "Message 2" };

            // act 
            _instance.AddError(ErrorTypeChoices.JobHistoryErrorJob, new IntegrationPointValidationException(new ValidationResult(errorMessages)));

            // assert
            _errors.Single().Error.Should().Be("Integration Point validation failed.\r\n" + string.Join(Environment.NewLine, errorMessages));
        }

        [Test]
        public void CommitErrors_HasJobHistory_CommitsJobHistoryErrors_ForDocumentLevelErrors()
        {
            // arrange
            _instance.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyIdentifier", "Fake item error.", "stack trace");
            _instance.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyIdentifier2", "Fake item error2.", "stack trace2");

            _instance.IntegrationPoint.HasErrors = false;

            // act
            _instance.CommitErrors();

            // assert
            _objectManagerFake.Verify(x => x.CreateAsync(_caseServiceContextFake.Object.WorkspaceID, It.IsAny<MassCreateRequest>()), Times.Once);

            Assert.AreEqual(2, _errors.Count);
            Assert.AreEqual(ErrorTypeChoices.JobHistoryErrorItem.Name, _errors[0].ErrorType.Name);
            Assert.AreEqual("Fake item error.", _errors[0].Error);
            Assert.AreEqual("stack trace", _errors[0].StackTrace);
            Assert.AreEqual(ErrorTypeChoices.JobHistoryErrorItem.Name, _errors[1].ErrorType.Name);
            Assert.AreEqual("Fake item error2.", _errors[1].Error);
            Assert.AreEqual("stack trace2", _errors[1].StackTrace);
            Assert.IsNotNull(_instance.IntegrationPoint.HasErrors);
            Assert.IsTrue(_instance.IntegrationPoint.HasErrors.Value);
        }

        [Test]
        public void AddError_CommitsJobHistoryErrors_ForJobLevelErrors()
        {
            // arrange
            _instance.IntegrationPoint.HasErrors = false;

            // act
            _instance.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error.", "stack trace");
            _instance.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error2.", "stack trace2");

            _objectManagerFake.Verify(x => x.CreateAsync(_caseServiceContextFake.Object.WorkspaceID, It.IsAny<MassCreateRequest>()), Times.Exactly(2));
            Assert.AreEqual(2, _errors.Count);
        }

        [Test]
        public void CommitErrors_HasJobHistory_NoErrorsToCommit()
        {
            // arrange
            _instance.IntegrationPoint.HasErrors = true;

            // act
            _instance.CommitErrors();

            // assert
            _objectManagerFake.Verify(x => x.CreateAsync(_caseServiceContextFake.Object.WorkspaceID, It.IsAny<MassCreateRequest>()), Times.Never);
            Assert.IsNotNull(_instance.IntegrationPoint.HasErrors);
            Assert.IsFalse(_instance.IntegrationPoint.HasErrors.Value);
        }

        [Test]
        public void CommitErrors_FailsCommit_ThrowsException_ItemLevelError()
        {
            // arrange
            _instance.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyIdentifier", "Fake item error.", null);

            _objectManagerFake.Setup(x => x.CreateAsync(_caseServiceContextFake.Object.WorkspaceID, It.IsAny<MassCreateRequest>()))
                .ThrowsAsync(new Exception());

            _instance.IntegrationPoint.HasErrors = false;

            // act
            Exception returnedException = Assert.Throws<Exception>(() => _instance.CommitErrors());

            // assert
            _integrationPointRepositoryFake.Verify(x => x.Update(It.IsAny<Data.IntegrationPoint>()));

            Assert.IsTrue(returnedException.Message.Contains("Could not commit Job History Errors. These are uncommitted errors:" + Environment.NewLine));
            Assert.IsTrue(returnedException.Message.Contains("Type: Item    Identifier: MyIdentifier    Error: Fake item error."));
        }

        [Test]
        public void CommitErrors_FailsCommit_ThrowsException_JobLevelError()
        {
            // arrange
            _objectManagerFake.Setup(x => x.CreateAsync(_caseServiceContextFake.Object.WorkspaceID, It.IsAny<MassCreateRequest>()))
                .ThrowsAsync(new Exception());
            _instance.IntegrationPoint.HasErrors = false;

            // act
            //Adding job level error automatically commits errors
            Exception returnedException = Assert.Throws<Exception>(() => _instance.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error.", null));

            // assert
            _integrationPointRepositoryFake.Verify(x => x.Update(It.IsAny<Data.IntegrationPoint>()), Times.Once);
            Assert.IsTrue(returnedException.Message.Contains("Could not commit Job History Errors. These are uncommitted errors:" + Environment.NewLine));
            Assert.IsTrue(returnedException.Message.Contains("Type: Job    Error: Fake job error." + Environment.NewLine));
        }

        [Test]
        public void AddError_NoJobHistory_ThrowsException()
        {
            // arrange
            _instance.JobHistory = null;

            // act
            Exception returnedException = Assert.Throws<Exception>(() => _instance.AddError(ErrorTypeChoices.JobHistoryErrorJob, "", "Fake job error.", null));

            // assert
            _integrationPointRepositoryFake.Verify(x => x.Update(It.IsAny<Data.IntegrationPoint>()), Times.Never);
            Assert.That(returnedException.Message, Is.EqualTo("Type:Job Id:  Error:Fake job error."));
        }

        [Test]
        [Category(TestConstants.TestCategories.STOP_JOB)]
        public void OnRowError_DoNotAddErrorWhenStopped()
        {
            // arrange
            const string identifier = "identifier";
            Reporter reporter = new Reporter();
            _stopJobManagerFake.Setup(x => x.IsStopRequested()).Returns(true);

            // act
            _instance.SubscribeToBatchReporterEvents(reporter);
            reporter.RaiseDocumentError(identifier, identifier);

            // assert
            _instance.PendingErrorCount.Should().Be(0);
        }

        [Test]
        [Category(TestConstants.TestCategories.STOP_JOB)]
        public void OnRowError_AddErrorWhenRunning()
        {
            // arrange
            const string identifier = "identifier";
            Reporter reporter = new Reporter();
            _stopJobManagerFake.Setup(x => x.IsStopRequested()).Returns(false);

            // act
            _instance.SubscribeToBatchReporterEvents(reporter);
            reporter.RaiseDocumentError(identifier, identifier);

            // assert
            _instance.PendingErrorCount.Should().Be(1);
        }

        [Test]
        public void AddError_CommitErrorsByBatch()
        {
            // arrange
            Exception exception = new Exception();
            Reporter reporter = new Reporter();
            _stopJobManagerFake.Setup(x => x.IsStopRequested()).Returns(true);
            MassCreateRequest request = null;

            _objectManagerFake.Setup(x =>
                    x.CreateAsync(_caseServiceContextFake.Object.WorkspaceID, It.IsAny<MassCreateRequest>()))
                .ReturnsAsync(new MassCreateResult
                {
                    Success = true
                })
                .Callback<int, MassCreateRequest>((_, req) => request = req);

            // act
            _instance.SubscribeToBatchReporterEvents(reporter);
            for (int i = 0; i < JobHistoryErrorService.ERROR_BATCH_SIZE; i++)
            {
                _instance.AddError(ErrorTypeChoices.JobHistoryErrorItem, exception);
            }

            // assert 
            request.ValueLists.Count.Should().Be(JobHistoryErrorService.ERROR_BATCH_SIZE);
        }
        
        [Test]
        public void CommitErrors_SetHasErrorToFalseWhenStopAndNoErrorOccured()
        {
            // arrange
            _stopJobManagerFake.Setup(x => x.IsStopRequested()).Returns(true);

            // act
            _instance.CommitErrors();

            // assert
            Assert.IsFalse(_integrationPoint.HasErrors);
            _integrationPointRepositoryFake.Verify(x => x.Update(_integrationPoint), Times.Once);
        }

        [Test]
        public void CommitErrors_SetHasErrorToFalseWhenRunningAndNoErrorOccured()
        {
            // arrange
            _stopJobManagerFake.Setup(x => x.IsStopRequested()).Returns(false);

            // act
            _instance.CommitErrors();

            // assert
            Assert.IsFalse(_integrationPoint.HasErrors);
            _integrationPointRepositoryFake.Verify(x => x.Update(_integrationPoint), Times.Once);
        }

        [Test]
        public void CommitErrors_SetHasErrorToFalseWhenStopAndErrorsOccured()
        {
            // arrange
            _instance.AddError(ErrorTypeChoices.JobHistoryErrorItem, new Exception());
            _stopJobManagerFake.Setup(x => x.IsStopRequested()).Returns(true);

            // act
            _instance.CommitErrors();

            // assert
            Assert.IsFalse(_integrationPoint.HasErrors);
            _integrationPointRepositoryFake.Verify(x => x.Update(_integrationPoint), Times.Once);
        }

        [Test]
        public void CommitErrors_SetHasErrorToTrueWhenRunningAndErrorsOccured()
        {
            // arrange
            _instance.AddError(ErrorTypeChoices.JobHistoryErrorItem, new Exception());
            _stopJobManagerFake.Setup(x => x.IsStopRequested()).Returns(false);

            // act
            _instance.CommitErrors();

            // assert
            Assert.IsTrue(_integrationPoint.HasErrors);
            _integrationPointRepositoryFake.Verify(x => x.Update(_integrationPoint), Times.Once);
        }

        [Test]
        public void CommitErrors_SuppressErrorOnUpdateHasErrorField()
        {
            // arrange
            _integrationPointRepositoryFake
                .Setup(x => x.Update(It.IsAny<Data.IntegrationPoint>()))
                .Throws(new Exception());

            // act & assert
            Assert.DoesNotThrow(() => _instance.CommitErrors());
        }

        private class Reporter : IBatchReporter
        {
            public event BatchCompleted OnBatchComplete { add { } remove { } }
            public event BatchSubmitted OnBatchSubmit { add { } remove { } }
            public event BatchCreated OnBatchCreate { add { } remove { } }
            public event StatusUpdate OnStatusUpdate { add { } remove { } }
            public event StatisticsUpdate OnStatisticsUpdate { add { } remove { } }
            public event JobError OnJobError;
            public event RowError OnDocumentError;

            public void RaiseDocumentError(string identifier, string msg)
            {
                OnDocumentError?.Invoke(identifier, msg);
            }

            public void RaiseOnJobError(Exception ex)
            {
                OnJobError?.Invoke(ex);
            }
        }

        private IEnumerable<JobHistoryError> ExtractErrors(IReadOnlyCollection<IReadOnlyCollection<object>> values)
        {
            return values.Select(x => new JobHistoryError
            {
                Error = x.ElementAt(0).ToString(),
                ErrorType = (x.ElementAt(2) as ChoiceRef).Guid == ErrorTypeChoices.JobHistoryErrorItemGuid ? ErrorTypeChoices.JobHistoryErrorItem : ErrorTypeChoices.JobHistoryErrorJob,
                SourceUniqueID = x.ElementAt(4).ToString(),
                StackTrace = x.ElementAt(5).ToString(),
            });
        }
    }
}