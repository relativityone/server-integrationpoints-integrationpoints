using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Import.V1.Models.Settings;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Toggles.Service;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Tests.Unit.Transfer.ImportAPI
{
    internal class ImportSettingsBuilderTests
    {
        private Mock<IConfigureDocumentSynchronizationConfiguration> _configurationFake;

        private Mock<IFieldManager> _fieldManagerFake;
        private Mock<ISyncToggles> _syncTogglesFake;
        private Mock<IInstanceSettings> _instanceSettingsFake;

        private ImportSettingsBuilder _sut;

        private IFixture _fxt;

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();

            _configurationFake = _fxt.Freeze<Mock<IConfigureDocumentSynchronizationConfiguration>>();
            _configurationFake.SetupGet(x => x.ImageImport).Returns(false);

            _fieldManagerFake = _fxt.Freeze<Mock<IFieldManager>>();
            _fieldManagerFake.Setup(x => x.GetNativeAllFieldsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetIdentifierOnlyFieldsMapping());

            _sut = _fxt.Create<ImportSettingsBuilder>();
        }

        [Test]
        public async Task BuildAsync_ShouldPrepareBasicSettings()
        {
            // Act
            var result = await _sut.BuildAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.DocumentSettings.Fields.FieldMappings
                .Should().OnlyContain(x => x.ColumnIndex > 0);
        }

        [Test]
        public async Task BuildAsync_ShouldConfigureOverwriteMode_WhenAppendOnlyMode()
        {
            // Arrange
            _configurationFake.SetupGet(x => x.ImportOverwriteMode)
                .Returns(ImportOverwriteMode.AppendOnly);

            // Act
            var result = await _sut.BuildAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.DocumentSettings.Overlay.Should().BeNull();
        }

        [TestCase(ImportOverwriteMode.OverlayOnly, FieldOverlayBehavior.UseFieldSettings, OverlayMode.Overlay, MultiFieldOverlayBehaviour.UseRelativityDefaults)]
        [TestCase(ImportOverwriteMode.AppendOverlay, FieldOverlayBehavior.MergeValues, OverlayMode.AppendOverlay, MultiFieldOverlayBehaviour.MergeAll)]
        public async Task BuildAsync_ShouldConfigureOverwriteMode_WhenOverlayMode(
            ImportOverwriteMode syncOverwriteMode,
            FieldOverlayBehavior syncOverlayBehavior,
            OverlayMode iapiOverwriteMode,
            MultiFieldOverlayBehaviour iapiOverlayBehavior)
        {
            // Arrange
            IReadOnlyList<FieldInfoDto> fields = GetIdentifierOnlyFieldsMapping();

            _fieldManagerFake.Setup(x => x.GetNativeAllFieldsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(fields);

            _configurationFake.SetupGet(x => x.ImportOverwriteMode)
                .Returns(syncOverwriteMode);
            _configurationFake.SetupGet(x => x.FieldOverlayBehavior)
                .Returns(syncOverlayBehavior);

            // Act
            var result = await _sut.BuildAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            FieldInfoDto identityField = fields.Single(x => x.IsIdentifier);

            result.DocumentSettings.Overlay.Mode.Should().Be(iapiOverwriteMode);
            result.DocumentSettings.Overlay.MultiFieldOverlayBehaviour.Should().Be(iapiOverlayBehavior);
            result.DocumentSettings.Overlay.KeyField.Should().Be(identityField.DestinationFieldName);
        }

        [Test]
        public async Task BuildAsync_ShouldNotConfigureFilesImport_WhenDoNotImportNativeFiles()
        {
            // Arrange
            _configurationFake.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.DoNotImportNativeFiles);

            // Act
            var result = await _sut.BuildAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.DocumentSettings.Native.Should().BeNull();
            result.DocumentSettings.Image.Should().BeNull();
        }

        [TestCase(ImportNativeFileCopyMode.CopyFiles)]
        [TestCase(ImportNativeFileCopyMode.SetFileLinks)]
        public async Task BuildAsync_ShouldConfigureFilesImport_WhenNativesAreInvolved(ImportNativeFileCopyMode nativeFileCopyMode)
        {
            // Arrange
            List<FieldInfoDto> fields = GetNativeFieldsMapping();

            _fieldManagerFake.Setup(x => x.GetNativeAllFieldsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(fields);

            _configurationFake.SetupGet(x => x.ImportNativeFileCopyMode).Returns(nativeFileCopyMode);

            // Act
            var result = await _sut.BuildAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.DocumentSettings.Native.FileNameColumnIndex
                .Should().Be(fields.Single(x => x.SpecialFieldType == SpecialFieldType.NativeFileFilename).DocumentFieldIndex);
            result.DocumentSettings.Native.FilePathColumnIndex
                .Should().Be(fields.Single(x => x.SpecialFieldType == SpecialFieldType.NativeFileLocation).DocumentFieldIndex);

            result.AdvancedSettings.Native.FileSizeColumnIndex
                .Should().Be(fields.Single(x => x.SpecialFieldType == SpecialFieldType.NativeFileSize).DocumentFieldIndex);
            result.AdvancedSettings.Native.FileType.SupportedByViewerColumnIndex
                .Should().Be(fields.Single(x => x.SpecialFieldType == SpecialFieldType.SupportedByViewer).DocumentFieldIndex);
            result.AdvancedSettings.Native.FileType.RelativityNativeTypeColumnIndex
                .Should().Be(fields.Single(x => x.SpecialFieldType == SpecialFieldType.RelativityNativeType).DocumentFieldIndex);

            result.DocumentSettings.Image.Should().BeNull();
        }

        [Test]
        public async Task BuildAsync_ShouldBillForFilesAndDisableFilesLocationValidation_WhenCopyNativeFilesIsSet()
        {
            // Arrange
            List<FieldInfoDto> fields = GetNativeFieldsMapping();

            _fieldManagerFake.Setup(x => x.GetNativeAllFieldsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(fields);

            _configurationFake.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.CopyFiles);

            // Act
            var result = await _sut.BuildAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.AdvancedSettings.Other.Billable.Should().BeTrue();
            result.AdvancedSettings.Native.ValidateFileLocation.Should().BeFalse();
        }

        [Test]
        public async Task BuildAsync_ShouldSkipSpecialFieldsInMappingConfiguration()
        {
            // Arrange
            List<FieldInfoDto> fields = GetIdentifierOnlyFieldsMapping();

            FieldInfoDto specialField = FieldInfoDto.SupportedByViewerField();
            FieldInfoDto yesNoField = FieldInfoDto.DocumentField(
                _fxt.Create<string>(),
                _fxt.Create<string>(),
                false,
                RelativityDataType.YesNo);

            fields.Add(specialField);
            fields.Add(yesNoField);

            _fieldManagerFake.Setup(x => x.GetNativeAllFieldsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(fields);

            // Act
            var result = await _sut.BuildAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.DocumentSettings.Fields.FieldMappings.Should()
                .Contain(x => x.Field == yesNoField.DestinationFieldName)
                .And.NotContain(x => x.Field == specialField.DestinationFieldName);
        }

        [Test]
        public async Task BuildAsync_ShouldConfigureDefaultRootFolder_WhenFolderStructureIsSetNone()
        {
            // Arrange
            _configurationFake.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.None);

            // Act
            var result = await _sut.BuildAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.DocumentSettings.Folder.RootFolderID.Should().BePositive();
            result.DocumentSettings.Folder.FolderPathColumnIndex.Should().BeNull();
        }

        [Test]
        public async Task BuildAsync_ShouldConfigureFolder_WhenFolderStructureIsReadFromField()
        {
            // Arrange
            List<FieldInfoDto> fields = GetIdentifierOnlyFieldsMapping();

            FieldInfoDto folderPathField = FieldInfoDto.DocumentField(
                _fxt.Create<string>(),
                _fxt.Create<string>(),
                false);
            folderPathField.DocumentFieldIndex = _fxt.Create<int>();

            fields.Add(folderPathField);

            _fieldManagerFake.Setup(x => x.GetNativeAllFieldsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(fields);

            _configurationFake.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.ReadFromField);
            _configurationFake.SetupGet(x => x.FolderPathSourceFieldName).Returns(folderPathField.SourceFieldName);

            // Act
            var result = await _sut.BuildAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.DocumentSettings.Folder.RootFolderID.Should().BePositive();
            result.DocumentSettings.Folder.FolderPathColumnIndex.Should().Be(folderPathField.DocumentFieldIndex);
        }

        [Test]
        public async Task BuildAsync_ShouldConfigureFolderFromSpecialField_WhenFolderStructureIsRetainFolderStrcture()
        {
            // Arrange
            List<FieldInfoDto> fields = GetIdentifierOnlyFieldsMapping();

            FieldInfoDto folderPathField = FieldInfoDto.FolderPathFieldFromSourceWorkspaceStructure();
            folderPathField.DocumentFieldIndex = _fxt.Create<int>();

            fields.Add(folderPathField);

            _fieldManagerFake.Setup(x => x.GetNativeAllFieldsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(fields);

            _configurationFake.SetupGet(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure);
            _configurationFake.SetupGet(x => x.FolderPathSourceFieldName).Returns(folderPathField.SourceFieldName);

            // Act
            var result = await _sut.BuildAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.DocumentSettings.Folder.RootFolderID.Should().BePositive();
            result.DocumentSettings.Folder.FolderPathColumnIndex.Should().Be(folderPathField.DocumentFieldIndex);
        }

        [Test]
        public async Task BuildAsync_ShouldConfigureMoveExistingDocuments_WhenConditionsAreMet()
        {
            // Arrange
            _configurationFake.SetupGet(x => x.FolderPathSourceFieldName).Returns(_fxt.Create<string>());
            _configurationFake.SetupGet(x => x.MoveExistingDocuments).Returns(true);
            _configurationFake.SetupGet(x => x.ImportOverwriteMode).Returns(ImportOverwriteMode.AppendOverlay);

            // Act
            var result = await _sut.BuildAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.AdvancedSettings.Folder.MoveExistingDocuments.Should().BeTrue();
        }

        private List<FieldInfoDto> GetIdentifierOnlyFieldsMapping()
        {
            FieldInfoDto identifierField = FieldInfoDto.DocumentField(
                    _fxt.Create<string>(),
                    _fxt.Create<string>(),
                    true);
            identifierField.DocumentFieldIndex = _fxt.Create<int>();

            return new List<FieldInfoDto> { identifierField };
        }

        private List<FieldInfoDto> GetNativeFieldsMapping()
        {
            FieldInfoDto identifierField = FieldInfoDto.DocumentField(
                _fxt.Create<string>(),
                _fxt.Create<string>(),
                true);

            List<FieldInfoDto> mappedFields = new List<FieldInfoDto>()
            {
                identifierField,
                FieldInfoDto.NativeFileLocationField(),
                FieldInfoDto.NativeFileFilenameField(),
                FieldInfoDto.NativeFileSizeField(),
                FieldInfoDto.SupportedByViewerField(),
                FieldInfoDto.RelativityNativeTypeField(),
            };

            foreach (var field in mappedFields)
            {
                field.DocumentFieldIndex = _fxt.Create<int>();
            }

            return mappedFields;
        }
    }
}
