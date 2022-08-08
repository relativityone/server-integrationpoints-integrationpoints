using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Process
{
    [TestFixture, Category("Unit")]
    public class ExportSettingsBuilderTests : TestBase
    {
        private ExportSettingsBuilder _exportSettingsBuilder;

        private const bool _EXPORT_FULL_TEXT_AS_FILE = true;

        [SetUp]
        public override void SetUp()
        {
            _exportSettingsBuilder = new ExportSettingsBuilder(NSubstitute.Substitute.For<IHelper>(), NSubstitute.Substitute.For<IDescriptorPartsBuilder>());
        }

        [Test]
        public void ItShouldThrowExceptionForUnknownExportType()
        {
            var incorrectEnumValue = Enum.GetValues(typeof(ExportSettings.ExportType)).Cast<ExportSettings.ExportType>().Max() + 1;

            var sourceSettings = CreateSourceSettings();
            sourceSettings.ExportType = ((int)incorrectEnumValue).ToString();

            Assert.That(() => _exportSettingsBuilder.Create(sourceSettings, new List<FieldMap>(), 1),
                Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown ExportType ({incorrectEnumValue})"));
        }

        [Test]
        public void ItShouldReturnNullForUnknownImageFileType()
        {
            var incorrectEnumValue = Enum.GetValues(typeof(ExportSettings.ImageFileType)).Cast<ExportSettings.ImageFileType>().Max() + 1;

            var sourceSettings = CreateSourceSettings();
            sourceSettings.SelectedImageFileType = ((int) incorrectEnumValue).ToString();

            var exportSettings = _exportSettingsBuilder.Create(sourceSettings, new List<FieldMap>(), 1);

            Assert.That(exportSettings.ImageType, Is.Null);
        }

        [Test]
        public void ItShouldThrowExceptionForUnknownDataFileFormat()
        {
            var incorrectEnumValue = Enum.GetValues(typeof(ExportSettings.DataFileFormat)).Cast<ExportSettings.DataFileFormat>().Max() + 1;

            var sourceSettings = CreateSourceSettings();
            sourceSettings.SelectedDataFileFormat = ((int)incorrectEnumValue).ToString();

            Assert.That(() => _exportSettingsBuilder.Create(sourceSettings, new List<FieldMap>(), 1),
                Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown DataFileFormat ({incorrectEnumValue})"));
        }


        [Test]
        public void ItShouldThrowExceptionForUnknownImageDataFileFormat()
        {
            var incorrectEnumValue = Enum.GetValues(typeof(ExportSettings.ImageDataFileFormat)).Cast<ExportSettings.ImageDataFileFormat>().Max() + 1;

            var sourceSettings = CreateSourceSettings();
            sourceSettings.SelectedImageDataFileFormat = ((int)incorrectEnumValue).ToString();
            
            Assert.That(() => _exportSettingsBuilder.Create(sourceSettings, new List<FieldMap>(), 1),
                Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown ImageDataFileFormat ({incorrectEnumValue})"));
        }


        [Test]
        public void ItShouldReturnNullForUnknownNativeFilenameFromType()
        {
            var incorrectEnumValue = Enum.GetValues(typeof(ExportSettings.NativeFilenameFromType)).Cast<ExportSettings.NativeFilenameFromType>().Max() + 1;

            var sourceSettings = CreateSourceSettings();
            sourceSettings.ExportNativesToFileNamedFrom = ((int)incorrectEnumValue).ToString();

            var exportSettings = _exportSettingsBuilder.Create(sourceSettings, new List<FieldMap>(), 1);

            Assert.That(exportSettings.ExportNativesToFileNamedFrom, Is.Null);
        }

        [Test]
        public void ItShouldThrowExceptionForUnknownFilePath()
        {
            var incorrectEnumValue = Enum.GetValues(typeof(ExportSettings.FilePathType)).Cast<ExportSettings.FilePathType>().Max() + 1;

            var sourceSettings = CreateSourceSettings();
            sourceSettings.FilePath = ((int)incorrectEnumValue).ToString();

            Assert.That(() => _exportSettingsBuilder.Create(sourceSettings, new List<FieldMap>(), 1),
                Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown FilePathType ({incorrectEnumValue})"));
        }

        [Test]
        public void ItShouldSelectFieldIdentifiersForExportableFields()
        {
            var fields = new List<FieldMap>()
            {
                new FieldMap() {DestinationField = new FieldEntry() {FieldIdentifier = "1"}},
                new FieldMap() {DestinationField = new FieldEntry() {FieldIdentifier = "2"}}
            };

            var exportSettings = _exportSettingsBuilder.Create(CreateSourceSettings(), fields, 1);

            CollectionAssert.AreEqual(exportSettings.SelViewFieldIds.Keys, new List<int> {1, 2});
        }

        [Test]
        public void ItShouldSelectFieldIdentifiersForTextPrecedenceFields()
        {
            var fields = new List<FieldEntry>()
            {
                 new FieldEntry() {FieldIdentifier = "1"},
                 new FieldEntry() {FieldIdentifier = "2"}
            };

            var sourceSettings = CreateSourceSettings();
            sourceSettings.TextPrecedenceFields = fields;

            var exportSettings = _exportSettingsBuilder.Create(sourceSettings, new List<FieldMap>(), 1);

            CollectionAssert.AreEqual(exportSettings.TextPrecedenceFieldsIds, new List<int> { 1, 2 });
        }

        [Test]
        public void ItShouldSetExportFullTextAsFile()
        {
            var sourceSettings = CreateSourceSettings();

            var exportSettings = _exportSettingsBuilder.Create(sourceSettings, new List<FieldMap>(), 1);

            Assert.That(exportSettings.ExportFullTextAsFile);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ItShouldSetCorrectAutomaticFolderCreationFlag(bool isAutomaticFolderCreationEnabled)
        {
            // Arrange
            ExportUsingSavedSearchSettings sourceSettings = CreateSourceSettings();
            sourceSettings.IsAutomaticFolderCreationEnabled = isAutomaticFolderCreationEnabled;

            // Act
            ExportSettings exportSettings = _exportSettingsBuilder.Create(sourceSettings, new List<FieldMap>(), 1);

            // Assert
            Assert.That(exportSettings.IsAutomaticFolderCreationEnabled, Is.EqualTo(isAutomaticFolderCreationEnabled));
        }

        private ExportUsingSavedSearchSettings CreateSourceSettings()
        {
            return new ExportUsingSavedSearchSettings()
            {
                SelectedImageFileType = ((int)default(ExportSettings.ImageFileType)).ToString(),
                ExportNativesToFileNamedFrom = ((int)default(ExportSettings.NativeFilenameFromType)).ToString(),
                SelectedDataFileFormat = ((int)default(ExportSettings.DataFileFormat)).ToString(),
                SelectedImageDataFileFormat = ((int)default(ExportSettings.ImageDataFileFormat)).ToString(),
                FilePath = ((int)default(ExportSettings.FilePathType)).ToString(),
                ExportType = ((int)default(ExportSettings.ExportType)).ToString(),
                DataFileEncodingType = "Unicode",
                TextFileEncodingType = "Unicode",
                ExportFullTextAsFile = _EXPORT_FULL_TEXT_AS_FILE,
                TextPrecedenceFields =  new List<FieldEntry>(),
                ProductionPrecedence = ((int)default(ExportSettings.ProductionPrecedenceType)).ToString()
            };
        }

    }
}
