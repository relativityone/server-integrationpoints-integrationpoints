using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common.Attributes;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
    [TestFixture]
    public class ImageFieldMappingsValidatorTests
    {
        private CancellationToken _cancellationToken;

        private Mock<IObjectManager> _objectManager;

        private Mock<IValidationConfiguration> _validationConfiguration;

        private JSONSerializer _jsonSerializer;
        private List<FieldMap> _fieldMappings;

        private ImageFieldMappingValidator _sut;

        private const int _TEST_DEST_WORKSPACE_ARTIFACT_ID = 202567;
        private const int _TEST_SOURCE_WORKSPACE_ARTIFACT_ID = 101234;
        private const int _TEST_DEST_FIELD_ARTIFACT_ID = 1003668;
        private const int _TEST_SOURCE_FIELD_ARTIFACT_ID = 1003667;
        private const string _TEST_DEST_FIELD_NAME = "Control Number";
        private const string _TEST_SOURCE_FIELD_NAME = "Control Number";

        private const string _TEST_FIELDS_MAP = @"[{
            ""sourceField"": {
                ""displayName"": ""Control Number"",
                ""isIdentifier"": true,
                ""fieldIdentifier"": ""1003667"",
                ""isRequired"": true
            },
            ""destinationField"": {
                ""displayName"": ""Control Number"",
                ""isIdentifier"": true,
                ""fieldIdentifier"": ""1003668"",
                ""isRequired"": true
            },
            ""fieldMapType"": ""Identifier""
        }]";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _jsonSerializer = new JSONSerializer();
            _fieldMappings = _jsonSerializer.Deserialize<List<FieldMap>>(_TEST_FIELDS_MAP);
        }

        [SetUp]
        public void SetUp()
        {
            _cancellationToken = CancellationToken.None;

            Mock<IDestinationServiceFactoryForUser> destinationServiceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();
            Mock<ISourceServiceFactoryForUser> sourceServiceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
            _objectManager = new Mock<IObjectManager>();

            destinationServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
            sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

            _validationConfiguration = new Mock<IValidationConfiguration>();
            _validationConfiguration.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(_TEST_DEST_WORKSPACE_ARTIFACT_ID).Verifiable();
            _validationConfiguration.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID).Verifiable();
            _validationConfiguration.Setup(x => x.GetFieldMappings()).Returns(_fieldMappings).Verifiable();
            _validationConfiguration.SetupGet(x => x.ImportOverwriteMode).Returns(ImportOverwriteMode.AppendOverlay);
            _validationConfiguration.SetupGet(x => x.FieldOverlayBehavior).Returns(FieldOverlayBehavior.UseFieldSettings);
            _validationConfiguration.SetupGet(x => x.RdoArtifactTypeId).Returns((int)ArtifactType.Document);
            _validationConfiguration.SetupGet(x => x.DestinationRdoArtifactTypeId).Returns((int)ArtifactType.Document);

            SetUpObjectManagerQuery(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID, _TEST_SOURCE_FIELD_ARTIFACT_ID, _TEST_SOURCE_FIELD_NAME);
            SetUpObjectManagerQuery(_TEST_DEST_WORKSPACE_ARTIFACT_ID, _TEST_DEST_FIELD_ARTIFACT_ID, _TEST_DEST_FIELD_NAME);

            _sut = new ImageFieldMappingValidator(sourceServiceFactoryForUser.Object, destinationServiceFactoryForUser.Object, new EmptyLogger());
        }

        [Test]
        public async Task ValidateAsync_ShouldPassGoldFlow()
        {
            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(actualResult.IsValid);

            VerifyObjectManagerQueryRequest();
            Mock.Verify(_validationConfiguration);
        }

        [Test]
        public async Task ValidateAsync_ShouldFailOnMoreThanOneMapping()
        {
            // Arrange
            var additionalFieldsMapped = _fieldMappings.Concat(new FieldMap[]
            {
                new FieldMap
                {
                    DestinationField = new FieldEntry
                    {
                        DisplayName = "Test",
                        FieldIdentifier = 5,
                    },
                    SourceField = new FieldEntry
                    {
                        DisplayName = "Test",
                        FieldIdentifier = 5,
                    },
                    FieldMapType = FieldMapType.None
                }
            }).ToList();

            _validationConfiguration.Setup(x => x.GetFieldMappings()).Returns(additionalFieldsMapped).Verifiable();

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            actualResult.IsValid.Should().BeFalse();
            actualResult.Messages.First().ShortMessage.Should().Be("Only unique identifier must be mapped.");

            Mock.Verify(_validationConfiguration);
        }

        [Test]
        public void ValidateAsync_ShouldDeserializeThrowsException_WhenGetFieldMappingsFails()
        {
            // Arrange
            _validationConfiguration.Setup(x => x.GetFieldMappings()).Throws<InvalidOperationException>().Verifiable();

            // Act
            Func<Task<ValidationResult>> func = async () => await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            func.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public async Task ValidateAsync_ShouldHandleDestinationFieldMissing()
        {
            // Arrange
            SetUpObjectManagerQuery(_TEST_DEST_WORKSPACE_ARTIFACT_ID, 0, null);

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(actualResult.IsValid);
            Assert.IsNotEmpty(actualResult.Messages);
            ValidationMessage actualMessage = actualResult.Messages.First();
            actualMessage.ErrorCode.Should().Be("20.005");
            actualMessage.ShortMessage.Should().StartWith("Destination field(s) mapped");

            VerifyObjectManagerQueryRequest();
            Mock.Verify(_validationConfiguration);
        }

        [Test]
        public async Task ValidateAsync_ShouldHAndleSourceFieldMissing()
        {
            // Arrange
            SetUpObjectManagerQuery(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID, 0, null);

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(actualResult.IsValid);
            Assert.IsNotEmpty(actualResult.Messages);
            actualResult.Messages.First().ShortMessage.Should().StartWith("Source field(s) mapped");

            VerifyObjectManagerQueryRequest();
            Mock.Verify(_validationConfiguration);
        }

        [Test]
        public async Task ValidateAsync_ShouldReturnInvalidMessage_WhenFieldInSourceWorkspaceHasBeenRenamed()
        {
            // Arrange
            SetUpObjectManagerQuery(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID, _TEST_SOURCE_FIELD_ARTIFACT_ID, "Control Number - RENAMED");

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(actualResult.IsValid);
            Assert.IsNotEmpty(actualResult.Messages);
            actualResult.Messages.First().ShortMessage
                .Should().StartWith("Source field(s) mapped")
                .And.Contain(_TEST_SOURCE_FIELD_NAME);

            VerifyObjectManagerQueryRequest();
            Mock.Verify(_validationConfiguration);
        }

        [Test]
        public async Task ValidateAsync_ShouldReturnInvalidMessage_WhenFieldInDestinationWorkspaceHasBeenRenamed()
        {
            // Arrange
            SetUpObjectManagerQuery(_TEST_DEST_WORKSPACE_ARTIFACT_ID, _TEST_DEST_FIELD_ARTIFACT_ID, "Control Number - RENAMED");

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(actualResult.IsValid);
            Assert.IsNotEmpty(actualResult.Messages);

            ValidationMessage actualMessage = actualResult.Messages.First();
            actualMessage.ErrorCode.Should().Be("20.005");
            actualMessage.ShortMessage
                .Should().StartWith("Destination field(s) mapped")
                .And.Contain(_TEST_DEST_FIELD_NAME);

            VerifyObjectManagerQueryRequest();
            Mock.Verify(_validationConfiguration);
        }

        [Test]
        public void ValidateAsync_ShouldThrowException_WhenObjectManagerFails()
        {
            // Arrange
            _objectManager.Setup(x => x.QueryAsync(It.Is<int>(y => y == _TEST_SOURCE_WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).Throws<InvalidOperationException>();

            // Act
            Func<Task<ValidationResult>> func = async () => await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            func.Should().Throw<InvalidOperationException>();
        }

        [TestCaseSource(nameof(_invalidUniqueIdentifiersFieldMap))]
        public async Task ValidateAsync_ShouldHandleUniqueIdentifierInvalid(string testInvalidFieldMap, string expectedErrorMessage)
        {
            // Arrange
            List<FieldMap> fieldMap = _jsonSerializer.Deserialize<List<FieldMap>>(testInvalidFieldMap);
            _validationConfiguration.Setup(x => x.GetFieldMappings()).Returns(fieldMap).Verifiable();

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(actualResult.IsValid);
            Assert.IsNotEmpty(actualResult.Messages);
            actualResult.Messages.First().ShortMessage.Should().Be(expectedErrorMessage);

            VerifyObjectManagerQueryRequest();
            Mock.Verify(_validationConfiguration);
        }

        [Test]
        public async Task ValidateAsync_ShouldHandleFieldOverlayBehaviorInvalid()
        {
            // Arrange
            _validationConfiguration.SetupGet(x => x.ImportOverwriteMode).Returns(ImportOverwriteMode.AppendOnly);
            _validationConfiguration.SetupGet(x => x.FieldOverlayBehavior).Returns(FieldOverlayBehavior.ReplaceValues);

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            Assert.False(actualResult.IsValid);
            Assert.IsNotEmpty(actualResult.Messages);
            actualResult.Messages.First().ShortMessage.Should().Contain("overlay behavior");

            VerifyObjectManagerQueryRequest();
            Mock.Verify(_validationConfiguration);
        }

        [TestCase(typeof(SyncDocumentRunPipeline), false)]
        [TestCase(typeof(SyncDocumentRetryPipeline), false)]
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

        private void SetUpObjectManagerQuery(int testWorkspaceArtifactId, int testFieldArtifactId, string testFieldName)
        {
            var queryResult = new QueryResult
            {
                Objects = new List<RelativityObject>
                {
                    new RelativityObject
                    {
                        ArtifactID = testFieldArtifactId,
                        Name = testFieldName
                    }
                }
            };
            _objectManager.Setup(x => x.QueryAsync(It.Is<int>(y => y == testWorkspaceArtifactId), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult);
        }

        private void VerifyObjectManagerQueryRequest()
        {
            const int expectedFieldArtifactTypeId = (int)ArtifactType.Document;
            const string expectedObjectTypeName = "Field";

            string expectedDestQueryCondition = $"(('FieldArtifactTypeID' == {expectedFieldArtifactTypeId} AND 'ArtifactID' IN [{_TEST_DEST_FIELD_ARTIFACT_ID}]))";
            string expectedSourceQueryCondition = $"(('FieldArtifactTypeID' == {expectedFieldArtifactTypeId} AND 'ArtifactID' IN [{_TEST_SOURCE_FIELD_ARTIFACT_ID}]))";

            _objectManager.Verify(x => x.QueryAsync(
                It.Is<int>(y => y == _TEST_DEST_WORKSPACE_ARTIFACT_ID),
                It.Is<QueryRequest>(y => y.ObjectType.Name == expectedObjectTypeName && y.Condition == expectedDestQueryCondition && y.IncludeNameInQueryResult == true),
                It.Is<int>(y => y == 0),
                It.Is<int>(y => y == 1)));

            _objectManager.Verify(x => x.QueryAsync(
                It.Is<int>(y => y == _TEST_SOURCE_WORKSPACE_ARTIFACT_ID),
                It.Is<QueryRequest>(y => y.ObjectType.Name == expectedObjectTypeName && y.Condition == expectedSourceQueryCondition && y.IncludeNameInQueryResult == true),
                It.Is<int>(y => y == 0),
                It.Is<int>(y => y == 1)));
        }

        private static IEnumerable<TestCaseData> _invalidUniqueIdentifiersFieldMap => new[]
        {
            new TestCaseData(
                @"[{
                ""sourceField"": {
                    ""displayName"": ""Control Number"",
                    ""isIdentifier"": false,
                    ""fieldIdentifier"": ""1003667"",
                    ""isRequired"": true
                },
                ""destinationField"": {
                    ""displayName"": ""Control Number"",
                    ""isIdentifier"": true,
                    ""fieldIdentifier"": ""1003668"",
                    ""isRequired"": true
                },
                ""fieldMapType"": ""Identifier""
            }]", "The unique identifier must be mapped.").SetName($"{nameof(ValidateAsync_ShouldHandleUniqueIdentifierInvalid)}_SourceInvalid"),
            new TestCaseData(
                @"[{
                ""sourceField"": {
                    ""displayName"": ""Control Number"",
                    ""isIdentifier"": true,
                    ""fieldIdentifier"": ""1003667"",
                    ""isRequired"": true
                },
                ""destinationField"": {
                    ""displayName"": ""Control Number"",
                    ""isIdentifier"": false,
                    ""fieldIdentifier"": ""1003668"",
                    ""isRequired"": true
                },
                ""fieldMapType"": ""Identifier""
            }]", "Identifier must be mapped with another identifier.").SetName($"{nameof(ValidateAsync_ShouldHandleUniqueIdentifierInvalid)}_DestinationInvalid"),
        };
    }
}
