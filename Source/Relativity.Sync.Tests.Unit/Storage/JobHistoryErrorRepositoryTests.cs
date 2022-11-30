using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Storage
{
    [TestFixture]
    internal sealed class JobHistoryErrorRepositoryTests
    {
        private JobHistoryErrorRepository _sut;
        private Mock<ISourceServiceFactoryForUser> _serviceFactoryForUserFake;
        private Mock<IObjectManager> _objectManagerMock;
        private Mock<IDateTime> _dateTimeFake;
        private DateTime _utcNow;

        private const ErrorType _TEST_ERROR_TYPE_ITEM = ErrorType.Item;

        private const int _TEST_JOB_HISTORY_ARTIFACT_ID = 101800;
        private const int _TEST_WORKSPACE_ARTIFACT_ID = 101789;
        private const int _TEST_ERROR_TYPE_JOB_ARTIFACT_ID = 102556;
        private const string _TEST_ERROR_MESSAGE = "Test error";
        private const string _TEST_SOURCE_UNIQUE_ID = "101810";
        private const string _TEST_STACK_TRACE = "Test stack trace.";

        private const string _ENTITY_TOO_LARGE_EXCEPTION = "Request Entity Too Large";
        private const string _VIOLATION_OF_PRIMARY_KEY_EXCPETION = "Violation of PRIMARY KEY constraint 'PK_1000081'. Cannot insert duplicate key in object 'EDDSDBO.JobHistoryError'. The duplicate key value is (596699325)";

        private readonly Guid _expectedErrorMessageField = new Guid("4112B894-35B0-4E53-AB99-C9036D08269D");
        private readonly Guid _expectedErrorStatusField = new Guid("DE1A46D2-D615-427A-B9F2-C10769BC2678");

        private readonly Guid _expectedErrorStatusNew = new Guid("F881B199-8A67-4D49-B1C1-F9E68658FB5A");
        private readonly Guid _expectedErrorTypeField = new Guid("EEFFA5D3-82E3-46F8-9762-B4053D73F973");

        private readonly Guid _expectedJobHistoryErrorObject = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");
        private readonly Guid _expectedJobHistoryRelationGuid = new Guid("8B747B91-0627-4130-8E53-2931FFC4135F");

        private readonly Guid _expectedNameField = new Guid("84E757CC-9DA2-435D-B288-0C21EC589E66");
        private readonly Guid _expectedSourceUniqueIdField = new Guid("5519435E-EE82-4820-9546-F1AF46121901");
        private readonly Guid _expectedStackTraceField = new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF");
        private readonly Guid _expectedTimestampUtcField = new Guid("B9CBA772-E7C9-493E-B7F8-8D605A6BFE1F");

        private readonly Guid _expectedErrorTypeItem = new Guid("9DDC4914-FEF3-401F-89B7-2967CD76714B");
        private readonly Guid _expectedErrorTypeJob = new Guid("FA8BB625-05E6-4BF7-8573-012146BAF19B");

        private readonly Guid _jobHistoryErrorTypeGuid = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");
        private readonly Guid _jobHistoryRelationGuid = new Guid("8b747b91-0627-4130-8e53-2931ffc4135f");
        private readonly Guid _jobHistoryTypeGuid = new Guid("08f4b1f7-9692-4a08-94ab-b5f3a88b6cc9");
        private readonly Guid _itemLevelErrorGuid = new Guid("9DDC4914-FEF3-401F-89B7-2967CD76714B");
        private readonly Guid _errorTypeGuid = new Guid("EEFFA5D3-82E3-46F8-9762-B4053D73F973");

        private ConfigurationStub _configuration;

        [SetUp]
        public void SetUp()
        {
            _serviceFactoryForUserFake = new Mock<ISourceServiceFactoryForUser>();
            _utcNow = DateTime.UtcNow;
            _dateTimeFake = new Mock<IDateTime>();
            _dateTimeFake.SetupGet(x => x.UtcNow).Returns(_utcNow);
            _objectManagerMock = new Mock<IObjectManager>();
            _serviceFactoryForUserFake.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManagerMock.Object);
            _configuration = new ConfigurationStub();
            _sut = new JobHistoryErrorRepository(_serviceFactoryForUserFake.Object, _configuration, _configuration, _dateTimeFake.Object, new EmptyLogger(), new WrapperForRandom());
            _sut.SecondsBetweenRetriesBase = 0.1;
        }

        [Test]
        public async Task ItShouldCreateJobHistoryError()
        {
            ErrorType errorType = ErrorType.Item;
            const string stackTrace = "Stack trace";
            const string sourceUniqueId = "src unique id";
            const string errorMessage = "Some message.";
            const int errorArtifactId = 10;
            CreateJobHistoryErrorDto createJobHistoryErrorDto = new CreateJobHistoryErrorDto(errorType)
            {
                ErrorMessage = errorMessage,
                SourceUniqueId = sourceUniqueId,
                StackTrace = stackTrace
            };
            MassCreateResult massCreateResult = new MassCreateResult
            {
                Success = true,
                Objects = new List<RelativityObjectRef>
                {
                    new RelativityObjectRef
                    {
                        ArtifactID = errorArtifactId
                    }
                }
            };
            _objectManagerMock.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<MassCreateRequest>())).ReturnsAsync(massCreateResult);

            // act
            int createResult = await _sut.CreateAsync(_TEST_WORKSPACE_ARTIFACT_ID, _TEST_JOB_HISTORY_ARTIFACT_ID, createJobHistoryErrorDto)
                .ConfigureAwait(false);

            // assert
            _objectManagerMock.Verify(x => x.CreateAsync(It.IsAny<int>(), It.Is<MassCreateRequest>(cr => VerifyMassCreateRequest(cr, createJobHistoryErrorDto))));
            createResult.Should().Be(errorArtifactId);
        }

        [Test]
        public void CreateAsyncObjectManagerThrowsExceptionTest()
        {
            // Arrange
            _objectManagerMock.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<MassCreateRequest>())).Throws<NotAuthorizedException>();

            CreateJobHistoryErrorDto expectedCreateJobHistoryErrorDto = CreateJobHistoryErrorDto(_TEST_ERROR_TYPE_ITEM);

            // Act & Assert
            Assert.ThrowsAsync<NotAuthorizedException>(async () =>
                await _sut.MassCreateAsync(_TEST_WORKSPACE_ARTIFACT_ID, _TEST_JOB_HISTORY_ARTIFACT_ID, new List<CreateJobHistoryErrorDto>()
            {
                expectedCreateJobHistoryErrorDto
            }).ConfigureAwait(false));

            _objectManagerMock.Verify(x => x.CreateAsync(
                It.Is<int>(y => y == _TEST_WORKSPACE_ARTIFACT_ID),
                It.Is<MassCreateRequest>(y => VerifyMassCreateRequest(y, expectedCreateJobHistoryErrorDto))));
        }

        [TestCase(0, 5, 0)]
        [TestCase(4, 5, 1)]
        [TestCase(5, 5, 3)]
        [TestCase(6, 5, 3)]
        public async Task CreateMassAsync_ShouldCreateErrorsInChunks_WhenEntityTooLargeExceptionOccurs(int itemLevelErrorsCount, int lessThanItemsPerRequest, int expectedCalls)
        {
            // Arrange
            IList<CreateJobHistoryErrorDto> itemLevelErrors = Enumerable.Repeat(CreateJobHistoryErrorDto(ErrorType.Item), itemLevelErrorsCount).ToList();

            SetupMassCreate(lessThanItemsPerRequest);

            // Act
            IEnumerable<int> result = await _sut.MassCreateAsync(_TEST_WORKSPACE_ARTIFACT_ID, _TEST_JOB_HISTORY_ARTIFACT_ID, itemLevelErrors).ConfigureAwait(false);

            // Assert
            result.Should().HaveCount(itemLevelErrorsCount);

            _objectManagerMock.Verify(x => x.CreateAsync(
                It.Is<int>(y => y == _TEST_WORKSPACE_ARTIFACT_ID),
                It.IsAny<MassCreateRequest>()), Times.Exactly(expectedCalls));
        }

        [Test]
        public void CreateMassAsync_ShouldRetry_WhenPRimaryKeyViolationOccurs()
        {
            // Arrange
            const int itemLevelErrorsCount = 5;
            const int expectedCalls = 4; // 1 call, 3 retries
            IList<CreateJobHistoryErrorDto> itemLevelErrors = Enumerable.Repeat(CreateJobHistoryErrorDto(ErrorType.Item), itemLevelErrorsCount).ToList();

            _objectManagerMock.Setup(x => x.CreateAsync(_TEST_WORKSPACE_ARTIFACT_ID, It.IsAny<MassCreateRequest>()))
                .Throws(new ServiceException(_VIOLATION_OF_PRIMARY_KEY_EXCPETION));

            // Act
            var action = new Func<Task<IEnumerable<int>>>(() => _sut.MassCreateAsync(_TEST_WORKSPACE_ARTIFACT_ID, _TEST_JOB_HISTORY_ARTIFACT_ID, itemLevelErrors));

            // Assert
            action.Should().Throw<ServiceException>();

            _objectManagerMock.Verify(x => x.CreateAsync(
                It.Is<int>(y => y == _TEST_WORKSPACE_ARTIFACT_ID),
                It.IsAny<MassCreateRequest>()), Times.Exactly(expectedCalls));
        }

        [Test]
        public void CreateMassAsync_ShouldThrowException_WhenEvenSingleItemIsTooLargeForObjectManager()
        {
            // Arrange
            const int itemLevelErrorsCount = 3;
            IList<CreateJobHistoryErrorDto> itemLevelErrors = Enumerable.Repeat(CreateJobHistoryErrorDto(ErrorType.Item), itemLevelErrorsCount).ToList();

            const int lessThanItemsPerRequest = 1;
            SetupMassCreate(lessThanItemsPerRequest);

            // Act & Assert
            Func<Task> action = async () => await _sut.MassCreateAsync(_TEST_WORKSPACE_ARTIFACT_ID, _TEST_JOB_HISTORY_ARTIFACT_ID, itemLevelErrors).ConfigureAwait(false);

            action.Should().ThrowAsync<SyncException>();
        }

        [Test]
        public async Task GetLastJobErrorAsyncGoldFlowTest()
        {
            // Arrange
            const int testJobHistoryErrorArtifactId = 108557;
            const string testErrorMessage = "Job failed. See inner exception for more details.";
            string testErrorStack = string.Empty;

            _objectManagerMock.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<ReadRequest>())).ReturnsAsync(new ReadResult
            {
                Object = new RelativityObject { ArtifactID = _TEST_ERROR_TYPE_JOB_ARTIFACT_ID }
            });

            _objectManagerMock.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(() => new QueryResult
            {
                TotalCount = 1,
                Objects = new List<RelativityObject> { GetQueryJobHistoryErrorResponse(testJobHistoryErrorArtifactId, testErrorMessage, testErrorStack) }
            });

            // Act
            IJobHistoryError actualResult = await _sut.GetLastJobErrorAsync(_TEST_WORKSPACE_ARTIFACT_ID, _TEST_JOB_HISTORY_ARTIFACT_ID).ConfigureAwait(false);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.ArtifactId.Should().Be(testJobHistoryErrorArtifactId);
            actualResult.ErrorStatus.Should().Be(ErrorStatus.New);
            actualResult.ErrorType.Should().Be(ErrorType.Job);
            actualResult.StackTrace.Should().BeEmpty();
            actualResult.ErrorMessage.Should().Be("Job failed. See inner exception for more details.");

            _objectManagerMock.Verify(x => x.ReadAsync(_TEST_WORKSPACE_ARTIFACT_ID, It.Is<ReadRequest>(y => VerifyReadErrorTypeJobRequest(y))), Times.Once);
            _objectManagerMock.Verify(x => x.QueryAsync(_TEST_WORKSPACE_ARTIFACT_ID, It.Is<QueryRequest>(y => VerifyQueryLastJobHistoryErrorRequest(y)), 0, 1), Times.Once);
        }

        [Test]
        public async Task TotalCountEqualZeroTest()
        {
            // Arrange
            const int testJobHistoryErrorArtifactId = 108557;
            const string testErrorMessage = "Job failed. See inner exception for more details.";
            string testErrorStack = string.Empty;

            _objectManagerMock.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<ReadRequest>())).ReturnsAsync(new ReadResult
            {
                Object = new RelativityObject { ArtifactID = _TEST_ERROR_TYPE_JOB_ARTIFACT_ID }
            });

            _objectManagerMock.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(() => new QueryResult
            {
                TotalCount = 0,
                Objects = new List<RelativityObject> { GetQueryJobHistoryErrorResponse(testJobHistoryErrorArtifactId, testErrorMessage, testErrorStack) }
            });

            // Act
            IJobHistoryError actualResult = await _sut.GetLastJobErrorAsync(_TEST_WORKSPACE_ARTIFACT_ID, _TEST_JOB_HISTORY_ARTIFACT_ID).ConfigureAwait(false);

            // Assert
            actualResult.Should().BeNull();
        }

        [Test]
        public void ItShouldThrowExceptionTest()
        {
            // Arrange
            _objectManagerMock.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<ReadRequest>())).Throws<SyncException>();

            // Act & Assert
            Assert.ThrowsAsync<SyncException>(async () => await _sut.GetLastJobErrorAsync(_TEST_WORKSPACE_ARTIFACT_ID, _TEST_JOB_HISTORY_ARTIFACT_ID).ConfigureAwait(false));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SyncExceptionThrowVerification(bool massCreateResultSuccess)
        {
            // Arrange
            const int itemLevelErrorsCount = 3;
            IList<CreateJobHistoryErrorDto> itemLevelErrors = Enumerable.Repeat(CreateJobHistoryErrorDto(ErrorType.Item), itemLevelErrorsCount).ToList();
            _objectManagerMock.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<MassCreateRequest>()))
                .Returns((int workspaceId, MassCreateRequest request) => Task.FromResult(GetResultFrom(massCreateResultSuccess ? null : request, massCreateResultSuccess)));

            // Act
            Func<Task> action = async () => await _sut.MassCreateAsync(_TEST_WORKSPACE_ARTIFACT_ID, _TEST_JOB_HISTORY_ARTIFACT_ID, itemLevelErrors).ConfigureAwait(false);

            // Assert
            action.Should().Throw<SyncException>();
        }

        [Test]
        public async Task MassCreateAsync_ShouldFilterOutItemLevelError_WhenLogItemLevelErrorsIsFalse()
        {
            // Arrange
            _configuration.LogItemLevelErrors = false;

            CreateJobHistoryErrorDto itemLevelErrorDto = CreateJobHistoryErrorDto(_TEST_ERROR_TYPE_ITEM);
            CreateJobHistoryErrorDto jobLevelErrorDto = CreateJobHistoryErrorDto(ErrorType.Job);

            _objectManagerMock.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<MassCreateRequest>()))
                .Returns((int workspaceId, MassCreateRequest request) => Task.FromResult(GetResultFrom(request, true)));

            // Act
            await _sut.MassCreateAsync(_TEST_WORKSPACE_ARTIFACT_ID, _TEST_JOB_HISTORY_ARTIFACT_ID, new List<CreateJobHistoryErrorDto>()
                {
                    itemLevelErrorDto, jobLevelErrorDto
                }).ConfigureAwait(false);

            // Assert
            _objectManagerMock.Verify(x => x.CreateAsync(
                It.Is<int>(y => y == _TEST_WORKSPACE_ARTIFACT_ID),
                It.Is<MassCreateRequest>(y => y.ValueLists.Count == 1 && VerifyMassCreateRequest(y, jobLevelErrorDto))));
        }

        [Test]
        public async Task HasErrorsAsync_ShouldReturnTrue_WhenItemLevelErrorsExists_AndFailedItemsCountZero()
        {
            // Arrange
            _objectManagerMock
                .Setup(x => x.QueryAsync(_TEST_WORKSPACE_ARTIFACT_ID, It.Is<QueryRequest>(req => req.ObjectType.Guid == _jobHistoryErrorTypeGuid &&
                                                                                                 req.Condition == $"('{_jobHistoryRelationGuid}' IN OBJECT [{_TEST_JOB_HISTORY_ARTIFACT_ID}]) AND ('{_errorTypeGuid}' == CHOICE {_itemLevelErrorGuid})"), 0, 1))
                .ReturnsAsync(new QueryResult()
                {
                    ResultCount = 1
                });

            _objectManagerMock
                .Setup(x => x.QueryAsync(_TEST_WORKSPACE_ARTIFACT_ID, It.Is<QueryRequest>(req => req.ObjectType.Guid == _jobHistoryTypeGuid &&
                                                                                                 req.Condition == $"'Artifact ID' == '{_TEST_JOB_HISTORY_ARTIFACT_ID}'"), 0, 1))
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
                                    Value = 0
                                }
                            }
                        }
                    }
                });

            // Act
            bool hasErrors = await _sut.HasErrorsAsync(_TEST_WORKSPACE_ARTIFACT_ID, _TEST_JOB_HISTORY_ARTIFACT_ID);

            // Assert
            hasErrors.Should().BeTrue();
        }

        [Test]
        public async Task HasErrorsAsync_ShouldReturnTrue_WhenNoItemLevelErrors_AndFailedItemsCountIsOne()
        {
            // Arrange
            _objectManagerMock
                .Setup(x => x.QueryAsync(_TEST_WORKSPACE_ARTIFACT_ID, It.Is<QueryRequest>(req => req.ObjectType.Guid == _jobHistoryErrorTypeGuid &&
                                                                                                 req.Condition == $"('{_jobHistoryRelationGuid}' IN OBJECT [{_TEST_JOB_HISTORY_ARTIFACT_ID}]) AND ('{_errorTypeGuid}' == CHOICE {_itemLevelErrorGuid})"), 0, 1))
                .ReturnsAsync(new QueryResult()
                {
                    Objects = new List<RelativityObject>()
                });

            _objectManagerMock
                .Setup(x => x.QueryAsync(_TEST_WORKSPACE_ARTIFACT_ID, It.Is<QueryRequest>(req => req.ObjectType.Guid == _jobHistoryTypeGuid &&
                                                                                                 req.Condition == $"'Artifact ID' == '{_TEST_JOB_HISTORY_ARTIFACT_ID}'"), 0, 1))
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
                                    Value = 1
                                }
                            }
                        }
                    }
                });

            // Act
            bool hasErrors = await _sut.HasErrorsAsync(_TEST_WORKSPACE_ARTIFACT_ID, _TEST_JOB_HISTORY_ARTIFACT_ID);

            // Assert
            hasErrors.Should().BeTrue();
        }

        [Test]
        public async Task HasErrorsAsync_ShouldReturnFalse_WhenNoItemLevelErrors_AndFailedItemsCountIsZero()
        {
            // Arrange
            _objectManagerMock
                .Setup(x => x.QueryAsync(_TEST_WORKSPACE_ARTIFACT_ID, It.Is<QueryRequest>(req => req.ObjectType.Guid == _jobHistoryErrorTypeGuid &&
                                                                                                 req.Condition == $"('{_jobHistoryRelationGuid}' IN OBJECT [{_TEST_JOB_HISTORY_ARTIFACT_ID}]) AND ('{_errorTypeGuid}' == CHOICE {_itemLevelErrorGuid})"), 0, 1))
                .ReturnsAsync(new QueryResult()
                {
                    Objects = new List<RelativityObject>()
                });

            _objectManagerMock
                .Setup(x => x.QueryAsync(_TEST_WORKSPACE_ARTIFACT_ID, It.Is<QueryRequest>(req => req.ObjectType.Guid == _jobHistoryTypeGuid &&
                                                                                                 req.Condition == $"'Artifact ID' == '{_TEST_JOB_HISTORY_ARTIFACT_ID}'"), 0, 1))
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
                                    Value = 0
                                }
                            }
                        }
                    }
                });

            // Act
            bool hasErrors = await _sut.HasErrorsAsync(_TEST_WORKSPACE_ARTIFACT_ID, _TEST_JOB_HISTORY_ARTIFACT_ID);

            // Assert
            hasErrors.Should().BeFalse();
        }

        private bool VerifyMassCreateRequest(MassCreateRequest request, CreateJobHistoryErrorDto dto)
        {
            Guid GetErrorType(CreateJobHistoryErrorDto errorDto)
            {
                switch (errorDto.ErrorType)
                {
                    case ErrorType.Item:
                        return _expectedErrorTypeItem;
                    case ErrorType.Job:
                        return _expectedErrorTypeJob;

                    default:
                        return _expectedErrorTypeItem;
                }
            }

#pragma warning disable RG2009 // Hardcoded Numeric Value
            IReadOnlyList<object> valueList = request.ValueLists[0];

            return request.Fields[0].Guid == _expectedErrorMessageField &&
                request.Fields[1].Guid == _expectedErrorStatusField &&
                request.Fields[2].Guid == _expectedErrorTypeField &&
                request.Fields[3].Guid == _expectedNameField &&
                request.Fields[4].Guid == _expectedSourceUniqueIdField &&
                request.Fields[5].Guid == _expectedStackTraceField &&
                request.Fields[6].Guid == _expectedTimestampUtcField &&

                (string)valueList[0] == dto.ErrorMessage &&
                ((ChoiceRef)valueList[1]).Guid == _expectedErrorStatusNew &&
                ((ChoiceRef)valueList[2]).Guid == GetErrorType(dto) &&
                (string)valueList[3] != Guid.Empty.ToString() &&
                (string)valueList[4] == dto.SourceUniqueId &&
                (string)valueList[5] == dto.StackTrace &&
                (DateTime)valueList[6] == _utcNow;
#pragma warning restore RG2009 // Hardcoded Numeric Value
        }

        private CreateJobHistoryErrorDto CreateJobHistoryErrorDto(ErrorType expectedErrorType)
        {
            var createJobHistoryErrorDto = new CreateJobHistoryErrorDto(expectedErrorType)
            {
                ErrorMessage = _TEST_ERROR_MESSAGE,
                SourceUniqueId = _TEST_SOURCE_UNIQUE_ID,
                StackTrace = _TEST_STACK_TRACE
            };
            return createJobHistoryErrorDto;
        }

        private RelativityObject GetQueryJobHistoryErrorResponse(int testErrorArtifactId, string testErrorMessage, string testErrorStack)
        {
            var relativityObject = new RelativityObject
            {
                ArtifactID = testErrorArtifactId,
                FieldValues = new List<FieldValuePair>
                {
                    new FieldValuePair
                    {
                        Field = new Field { Guids = new List<Guid> { _expectedErrorMessageField } },
                        Value = testErrorMessage
                    },
                    new FieldValuePair
                    {
                        Field = new Field { Guids = new List<Guid> { _expectedErrorStatusField } },
                        Value = new Choice { Name = ErrorStatus.New.GetDescription() }
                    },
                    new FieldValuePair
                    {
                        Field = new Field { Guids = new List<Guid> { _expectedErrorTypeField } },
                        Value = new Choice { Name = ErrorType.Job.ToString() }
                    },
                    new FieldValuePair
                    {
                        Field = new Field { Guids = new List<Guid> { _expectedNameField } },
                        Value = Guid.NewGuid().ToString()
                    },
                    new FieldValuePair
                    {
                        Field = new Field { Guids = new List<Guid> { _expectedSourceUniqueIdField } },
                        Value = string.Empty
                    },
                    new FieldValuePair
                    {
                        Field = new Field { Guids = new List<Guid> { _expectedStackTraceField } },
                        Value = testErrorStack
                    },
                    new FieldValuePair
                    {
                        Field = new Field { Guids = new List<Guid> { _expectedTimestampUtcField } },
                        Value = DateTime.UtcNow
                    }
                }
            };
            return relativityObject;
        }

        private bool VerifyReadErrorTypeJobRequest(ReadRequest actualRequest)
        {
            actualRequest.Object.Guid.Should().Be(_expectedErrorTypeJob);
            return true;
        }

        private bool VerifyQueryLastJobHistoryErrorRequest(QueryRequest actualRequest)
        {
            const int expectedNumberOfFieldsInRequest = 7;
            actualRequest.ObjectType.Guid.Should().Be(_expectedJobHistoryErrorObject);
            actualRequest.Condition.Should().Be($"'{_expectedJobHistoryRelationGuid}' == OBJECT {_TEST_JOB_HISTORY_ARTIFACT_ID} AND '{_expectedErrorTypeField}' == CHOICE {_TEST_ERROR_TYPE_JOB_ARTIFACT_ID}");
            actualRequest.Sorts.First().Direction.Should().Be(SortEnum.Descending);
            actualRequest.Sorts.First().FieldIdentifier.Guid.Should().Be(_expectedTimestampUtcField);
            actualRequest.Fields.Should().HaveCount(expectedNumberOfFieldsInRequest);
            return true;
        }

        private void SetupMassCreate(int lessThanItemsPerRequest)
        {
            _objectManagerMock.Setup(x => x.CreateAsync(_TEST_WORKSPACE_ARTIFACT_ID, It.Is<MassCreateRequest>(req => req.ValueLists.Count >= lessThanItemsPerRequest)))
                .Throws(new ServiceException(_ENTITY_TOO_LARGE_EXCEPTION));
            _objectManagerMock.Setup(x => x.CreateAsync(_TEST_WORKSPACE_ARTIFACT_ID, It.Is<MassCreateRequest>(req => req.ValueLists.Count < lessThanItemsPerRequest)))
                .Returns((int workspaceId, MassCreateRequest request) => Task.FromResult(GetResultFrom(request)));
        }

        private static MassCreateResult GetResultFrom(MassCreateRequest request, bool success = true)
        {
            ReadOnlyCollection<RelativityObjectRef> objects = new ReadOnlyCollection<RelativityObjectRef>(new List<RelativityObjectRef>());
            if (request != null)
            {
                objects = request.ValueLists.Select((value, index) => new RelativityObjectRef { ArtifactID = index })
                    .ToList()
                    .AsReadOnly();
            }

            return new MassCreateResult
            {
                Success = success,
                Objects = objects
            };
        }
    }
}
