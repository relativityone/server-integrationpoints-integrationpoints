using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Utils;
using Relativity.Sync.Utils.Workarounds;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    public class JobProgressUpdaterTests
    {
        private Mock<IDateTime> _dateTime;
        private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepository;
        private Mock<IRdoGuidConfiguration> _rdoGuidConfiguration;
        private Mock<IRipWorkarounds> _ripWorkarounds;
        private Mock<ISourceServiceFactoryForAdmin> _serviceFactory;
        private Mock<IObjectManager> _objectManager;
        private SyncJobParameters _syncJobParameters;

        private const int _WORKSPACE_ID = 111;
        private const int _JOB_HISTORY_ID = 222;
        private const int _USER_ID = 333;
        private const int _SYNC_CONFIGURATION_ID = 444;

        private Guid _workflowId = Guid.NewGuid();

        private Guid _jobHistoryErrorTypeGuid = new Guid("0C88EB14-14D5-4FED-85C1-56F5885D76B9");
        private Guid _jobHistoryRelationGuid = new Guid("61314C61-3504-4054-BF97-09F8A0D08B77");
        private Guid _errorTypeGuid = new Guid("67936C04-A0A8-4C15-B055-F37D5978F4EF");
        private Guid _itemLevelErrorGuid = new Guid("6D9DAB63-4A26-4E36-A2FC-2EDD73B8C29C");

        private Guid _jobHistoryTypeGuid = new Guid("DF0A4E86-251E-4B21-870D-265C9B00B0F5");
        private Guid _completedItemsFieldGuid = new Guid("EC869E59-933F-44C8-9E9F-5F1C4619B1AA");
        private Guid _failedItemsFieldGuid = new Guid("ABC708E9-4DB9-4B62-B2E9-3EEF0166A695");
        private Guid _totalItemsFieldGuid = new Guid("B54407A6-26F5-48CE-9079-0A99A49C9CF3");
        private Guid _jobIdGuid = new Guid("77d797ef-96c9-4b47-9ef8-33f498b5af0d");
        private Guid _startTimeGuid = new Guid("25b7c8ef-66d9-41d1-a8de-29a93e47fb11");
        private Guid _endTimeGuid = new Guid("4736cf49-ad0f-4f02-aaaa-898e07400f22");
        private Guid _statusGuid = new Guid("5c28ce93-c62f-4d25-98c9-9a330a6feb52");

        private Guid _completedGuid = new Guid("c7d1eb34-166e-48d0-bce7-0be0df43511c");
        private Guid _completedWithErrorsGuid = new Guid("c0f4a2b2-499e-45bc-96d7-f8bc25e18b37");
        private Guid _jobFailedGuid = new Guid("3152ece9-40e6-44dd-afc8-1004f55dfb63");
        private Guid _processingGuid = new Guid("bb170e53-2264-4708-9b00-86156187ed54");
        private Guid _stoppedGuid = new Guid("a29c5bcb-d3a6-4f81-877a-2a6556c996c3");
        private Guid _stoppingGuid = new Guid("97c1410d-864d-4811-857b-952464872baa");
        private Guid _suspendedGuid = new Guid("f219e060-d7e1-4666-964d-f229a1a13baa");
        private Guid _suspendingGuid = new Guid("c65658c3-79ea-4762-b78e-85d9f38785b6");
        private Guid _validationFailedGuid = new Guid("d0b43a57-bdc8-4c14-b2f0-2928ae4f750a");
        private Guid _validatingGuid = new Guid("6a2dcef5-5826-4f61-9bac-59fef879ebc2");

        [SetUp]
        public void SetUp()
        {
            _dateTime = new Mock<IDateTime>();
            _jobHistoryErrorRepository = new Mock<IJobHistoryErrorRepository>();
            _rdoGuidConfiguration = new Mock<IRdoGuidConfiguration>();
            _ripWorkarounds = new Mock<IRipWorkarounds>();
            _objectManager = new Mock<IObjectManager>();
            _serviceFactory = new Mock<ISourceServiceFactoryForAdmin>();

            _serviceFactory
                .Setup(x => x.CreateProxyAsync<IObjectManager>())
                .ReturnsAsync(_objectManager.Object);

            _syncJobParameters = new SyncJobParameters(_SYNC_CONFIGURATION_ID, _WORKSPACE_ID, _USER_ID, _workflowId, Guid.Empty);

            SetupGuids();
        }

        [Test]
        public async Task SetTotalItemsCountAsync_ShouldSetTotalItems()
        {
            // Arrange
            const int totalItems = 50;

            JobProgressUpdater sut = PrepareSut();

            // Act
            await sut.SetTotalItemsCountAsync(totalItems);

            // Assert
            FieldRefValuePair[] expectedFields = new[]
            {
                new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = _totalItemsFieldGuid
                    },
                    Value = totalItems
                }
            };
            _objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(req => VerifyJobHistoryUpdateRequest(req, expectedFields))));
        }

        [Test]
        public async Task SetJobStartedAsync_ShouldSetJobIdAndStartTime_WhenMissing()
        {
            // Arrange
            DateTime startTime = DateTime.Now;
            _dateTime.SetupGet(x => x.UtcNow).Returns(startTime);

            _objectManager
                .Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(req => req.ObjectType.Guid == _jobHistoryTypeGuid &&
                                                                                             req.Condition == $"'Artifact ID' == {_JOB_HISTORY_ID}" &&
                                                                                             req.Fields.Single().Guid == _jobIdGuid), 0, 1))
                .ReturnsAsync(new QueryResult()
                {
                    Objects = new List<RelativityObject>()
                    {
                        new RelativityObject()
                        {
                            FieldValues = new List<FieldValuePair>()
                            {
                                new FieldValuePair()
                                {
                                    Value = string.Empty
                                }
                            }
                        }
                    }
                });

            Guid jobId = Guid.NewGuid();
            _syncJobParameters = new SyncJobParameters(_SYNC_CONFIGURATION_ID, _WORKSPACE_ID, _USER_ID, _workflowId, jobId);

            JobProgressUpdater sut = PrepareSut();

            // Act
            await sut.SetJobStartedAsync();

            // Assert
            FieldRefValuePair[] expectedFields = new[]
            {
                new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = _startTimeGuid
                    },
                    Value = startTime
                },
                new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = _jobIdGuid
                    },
                    Value = jobId.ToString()
                }
            };
            _objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(req => VerifyJobHistoryUpdateRequest(req, expectedFields))));
        }

        [Test]
        public async Task SetJobStartedAsync_ShouldNotSetJobIdAndStartTime_WhenAlreadySet()
        {
            // Arrange
            const int jobId = 123;

            _rdoGuidConfiguration.SetupGet(x => x.JobHistory.TypeGuid).Returns(_jobHistoryTypeGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistory.JobIdGuid).Returns(_jobIdGuid);

            _objectManager
                .Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(req => req.ObjectType.Guid == _jobHistoryTypeGuid &&
                                                                                   req.Condition == $"'Artifact ID' == {_JOB_HISTORY_ID}" &&
                                                                                   req.Fields.Single().Guid == _jobIdGuid), 0, 1))
                .ReturnsAsync(new QueryResult()
                {
                    Objects = new List<RelativityObject>()
                    {
                        new RelativityObject()
                        {
                            FieldValues = new List<FieldValuePair>()
                            {
                                new FieldValuePair()
                                {
                                    Value = jobId
                                }
                            }
                        }
                    }
                });

            JobProgressUpdater sut = PrepareSut();

            // Act
            await sut.SetJobStartedAsync();

            // Assert
            _objectManager.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>()), Times.Never);
        }

        [Test]
        public async Task UpdateJobProgressAsync_ShouldUpdateCompletedAndFailedRecords()
        {
            // Arrange
            const int completedItems = 555;
            const int failedItems = 666;

            _rdoGuidConfiguration.SetupGet(x => x.JobHistory.CompletedItemsFieldGuid).Returns(_completedItemsFieldGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistory.FailedItemsFieldGuid).Returns(_failedItemsFieldGuid);

            JobProgressUpdater sut = PrepareSut();

            // Act
            await sut.UpdateJobProgressAsync(completedItems, failedItems);

            // Assert
            FieldRefValuePair[] expectedFields = new[]
            {
                new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = _completedItemsFieldGuid
                    },
                    Value = completedItems
                },
                new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = _failedItemsFieldGuid
                    },
                    Value = failedItems
                }
            };
            _objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(req => VerifyJobHistoryUpdateRequest(req, expectedFields))));
        }

        [Test]
        public async Task AddJobErrorAsync_ShouldAddError()
        {
            // Arrange
            const string message = "Error message";
            InvalidOperationException exception = new InvalidOperationException(message);
            JobProgressUpdater sut = PrepareSut();

            // Act
            await sut.AddJobErrorAsync(exception);

            // Assert
            _jobHistoryErrorRepository.Verify(x => x.CreateAsync(_WORKSPACE_ID, _JOB_HISTORY_ID, It.Is<CreateJobHistoryErrorDto>(dto => dto.ErrorMessage == message)), Times.Once);
        }

        [TestCase(JobHistoryStatus.Validating)]
        [TestCase(JobHistoryStatus.Processing)]
        [TestCase(JobHistoryStatus.Stopping)]
        [TestCase(JobHistoryStatus.Suspending)]
        public async Task UpdateJobStatusAsync_ShouldUpdateJobStatus_WhenInProgress(JobHistoryStatus status)
        {
            // Arrange
            Guid statusGuid = SetupJobHistoryStatusGuid(status);
            JobProgressUpdater sut = PrepareSut();

            // Act
            await sut.UpdateJobStatusAsync(status);

            // Assert
            FieldRefValuePair[] expectedFields = new[]
            {
                new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = _statusGuid
                    },
                    Value = new ChoiceRef()
                    {
                        Guid = statusGuid
                    }
                }
            };
            _objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(req => VerifyJobHistoryUpdateRequest(req, expectedFields))));
        }

        [TestCase(JobHistoryStatus.ValidationFailed)]
        [TestCase(JobHistoryStatus.CompletedWithErrors)]
        [TestCase(JobHistoryStatus.Failed)]
        [TestCase(JobHistoryStatus.Stopped)]
        [TestCase(JobHistoryStatus.Suspended)]
        public async Task UpdateJobStatusAsync_ShouldUpdateJobStatus_WhenFinishedButNotCompleted(JobHistoryStatus status)
        {
            // Arrange
            const bool hasErrors = true;
            _jobHistoryErrorRepository.Setup(x => x.HasErrorsAsync(_WORKSPACE_ID, _JOB_HISTORY_ID)).ReturnsAsync(hasErrors);
            Guid statusGuid = SetupJobHistoryStatusGuid(status);
            DateTime endTime = DateTime.UtcNow;
            _dateTime.SetupGet(x => x.UtcNow).Returns(endTime);

            JobProgressUpdater sut = PrepareSut();

            // Act
            await sut.UpdateJobStatusAsync(status);

            // Assert
            FieldRefValuePair[] expectedFields = new[]
            {
                new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = _statusGuid
                    },
                    Value = new ChoiceRef()
                    {
                        Guid = statusGuid
                    }
                },
                new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = _endTimeGuid
                    },
                    Value = endTime
                }
            };
            _objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(req => VerifyJobHistoryUpdateRequest(req, expectedFields))));
            _ripWorkarounds.Verify(x => x.TryUpdateIntegrationPointAsync(_WORKSPACE_ID, _JOB_HISTORY_ID, hasErrors, endTime));
        }

        [Test]
        public async Task UpdateJobStatusAsync_ShouldUpdateJobStatus_WhenCompletedWithoutErrors()
        {
            // Arrange
            _jobHistoryErrorRepository.Setup(x => x.HasErrorsAsync(_WORKSPACE_ID, _JOB_HISTORY_ID)).ReturnsAsync(false);
            Guid statusGuid = SetupJobHistoryStatusGuid(JobHistoryStatus.Completed);
            JobProgressUpdater sut = PrepareSut();

            // Act
            await sut.UpdateJobStatusAsync(JobHistoryStatus.Completed);

            // Assert
            FieldRefValuePair[] expectedFields = new[]
            {
                new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = _statusGuid
                    },
                    Value = new ChoiceRef()
                    {
                        Guid = statusGuid
                    }
                }
            };
            _objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(req => VerifyJobHistoryUpdateRequest(req, expectedFields))));
        }

        [Test]
        public async Task UpdateJobStatusAsync_ShouldUpdateJobStatus_WhenCompletedWithErrors()
        {
            // Arrange
            _jobHistoryErrorRepository.Setup(x => x.HasErrorsAsync(_WORKSPACE_ID, _JOB_HISTORY_ID)).ReturnsAsync(true);
            Guid statusGuid = SetupJobHistoryStatusGuid(JobHistoryStatus.CompletedWithErrors);
            JobProgressUpdater sut = PrepareSut();

            // Act
            await sut.UpdateJobStatusAsync(JobHistoryStatus.Completed);

            // Assert
            FieldRefValuePair[] expectedFields = new[]
            {
                new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = _statusGuid
                    },
                    Value = new ChoiceRef()
                    {
                        Guid = statusGuid
                    }
                }
            };
            _objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(req => VerifyJobHistoryUpdateRequest(req, expectedFields))));
        }

        private bool VerifyJobHistoryUpdateRequest(UpdateRequest request, params FieldRefValuePair[] expectedFields)
        {
            request.Object.ArtifactID.Should().Be(_JOB_HISTORY_ID, "UpdateRequest should have valid Object Artifact ID");

            foreach (FieldRefValuePair expectedField in expectedFields)
            {
                FieldRefValuePair field = request.FieldValues.SingleOrDefault(x => x.Field.Guid == expectedField.Field.Guid);
                field.Should().NotBeNull($"Field {expectedField.Field.Guid} should be present in the request");

                if (field.Value is ChoiceRef actualChoice)
                {
                    ChoiceRef expectedChoice = expectedField.Value as ChoiceRef;
                    actualChoice.ArtifactID.Should().Be(expectedChoice.ArtifactID);
                    actualChoice.Guid.Should().Be(expectedChoice.Guid);
                }
                else
                {
                    field.Value.Should().Be(expectedField.Value, $"Field {expectedField.Field.Guid} should have proper value");
                }
            }

            return true;
        }

        private JobProgressUpdater PrepareSut()
        {
            return new JobProgressUpdater(_serviceFactory.Object, _rdoGuidConfiguration.Object, _dateTime.Object, _jobHistoryErrorRepository.Object, _ripWorkarounds.Object, _syncJobParameters, new EmptyLogger(), _WORKSPACE_ID, _JOB_HISTORY_ID);
        }

        private Guid SetupJobHistoryStatusGuid(JobHistoryStatus status)
        {
            switch (status)
            {
                case JobHistoryStatus.ValidationFailed:
                    return _validationFailedGuid;
                case JobHistoryStatus.Completed:
                    return _completedGuid;
                case JobHistoryStatus.CompletedWithErrors:
                    return _completedWithErrorsGuid;
                case JobHistoryStatus.Failed:
                    return _jobFailedGuid;
                case JobHistoryStatus.Processing:
                    return _processingGuid;
                case JobHistoryStatus.Stopped:
                    return _stoppedGuid;
                case JobHistoryStatus.Stopping:
                    return _stoppingGuid;
                case JobHistoryStatus.Suspended:
                    return _suspendedGuid;
                case JobHistoryStatus.Suspending:
                    return _suspendingGuid;
                case JobHistoryStatus.Validating:
                    return _validatingGuid;
                default:
                    throw new InvalidOperationException($"Unknown status: {status}");
            }
        }

        private void SetupGuids()
        {
            _rdoGuidConfiguration.SetupGet(x => x.JobHistory.StartTimeGuid).Returns(_startTimeGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistory.EndTimeGuid).Returns(_endTimeGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistory.JobIdGuid).Returns(_jobIdGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistory.TotalItemsFieldGuid).Returns(_totalItemsFieldGuid);

            _rdoGuidConfiguration.SetupGet(x => x.JobHistoryStatus.ValidationFailedGuid).Returns(_validationFailedGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistoryStatus.CompletedGuid).Returns(_completedGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistoryStatus.CompletedWithErrorsGuid).Returns(_completedWithErrorsGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistoryStatus.JobFailedGuid).Returns(_jobFailedGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistoryStatus.ProcessingGuid).Returns(_processingGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistoryStatus.StoppedGuid).Returns(_stoppedGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistoryStatus.StoppingGuid).Returns(_stoppingGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistoryStatus.SuspendedGuid).Returns(_suspendedGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistoryStatus.SuspendingGuid).Returns(_suspendingGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistoryStatus.ValidatingGuid).Returns(_validatingGuid);

            _rdoGuidConfiguration.SetupGet(x => x.JobHistory.StatusGuid).Returns(_statusGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistory.TypeGuid).Returns(_jobHistoryTypeGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistory.FailedItemsFieldGuid).Returns(_failedItemsFieldGuid);

            _rdoGuidConfiguration.SetupGet(x => x.JobHistoryError.TypeGuid).Returns(_jobHistoryErrorTypeGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistoryError.JobHistoryRelationGuid).Returns(_jobHistoryRelationGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistoryError.ErrorTypeGuid).Returns(_errorTypeGuid);
            _rdoGuidConfiguration.SetupGet(x => x.JobHistoryError.ItemLevelErrorGuid).Returns(_itemLevelErrorGuid);
        }
    }
}
