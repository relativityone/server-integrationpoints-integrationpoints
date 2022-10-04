using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Tests.Common.Attributes;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
    [TestFixture]
    public class FolderStructureBehaviorValidatorTests
    {
        private CancellationToken _cancellationToken;

        private Mock<ISourceServiceFactoryForUser> _sourceServiceFactoryForUser;
        private Mock<IObjectManager> _objectManager;
        private Mock<IValidationConfiguration> _validationConfiguration;

        private FolderStructureBehaviorValidator _sut;

        private const int _DOCUMENT_ARTIFACT_TYPE_ID = 10;
        private const string _TEST_FOLDER_NAME = "folder name";
        private const int _TEST_WORKSPACE_ARTIFACT_ID = 101202;
        private const string _EXPECTED_QUERY_FIELD_TYPE = "Field Type";

        [SetUp]
        public void SetUp()
        {
            _cancellationToken = CancellationToken.None;

            _sourceServiceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
            _objectManager = new Mock<IObjectManager>();
            _validationConfiguration = new Mock<IValidationConfiguration>();

            _validationConfiguration.Setup(x => x.GetFolderPathSourceFieldName()).Returns(_TEST_FOLDER_NAME);
            _validationConfiguration.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(_TEST_WORKSPACE_ARTIFACT_ID);
            _validationConfiguration.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.ReadFromField);

            _sut = new FolderStructureBehaviorValidator(_sourceServiceFactoryForUser.Object, new EmptyLogger());
        }

        [Test]
        public async Task ValidateAsync_ShouldPAssGoldFlow()
        {
            // Arrange
            _sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object).Verifiable();

            QueryResult queryResult = BuildQueryResult("Long Text");
            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(queryResult);

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            actualResult.IsValid.Should().BeTrue();

            VerifyObjectManagerQueryRequest();

            Mock.VerifyAll(_sourceServiceFactoryForUser, _objectManager);
            _objectManager.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public async Task ValidateAsync_ShouldHandleInvalidFieldTypeResult()
        {
            // Arrange
            _sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object).Verifiable();

            QueryResult queryResult = BuildQueryResult("Invalid Field Type Here");
            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(queryResult);

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            actualResult.IsValid.Should().BeFalse();
            actualResult.Messages.Should().HaveCount(1);

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
            actualResult.IsValid.Should().BeFalse();
            actualResult.Messages.Should().HaveCount(1);

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
        public void ValidateAsync_ShouldHandle_QueryAsync_ThrowsException()
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

        [Test]
        public void ValidateAsync_ShouldHandle_ObjectManager_ReturnsMoreThanOneResult()
        {
            // Arrange
            QueryResult queryResult = BuildQueryResult("Different Field Value", _TEST_FOLDER_NAME);
            _sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object).Verifiable();

            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                    .ReturnsAsync(queryResult);

            // Act
            Func<Task<ValidationResult>> func = async () => await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            func.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public async Task ValidateAsync_ShouldNotValidate_WhenBehaviorIsSetToNone()
        {
            // Arrange
            _validationConfiguration.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.None);

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            _sourceServiceFactoryForUser.Verify(x => x.CreateProxyAsync<IObjectManager>(), Times.Never);
            actualResult.IsValid.Should().Be(true);
        }

        [Test]
        public async Task ValidateAsync_ShouldNotValidate_WhenBehaviorIsSetToRetainFromSourceWorkspace()
        {
            // Arrange
            _validationConfiguration.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure);

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            _sourceServiceFactoryForUser.Verify(x => x.CreateProxyAsync<IObjectManager>(), Times.Never);
            actualResult.IsValid.Should().Be(true);
        }

        [TestCase(typeof(IAPI2_SyncDocumentRunPipeline), false)]
        [TestCase(typeof(SyncDocumentRunPipeline), true)]
        [TestCase(typeof(SyncDocumentRetryPipeline), true)]
        [TestCase(typeof(SyncImageRunPipeline), false)]
        [TestCase(typeof(SyncImageRetryPipeline), false)]
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

        private QueryResult BuildQueryResult(params string[] testFieldValues)
        {
            List<FieldValuePair> fieldValues = BuildResultFieldValues(testFieldValues);

            var queryResult = new QueryResult
            {
                Objects = new List<RelativityObject>
                {
                    new RelativityObject
                    {
                        FieldValues = fieldValues
                    }
                }
            };
            return queryResult;
        }

        private static List<FieldValuePair> BuildResultFieldValues(IEnumerable<string> testFieldValues)
        {
            var field = new Field
            {
                Name = _EXPECTED_QUERY_FIELD_TYPE
            };
            List<FieldValuePair> fieldValues = testFieldValues.Select(value => new FieldValuePair
            {
                Field = field,
                Value = value
            }).ToList();
            return fieldValues;
        }

        private void VerifyObjectManagerQueryRequest()
        {
            string expectedQueryCondition = $"(('FieldArtifactTypeID' == {_DOCUMENT_ARTIFACT_TYPE_ID} AND 'Name' == '{_TEST_FOLDER_NAME}'))";

            _objectManager.Verify(x => x.QueryAsync(
                It.Is<int>(y => y == _TEST_WORKSPACE_ARTIFACT_ID),
                It.Is<QueryRequest>(y => y.ObjectType.Name == "Field" && y.Condition == expectedQueryCondition && y.Fields.First().Name == _EXPECTED_QUERY_FIELD_TYPE),
                It.Is<int>(y => y == 0),
                It.Is<int>(y => y == 1)));
        }
    }
}
