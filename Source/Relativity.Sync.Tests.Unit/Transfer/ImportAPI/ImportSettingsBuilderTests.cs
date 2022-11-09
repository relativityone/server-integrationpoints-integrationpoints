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
            result.importSettings.Fields.FieldMappings
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
            result.importSettings.Overlay.Should().BeNull();
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

            result.importSettings.Overlay.Mode.Should().Be(iapiOverwriteMode);
            result.importSettings.Overlay.MultiFieldOverlayBehaviour.Should().Be(iapiOverlayBehavior);
            result.importSettings.Overlay.KeyField.Should().Be(identityField.DestinationFieldName);
        }

        [Test]
        public async Task BuildAsync_ShouldNotConfigureFilesImport_WhenDoNotImportNativeFiles()
        {
            // Arrange
            _configurationFake.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.DoNotImportNativeFiles);

            // Act
            var result = await _sut.BuildAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.importSettings.Native.Should().BeNull();
            result.importSettings.Image.Should().BeNull();
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
            result.importSettings.Native.FileNameColumnIndex
                .Should().Be(fields.Single(x => x.SpecialFieldType == SpecialFieldType.NativeFileFilename).DocumentFieldIndex);
            result.importSettings.Native.FilePathColumnIndex
                .Should().Be(fields.Single(x => x.SpecialFieldType == SpecialFieldType.NativeFileLocation).DocumentFieldIndex);

            result.advancedSettings.Native.FileSizeColumnIndex
                .Should().Be(fields.Single(x => x.SpecialFieldType == SpecialFieldType.NativeFileSize).DocumentFieldIndex);
            result.advancedSettings.Native.FileType.SupportedByViewerColumnIndex
                .Should().Be(fields.Single(x => x.SpecialFieldType == SpecialFieldType.SupportedByViewer).DocumentFieldIndex);
            result.advancedSettings.Native.FileType.RelativityNativeTypeColumnIndex
                .Should().Be(fields.Single(x => x.SpecialFieldType == SpecialFieldType.RelativityNativeType).DocumentFieldIndex);

            result.importSettings.Image.Should().BeNull();
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
            result.advancedSettings.Other.Billable.Should().BeTrue();
            result.advancedSettings.Native.ValidateFileLocation.Should().BeFalse();
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
            result.importSettings.Fields.FieldMappings.Should()
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
            result.importSettings.Folder.RootFolderID.Should().BePositive();
            result.importSettings.Folder.FolderPathColumnIndex.Should().BeNull();
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
            _configurationFake.SetupGet(x => x.FolderPathField).Returns(folderPathField.SourceFieldName);

            // Act
            var result = await _sut.BuildAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.importSettings.Folder.RootFolderID.Should().BePositive();
            result.importSettings.Folder.FolderPathColumnIndex.Should().Be(folderPathField.DocumentFieldIndex);
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
            _configurationFake.SetupGet(x => x.FolderPathField).Returns(folderPathField.SourceFieldName);

            // Act
            var result = await _sut.BuildAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.importSettings.Folder.RootFolderID.Should().BePositive();
            result.importSettings.Folder.FolderPathColumnIndex.Should().Be(folderPathField.DocumentFieldIndex);
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
