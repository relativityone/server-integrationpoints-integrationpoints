using System;
using System.ComponentModel;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.WinEDDS;
using NSubstitute;
using NUnit.Framework;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Process
{
    [TestFixture, Category("Unit")]
    public class ExportedObjectBuilderTests : TestBase
    {
        private IExportedArtifactNameRepository _nameRepository;
        private ExportedObjectBuilder _exportedObjectBuilder;
        private ExportSettings _exportSettings;
        private ExportFile _exportFile;

        public override void SetUp()
        {
            _nameRepository = Substitute.For<IExportedArtifactNameRepository>();
            _exportSettings = DefaultExportSettingsFactory.Create();
            _exportFile = new ExportFile(1);

            _exportedObjectBuilder = new ExportedObjectBuilder(_nameRepository);
        }

        [Test]
        public void ItShouldThrowExpectionForUnknownExportNativeWithFilenameFrom()
        {
            _exportSettings.TypeOfExport = ExportSettings.ExportType.ProductionSet;
            _exportSettings.ExportNativesToFileNamedFrom = Enum.GetValues(typeof(ExportSettings.NativeFilenameFromType)).Cast<ExportSettings.NativeFilenameFromType>().Max() + 1;

            Assert.That(() => _exportedObjectBuilder.SetExportedObjectIdAndName(_exportSettings, _exportFile),
                Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown ExportSettings.NativeFilenameFromType ({_exportSettings.ExportNativesToFileNamedFrom})"));
        }

        [Test]
        public void ItShouldSetProductionSettings()
        {
            var productionName = "production_name_965";

            _exportSettings.TypeOfExport = ExportSettings.ExportType.ProductionSet;
            _exportSettings.ProductionId = 763;
            _exportSettings.ExportNativesToFileNamedFrom = ExportSettings.NativeFilenameFromType.Identifier;

            _nameRepository.GetProductionName(_exportSettings.WorkspaceId, _exportSettings.ProductionId).Returns(productionName);

            _exportedObjectBuilder.SetExportedObjectIdAndName(_exportSettings, _exportFile);

            Assert.That(_exportFile.ArtifactID, Is.EqualTo(_exportSettings.ProductionId));
            Assert.That(_exportFile.LoadFilesPrefix, Is.EqualTo(productionName));
        }



        [Test]
        public void ItShouldSetSavedSearchSettings()
        {
            var savedSearchName = "saved_search_name_327";

            _exportSettings.TypeOfExport = ExportSettings.ExportType.SavedSearch;
            _exportSettings.SavedSearchArtifactId = 834;

            _nameRepository.GetSavedSearchName(_exportSettings.WorkspaceId, _exportSettings.SavedSearchArtifactId).Returns(savedSearchName);

            _exportedObjectBuilder.SetExportedObjectIdAndName(_exportSettings, _exportFile);

            Assert.That(_exportFile.ArtifactID, Is.EqualTo(_exportSettings.SavedSearchArtifactId));
            Assert.That(_exportFile.LoadFilesPrefix, Is.EqualTo(savedSearchName));
        }

        [Test]
        [TestCase(ExportSettings.ExportType.Folder)]
        [TestCase(ExportSettings.ExportType.FolderAndSubfolders)]
        public void ItShouldSetFolderSettings(ExportSettings.ExportType exportType)
        {
            var viewName = "view_name_803";

            _exportSettings.TypeOfExport = exportType;
            _exportSettings.FolderArtifactId = 972;
            _exportSettings.ViewId = 171;

            _nameRepository.GetViewName(_exportSettings.WorkspaceId, _exportSettings.ViewId).Returns(viewName);

            _exportedObjectBuilder.SetExportedObjectIdAndName(_exportSettings, _exportFile);

            Assert.That(_exportFile.ArtifactID, Is.EqualTo(_exportSettings.FolderArtifactId));
            Assert.That(_exportFile.ViewID, Is.EqualTo(_exportSettings.ViewId));
            Assert.That(_exportFile.LoadFilesPrefix, Is.EqualTo(viewName));
        }

        [Test]
        [TestCase(null, ExportNativeWithFilenameFrom.Identifier)]
        [TestCase(ExportSettings.NativeFilenameFromType.Identifier, ExportNativeWithFilenameFrom.Identifier)]
        [TestCase(ExportSettings.NativeFilenameFromType.Production, ExportNativeWithFilenameFrom.Production)]
        [TestCase(ExportSettings.NativeFilenameFromType.Custom, ExportNativeWithFilenameFrom.Custom)]
        public void ItShouldSetNativeFilenameFromAccordingly(ExportSettings.NativeFilenameFromType? givenSetting, ExportNativeWithFilenameFrom expectedSetting)
        {
            _exportSettings.ExportNativesToFileNamedFrom = givenSetting;

            _exportedObjectBuilder.SetExportedObjectIdAndName(_exportSettings, _exportFile);

            Assert.That(_exportFile.ExportNativesToFileNamedFrom, Is.EqualTo(expectedSetting));
        }
    }
}