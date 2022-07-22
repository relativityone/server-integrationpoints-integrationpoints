using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    internal sealed class NativeFileInfoRowValuesBuilderTests
    {
        private const long _SIZE = 100;
        private const int _DOCUMENT_ARTIFACT_ID = 10;
        private const string _LOCATION = "abc";
        private const string _FILENAME = "def";

        private const string _INITIAL_VALUE = "ghj";
        private static readonly Type _INITIAL_VALUE_TYPE = _INITIAL_VALUE.GetType();

        public static IEnumerable<TestCaseData> FieldInfoDtos()
        {
            yield return new TestCaseData(FieldInfoDto.DocumentField("abc", "def", false), _INITIAL_VALUE_TYPE, _INITIAL_VALUE);
            yield return new TestCaseData(FieldInfoDto.NativeFileSizeField(), typeof(long), _SIZE);
            yield return new TestCaseData(FieldInfoDto.NativeFileLocationField(), typeof(string), _LOCATION);
            yield return new TestCaseData(FieldInfoDto.NativeFileFilenameField(), typeof(string), _FILENAME);
        }

        private static IEnumerable<TestCaseData> UnsupportedNonDocumentFieldInfoDtos()
        {
            yield return new TestCaseData(FieldInfoDto.FolderPathFieldFromSourceWorkspaceStructure());
        }

        [SetUp]
        public void SetUp()
        {

        }

        [TestCaseSource(nameof(FieldInfoDtos))]
        public void BuildRowValue_ShouldReturnInitialValue_WhenFieldInfoDtoIsDocumentField(FieldInfoDto fieldInfoDto, Type expectedType, object expectedValue)
        {
            // ARRANGE
            RelativityObjectSlim document = new RelativityObjectSlim() { ArtifactID = _DOCUMENT_ARTIFACT_ID };
            IDictionary<int, INativeFile> artifactIdToNativeFileMap = new Dictionary<int, INativeFile>
            {
                { _DOCUMENT_ARTIFACT_ID, new NativeFile(_DOCUMENT_ARTIFACT_ID, _LOCATION, _FILENAME, _SIZE) }
            };

            NativeInfoRowValuesBuilder instance = PrepareSut(artifactIdToNativeFileMap);

            // ACT
            object result = instance.BuildRowValue(fieldInfoDto, document, _INITIAL_VALUE);

            // ASSERT
            result.Should().BeOfType(expectedType);
            result.Should().BeEquivalentTo(expectedValue);
        }

        [TestCaseSource(nameof(UnsupportedNonDocumentFieldInfoDtos))]
        public void BuildRowValue_ShouldThrowException_WhenNotSupportedNonDocumentSpecialField(FieldInfoDto fieldInfoDto)
        {
            // ARRANGE
            RelativityObjectSlim document = new RelativityObjectSlim() { ArtifactID = _DOCUMENT_ARTIFACT_ID };
            IDictionary<int, INativeFile> artifactIdToNativeFileMap = new Dictionary<int, INativeFile>
            {
                { _DOCUMENT_ARTIFACT_ID, new NativeFile(_DOCUMENT_ARTIFACT_ID, _LOCATION, _FILENAME, _SIZE) }
            };

            NativeInfoRowValuesBuilder instance = PrepareSut(artifactIdToNativeFileMap);

            // ACT
            Action action = () => instance.BuildRowValue(fieldInfoDto, document, _INITIAL_VALUE);

            // ASSERT
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void BuildRowValue_ShouldThrowSyncException_WhenDocumentArtifactIdNotPresentInDictionary()
        {
            // ARRANGE
            FieldInfoDto fieldInfoDto = FieldInfoDto.NativeFileSizeField();
            RelativityObjectSlim document = new RelativityObjectSlim() { ArtifactID = _DOCUMENT_ARTIFACT_ID };
            IDictionary<int, INativeFile> artifactIdToNativeFileMap = new Dictionary<int, INativeFile>();
            artifactIdToNativeFileMap.Add(_DOCUMENT_ARTIFACT_ID, new NativeFile(_DOCUMENT_ARTIFACT_ID, string.Empty, string.Empty, 0)
            {
                IsDuplicated = true
            });

            NativeInfoRowValuesBuilder instance = PrepareSut(artifactIdToNativeFileMap);

            // ACT
            Action action = () => instance.BuildRowValue(fieldInfoDto, document, _INITIAL_VALUE);

            // ASSERT
            action.Should().Throw<SyncItemLevelErrorException>();
        }

        [Test]
        public void BuildRowValue_ShouldThrowSyncException_WhenNativesAreDuplicated()
        {
            // Arrange
            FieldInfoDto fieldInfoDto = FieldInfoDto.NativeFileSizeField();

            IDictionary<int, INativeFile> artifactIdToNativeFileMap = new Dictionary<int, INativeFile>()
            {
                {
                    _DOCUMENT_ARTIFACT_ID, new NativeFile(2, string.Empty, string.Empty, 3)
                    {
                        IsDuplicated = true
                    }
                }
            };

            RelativityObjectSlim document = new RelativityObjectSlim() { ArtifactID = _DOCUMENT_ARTIFACT_ID };
            NativeInfoRowValuesBuilder instance = PrepareSut(artifactIdToNativeFileMap);

            // Act
            Action action = () => instance.BuildRowValue(fieldInfoDto, document, _INITIAL_VALUE);

            // Assert
            action
                .Should().Throw<SyncException>()
                .Which.Message.Should().Contain("has more than one native file");
        }

        private NativeInfoRowValuesBuilder PrepareSut(IDictionary<int, INativeFile> artifactIdsNativeCollection)
        {
            Mock<IAntiMalwareHandler> antiMalwareHandler = new Mock<IAntiMalwareHandler>();

            return new NativeInfoRowValuesBuilder(artifactIdsNativeCollection, antiMalwareHandler.Object);
        }
    }
}
