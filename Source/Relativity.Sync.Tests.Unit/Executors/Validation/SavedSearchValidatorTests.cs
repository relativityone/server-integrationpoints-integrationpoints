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
    public class SavedSearchValidatorTests
    {
        private CancellationToken _cancellationToken;

        private Mock<ISourceServiceFactoryForUser> _sourceServiceFactoryForUser;
        private Mock<IAPILog> _syncLog;
        private Mock<IObjectManager> _objectManager;
        private Mock<IValidationConfiguration> _validationConfiguration;

        private SavedSearchValidator _sut;

        private const int _TEST_SAVED_SEARCH_ARTIFACT_ID = 101345;
        private const int _TEST_WORKSPACE_ARTIFACT_ID = 101202;
        private const string _EXPECTED_QUERY_FIELD_TYPE = "Owner";

        [SetUp]
        public void SetUp()
        {
            _cancellationToken = CancellationToken.None;

            _sourceServiceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
            _syncLog = new Mock<IAPILog>();
            _objectManager = new Mock<IObjectManager>();
            _validationConfiguration = new Mock<IValidationConfiguration>();

            _validationConfiguration.SetupGet(x => x.SavedSearchArtifactId).Returns(_TEST_SAVED_SEARCH_ARTIFACT_ID);
            _validationConfiguration.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(_TEST_WORKSPACE_ARTIFACT_ID);

            _sut = new SavedSearchValidator(_sourceServiceFactoryForUser.Object, _syncLog.Object);
        }

        [Test]
        public async Task ValidateAsync_ShouldPassGoldFlow()
        {
            // Arrange
            _sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object).Verifiable();

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
        public async Task ValidateAsync_ShouldHandleInvalidFieldTypeResult()
        {
            // Arrange
            _sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object).Verifiable();

            QueryResult queryResult = BuildQueryResult("World, Hello");
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
        public async Task ValidateAsync_ShouldHandleNoQueryResults()
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

        [TestCase(typeof(IAPI2_SyncDocumentRunPipeline), true)]
        [TestCase(typeof(SyncDocumentRunPipeline), true)]
        [TestCase(typeof(SyncDocumentRetryPipeline), true)]
        [TestCase(typeof(SyncImageRunPipeline), true)]
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
                }
            };
            return queryResult;
        }

        private void VerifyObjectManagerQueryRequest()
        {
            const int searchArtifactTypeId = 15;

            _objectManager.Verify(x => x.QueryAsync(
                It.Is<int>(y => y == _TEST_WORKSPACE_ARTIFACT_ID),
                It.Is<QueryRequest>(y => y.ObjectType.ArtifactTypeID == searchArtifactTypeId && y.Fields.First().Name == _EXPECTED_QUERY_FIELD_TYPE),
                It.Is<int>(y => y == 0),
                It.Is<int>(y => y == 1)));
        }
    }
}
