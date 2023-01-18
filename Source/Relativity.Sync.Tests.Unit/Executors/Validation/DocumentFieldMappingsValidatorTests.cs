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
using Relativity.Sync.Transfer;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
    [TestFixture]
    public class DocumentFieldMappingsValidatorTests
    {
        private CancellationToken _cancellationToken;

        private Mock<IObjectManager> _objectManager;

        private Mock<IValidationConfiguration> _validationConfiguration;

        private Mock<IFieldManager> _fieldManagerMock;

        private JSONSerializer _jsonSerializer;
        private List<FieldMap> _fieldMappings;

        private FieldMappingValidator _sut;

        private const int _TEST_DEST_WORKSPACE_ARTIFACT_ID = 202567;
        private const int _TEST_SOURCE_WORKSPACE_ARTIFACT_ID = 101234;
        private const string _IDENTIFIER_DEST_FIELD_NAME = "Control Number";
        private const string _IDENTIFIER_SOURCE_FIELD_NAME = "Control Number";

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
            },
            {""sourceField"": {
                ""displayName"": ""Test"",
                ""isIdentifier"": false,
                ""fieldIdentifier"": ""1003669"",
            },
            ""destinationField"": {
                ""displayName"": ""Test"",
                ""isIdentifier"": false,
                ""fieldIdentifier"": ""1003670""
            },
            ""fieldMapType"": ""None""
        }
        ]";

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
            _fieldManagerMock = new Mock<IFieldManager>();
            _objectManager = new Mock<IObjectManager>();

            destinationServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
            sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
            _fieldManagerMock.Setup(x => x.GetMappedFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FieldInfoDto>());

            _validationConfiguration = new Mock<IValidationConfiguration>();
            _validationConfiguration.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(_TEST_DEST_WORKSPACE_ARTIFACT_ID).Verifiable();
            _validationConfiguration.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID).Verifiable();
            _validationConfiguration.Setup(x => x.GetFieldMappings()).Returns(_fieldMappings).Verifiable();
            _validationConfiguration.SetupGet(x => x.ImportOverwriteMode).Returns(ImportOverwriteMode.AppendOverlay);
            _validationConfiguration.SetupGet(x => x.FieldOverlayBehavior).Returns(FieldOverlayBehavior.UseFieldSettings);
            _validationConfiguration.SetupGet(x => x.RdoArtifactTypeId).Returns((int)ArtifactType.Document);
            _validationConfiguration.SetupGet(x => x.DestinationRdoArtifactTypeId).Returns((int)ArtifactType.Document);

            SetUpObjectManagerQuery(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID, _fieldMappings.Select(x => x.SourceField));
            SetUpObjectManagerQuery(_TEST_DEST_WORKSPACE_ARTIFACT_ID, _fieldMappings.Select(x => x.DestinationField));

            _sut = new FieldMappingValidator(sourceServiceFactoryForUser.Object, destinationServiceFactoryForUser.Object, _fieldManagerMock.Object, new EmptyLogger());
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
        public void ValidateAsync_ShouldDeserializeThrowsException()
        {
            // Arrange
            _validationConfiguration.Setup(x => x.GetFieldMappings()).Throws<InvalidOperationException>().Verifiable();

            // Act
            Func<Task<ValidationResult>> actualResult = async () => await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            actualResult.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public async Task ValidateAsync_ShouldHandleDestinationFieldMissing()
        {
            // Arrange
            SetUpObjectManagerQuery(_TEST_DEST_WORKSPACE_ARTIFACT_ID, Enumerable.Empty<FieldEntry>());

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
            SetUpObjectManagerQuery(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID, Enumerable.Empty<FieldEntry>());

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
            var newFieldMappings = _jsonSerializer.Deserialize<List<FieldMap>>(_TEST_FIELDS_MAP);
            newFieldMappings[0].SourceField.DisplayName = "Control Number - RENAMED";

            SetUpObjectManagerQuery(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID, newFieldMappings.Select(x => x.SourceField));

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(actualResult.IsValid);
            Assert.IsNotEmpty(actualResult.Messages);
            actualResult.Messages.First().ShortMessage
                .Should().StartWith("Source field(s) mapped")
                .And.Contain(_IDENTIFIER_SOURCE_FIELD_NAME);

            VerifyObjectManagerQueryRequest();
            Mock.Verify(_validationConfiguration);
        }

        [Test]
        public async Task ValidateAsync_ShouldReturnInvalidMessage_WhenFieldInDestinationWorkspaceHasBeenRenamed()
        {
            // Arrange
            var newFieldMappings = _jsonSerializer.Deserialize<List<FieldMap>>(_TEST_FIELDS_MAP);
            newFieldMappings[0].DestinationField.DisplayName = "Control Number - RENAMED";

            SetUpObjectManagerQuery(_TEST_DEST_WORKSPACE_ARTIFACT_ID, newFieldMappings.Select(x => x.DestinationField));

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(actualResult.IsValid);
            Assert.IsNotEmpty(actualResult.Messages);

            ValidationMessage actualMessage = actualResult.Messages.First();
            actualMessage.ErrorCode.Should().Be("20.005");
            actualMessage.ShortMessage
                .Should().StartWith("Destination field(s) mapped")
                .And.Contain(_IDENTIFIER_DEST_FIELD_NAME);

            VerifyObjectManagerQueryRequest();
            Mock.Verify(_validationConfiguration);
        }

        [Test]
        public void ValidateAsync_ShouldThrowException_WhenObjectManagerFails()
        {
            // Arrange
            _objectManager.Setup(x => x.QueryAsync(It.Is<int>(y => y == _TEST_SOURCE_WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).Throws<InvalidOperationException>();

            // Act
            Func<Task<ValidationResult>> actualResult = async () => await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            actualResult.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public async Task ValidateAsync_ShouldHandleUnsupportedFieldType()
        {
            // Arrange
            ValidationMessage expectedMessage = new ValidationMessage("Some mapped fields have unsupported type: 'File'.");
            List<FieldInfoDto> mappedFields = new List<FieldInfoDto>() { new FieldInfoDto(SpecialFieldType.None, "Test", "Test", false, false) { RelativityDataType = RelativityDataType.File } };
            _fieldManagerMock.Setup(x => x.GetMappedFieldsAsync(_cancellationToken)).ReturnsAsync(mappedFields);

            // Act
            ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

            // Assert
            actualResult.IsValid.Should().BeFalse();
            actualResult.Messages.Should().HaveCount(1);
            actualResult.Messages.Single().Should().Be(expectedMessage);
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

            VerifyObjectManagerQueryRequest(fieldMap);
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

        [TestCase(typeof(IAPI2_SyncDocumentRunPipeline), true)]
        [TestCase(typeof(SyncDocumentRunPipeline), true)]
        [TestCase(typeof(SyncDocumentRetryPipeline), true)]
        [TestCase(typeof(SyncImageRunPipeline), false)]
        [TestCase(typeof(SyncImageRetryPipeline), false)]
        [TestCase(typeof(SyncNonDocumentRunPipeline), true)]
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

        private void SetUpObjectManagerQuery(int workspaceArtifactId, IEnumerable<FieldEntry> fieldsAvailableInWorkspace)
        {
            var queryResult = new QueryResult
            {
                Objects = fieldsAvailableInWorkspace.Select(x => new RelativityObject
                { Name = x.DisplayName, ArtifactID = x.FieldIdentifier }).ToList()
            };

            _objectManager.Setup(x => x.QueryAsync(It.Is<int>(y => y == workspaceArtifactId), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult);
        }

        private void VerifyObjectManagerQueryRequest(IEnumerable<FieldMap> mappingToVerify = null)
        {
            const int expectedFieldArtifactTypeId = (int)ArtifactType.Document;
            const string expectedObjectTypeName = "Field";

            mappingToVerify = mappingToVerify ?? _fieldMappings;

            string expectedDestQueryCondition = $"(('FieldArtifactTypeID' == {expectedFieldArtifactTypeId} AND 'ArtifactID' IN [{string.Join(",", mappingToVerify.Select(x => x.DestinationField.FieldIdentifier))}]))";
            string expectedSourceQueryCondition = $"(('FieldArtifactTypeID' == {expectedFieldArtifactTypeId} AND 'ArtifactID' IN [{string.Join(",", mappingToVerify.Select(x => x.SourceField.FieldIdentifier))}]))";

            _objectManager.Verify(x => x.QueryAsync(
                It.Is<int>(y => y == _TEST_DEST_WORKSPACE_ARTIFACT_ID),
                It.Is<QueryRequest>(y => y.ObjectType.Name == expectedObjectTypeName && y.Condition == expectedDestQueryCondition && y.IncludeNameInQueryResult == true),
                It.Is<int>(y => y == 0),
                It.Is<int>(y => y == mappingToVerify.Count())));

            _objectManager.Verify(x => x.QueryAsync(
                It.Is<int>(y => y == _TEST_SOURCE_WORKSPACE_ARTIFACT_ID),
                It.Is<QueryRequest>(y => y.ObjectType.Name == expectedObjectTypeName && y.Condition == expectedSourceQueryCondition && y.IncludeNameInQueryResult == true),
                It.Is<int>(y => y == 0),
                It.Is<int>(y => y == mappingToVerify.Count())));
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
