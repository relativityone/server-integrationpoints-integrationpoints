using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Tests.Common.Attributes;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
    [TestFixture]
    public class RetryJobHistoryValidatorTests
    {
        private CancellationToken _cancellationToken;

        private Mock<ISourceServiceFactoryForUser> _sourceServiceFactoryForUser;
        private Mock<IAPILog> _syncLog;
        private Mock<IObjectManager> _objectManager;
        private Mock<IValidationConfiguration> _validationConfiguration;

        private RetryJobHistoryValidator _sut;

        private const int _TEST_JOB_HISTORY_TO_RETRY_ID = 101345;
        private const int _TEST_WORKSPACE_ARTIFACT_ID = 101202;
        private const string _EXPECTED_QUERY_FIELD_TYPE = "Owner";
        private const string _JOB_HISTORY_GUID = "08F4B1F7-9692-4A08-94AB-B5F3A88B6CC9";

        [SetUp]
        public void SetUp()
        {
            _cancellationToken = CancellationToken.None;

            _sourceServiceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
            _syncLog = new Mock<IAPILog>();
            _objectManager = new Mock<IObjectManager>();
            _validationConfiguration = new Mock<IValidationConfiguration>();

            _validationConfiguration.SetupGet(x => x.JobHistoryObjectTypeGuid).Returns(new Guid(_JOB_HISTORY_GUID));
            _validationConfiguration.SetupGet(x => x.JobHistoryToRetryId).Returns(_TEST_JOB_HISTORY_TO_RETRY_ID);
            _validationConfiguration.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(_TEST_WORKSPACE_ARTIFACT_ID);

            _sut = new RetryJobHistoryValidator(_sourceServiceFactoryForUser.Object, _syncLog.Object);

            _sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object).Verifiable();
        }

        [Test]
        public async Task ValidateAsync_ShouldPassGoldFlow()
        {
            // Arrange
            QueryResult queryResult = BuildQueryResult(string.Empty);
            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(queryResult);

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(actualResult.IsValid);

            VerifyObjectManagerQueryRequest();

            Mock.VerifyAll(_sourceServiceFactoryForUser, _objectManager);
            _objectManager.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public async Task ValidateAsync_Should_ReturnInvalid_When_JobHistoryToRetryIsNull()
        {
            // Arrange
            _validationConfiguration.SetupGet(x => x.JobHistoryToRetryId).Returns((int?)null);

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            actualResult.Messages.First().ShortMessage.Should()
                .Be("JobHistoryToRetry should be set in configuration for this pipeline");
        }

        [Test]
        public async Task ValidateAsync_Should_ReturnInvalid_WhenQueryReturnsNoObject()
        {
            // Arrange
            _sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object).Verifiable();

            QueryResult queryResult = new QueryResult { Objects = new List<RelativityObject>() };
            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(queryResult);

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(actualResult.IsValid);
            Assert.IsNotEmpty(actualResult.Messages);
            Assert.AreEqual(1, actualResult.Messages.Count());

            VerifyObjectManagerQueryRequest();

            Mock.VerifyAll(_sourceServiceFactoryForUser, _objectManager);
            _objectManager.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void ValidateAsync_ShouldThrow_WhenCreateProxyFails()
        {
            // Arrange
            _sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).Throws<InvalidOperationException>().Verifiable();

            // Act
            Func<Task<ValidationResult>> func = async () => await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            func.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void ValidateAsync_ShouldThrow_WhenQueryAsyncFails()
        {
            // Arrange
            _sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object).Verifiable();

            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<InvalidOperationException>();

            // Act
            Func<Task<ValidationResult>> func = async () => await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            func.Should().Throw<InvalidOperationException>();
        }

        [TestCase(typeof(SyncDocumentRunPipeline), false)]
        [TestCase(typeof(SyncDocumentRetryPipeline), true)]
        [TestCase(typeof(SyncImageRunPipeline), false)]
        [TestCase(typeof(SyncImageRetryPipeline), true)]
        [TestCase(typeof(SyncNonDocumentRunPipeline), false)]
        [EnsureAllPipelineTestCase(0)]
        public void ShouldExecute_ShouldReturnCorrectValue(Type pipelineType, bool expectedResult)
        {
            // Arrange
            ISyncPipeline pipelineObject = (ISyncPipeline)Activator.CreateInstance(pipelineType);

            // Act
            bool actualResult = _sut.ShouldValidate(pipelineObject);

            // Assert
            actualResult.Should().Be(expectedResult, $"ShouldValidate should return {expectedResult} for pipeline {pipelineType.Name}");
        }

        private QueryResult BuildQueryResult(string testFieldValue)
        {
            var queryResult = new QueryResult
            {
                Objects = new List<RelativityObject>
                {
                    new RelativityObject
                    {
                        FieldValues = new List<FieldValuePair>
                        {
                            new FieldValuePair
                            {
                                Field = new Field
                                {
                                    Name = _EXPECTED_QUERY_FIELD_TYPE
                                },
                                Value = testFieldValue
                            }
                        }
                    }
                },
                ResultCount = 1
            };
            return queryResult;
        }

        private void VerifyObjectManagerQueryRequest()
        {
            Guid searchArtifactTypeId = new Guid(_JOB_HISTORY_GUID);

            _objectManager.Verify(
                x => x.QueryAsync(
                    It.Is<int>(y => y == _TEST_WORKSPACE_ARTIFACT_ID),
                    It.Is<QueryRequest>(y => y.ObjectType.Guid == searchArtifactTypeId && y.Condition == $"'ArtifactId' == {_TEST_JOB_HISTORY_TO_RETRY_ID}"),
                    It.Is<int>(y => y == 1),
                    It.Is<int>(y => y == 1)));
        }
    }
}
