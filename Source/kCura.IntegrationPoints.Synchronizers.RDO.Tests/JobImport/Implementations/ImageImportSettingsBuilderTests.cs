using System;
using System.Reflection;
using System.Text;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.JobImport.Implementations
{
    [TestFixture, Category("Unit")]
    public class ImageImportSettingsBuilderTests : TestBase
    {
        protected ImageImportSettingsBuilder SystemUnderTest { get; set; }

        [SetUp]
        public override void SetUp()
        {
            var importApi = Substitute.For<IImportAPI>();
            SystemUnderTest = new ImageImportSettingsBuilder(importApi);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ItShouldMapAutoNumberImages(bool autoNumberImages)
        {
            // Arrange
            var importSettings = GetImportSettings();
            importSettings.DestinationConfiguration.AutoNumberImages = autoNumberImages;
            var imageSettings = GetImageSettings();

            // Act
            SystemUnderTest.PopulateFrom(importSettings, imageSettings);

            // Assert
            Assert.AreEqual(autoNumberImages, imageSettings.AutoNumberImages);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ItShouldMapForProduction(bool forProduction)
        {
            // Arrange
            var importSettings = GetImportSettings();
            importSettings.DestinationConfiguration.ProductionImport = forProduction;
            var imageSettings = GetImageSettings();

            // Act
            SystemUnderTest.PopulateFrom(importSettings, imageSettings);

            // Assert
            Assert.AreEqual(forProduction, imageSettings.ForProduction);
        }

        [TestCase(0)]
        [TestCase(7)]
        [TestCase(int.MaxValue)]
        public void ItShouldMapProductionArtifactId(int productionArtifactId)
        {
            // Arrange
            var importSettings = GetImportSettings();
            importSettings.DestinationConfiguration.ProductionArtifactId = productionArtifactId;
            var imageSettings = GetImageSettings();

            // Act
            SystemUnderTest.PopulateFrom(importSettings, imageSettings);

            // Assert
            Assert.AreEqual(productionArtifactId, imageSettings.ProductionArtifactID);
        }

        [TestCase("UTF-8")]
        [TestCase("ASCII")]
        public void ItShouldMapExtractedTextEncoding(string encoding)
        {
            // Arrange
            var importSettings = GetImportSettings();
            importSettings.DestinationConfiguration.ExtractedTextFileEncoding = encoding;
            var imageSettings = GetImageSettings();

            // Act
            SystemUnderTest.PopulateFrom(importSettings, imageSettings);

            // Assert
            var expectedEncoding = Encoding.GetEncoding(encoding);
            Assert.AreEqual(expectedEncoding, imageSettings.ExtractedTextEncoding);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ItShouldMapExtractedTextFieldContainsFilePath(bool containsPath)
        {
            // Arrange
            var importSettings = GetImportSettings();
            importSettings.DestinationConfiguration.ExtractedTextFieldContainsFilePath = containsPath;
            var imageSettings = GetImageSettings();

            // Act
            SystemUnderTest.PopulateFrom(importSettings, imageSettings);

            // Assert
            Assert.AreEqual(containsPath, imageSettings.ExtractedTextFieldContainsFilePath);
        }

        [TestCase("path\\to\\file\\repo")]
        public void ItShouldMapSelectedCasePath(string casePath)
        {
            // Arrange
            var importSettings = GetImportSettings();
            importSettings.DestinationConfiguration.SelectedCaseFileRepoPath = casePath;
            var imageSettings = GetImageSettings();

            // Act
            SystemUnderTest.PopulateFrom(importSettings, imageSettings);

            // Assert
            Assert.AreEqual(casePath, imageSettings.SelectedCasePath);
        }

        [TestCase(0)]
        [TestCase(7)]
        [TestCase(int.MaxValue)]
        public void ItShouldMapDestinationFolderArtifactId(int artifactId)
        {
            // Arrange
            var importSettings = GetImportSettings();
            importSettings.DestinationConfiguration.DestinationFolderArtifactId = artifactId;
            var imageSettings = GetImageSettings();

            // Act
            SystemUnderTest.PopulateFrom(importSettings, imageSettings);

            // Assert
            Assert.AreEqual(artifactId, imageSettings.DestinationFolderArtifactID);
        }

        [TestCase(true, false, true, 54443, 232, "utf-8", "path\\to\\directory" )]
        [TestCase(false, true, true, 54443, 232, "utf-8", "C:\\path" )]
        [TestCase(true, false, false, 54443, 232, "ascii", "path\\to\\directory" )]

        public void ItShouldMapAllFields(bool autoNumberImages, bool forProduction, bool containsPath,
            int productionArtifactId, int artifactId, string encoding, string casePath)
        {
            // Arrange
            var importSettings = GetImportSettings();
            importSettings.DestinationConfiguration.AutoNumberImages = autoNumberImages;
            importSettings.DestinationConfiguration.ProductionImport = forProduction;
            importSettings.DestinationConfiguration.ExtractedTextFieldContainsFilePath = containsPath;
            importSettings.DestinationConfiguration.ProductionArtifactId = productionArtifactId;
            importSettings.DestinationConfiguration.DestinationFolderArtifactId = artifactId;
            importSettings.DestinationConfiguration.ExtractedTextFileEncoding = encoding;
            importSettings.DestinationConfiguration.SelectedCaseFileRepoPath = casePath;

            var imageSettings = GetImageSettings();

            // Act
            SystemUnderTest.PopulateFrom(importSettings, imageSettings);

            // Assert
            var expectedEncoding = Encoding.GetEncoding(encoding);
            Assert.AreEqual(autoNumberImages, imageSettings.AutoNumberImages);
            Assert.AreEqual(forProduction, imageSettings.ForProduction);
            Assert.AreEqual(containsPath, imageSettings.ExtractedTextFieldContainsFilePath);
            Assert.AreEqual(productionArtifactId, imageSettings.ProductionArtifactID);
            Assert.AreEqual(artifactId, imageSettings.DestinationFolderArtifactID);
            Assert.AreEqual(expectedEncoding, imageSettings.ExtractedTextEncoding);
            Assert.AreEqual(casePath, imageSettings.SelectedCasePath);
        }

        protected ImportSettings GetImportSettings()
        {
            return new ImportSettings(new DestinationConfiguration { ExtractedTextFileEncoding = "UTF-8" });
        }

        protected ImageSettings GetImageSettings()
        {
            // ImageSettings constructor is internal, so we need to use relection in order to invoke it
            var constructor = typeof(ImageSettings).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null);
            return (ImageSettings)constructor.Invoke(null);
        }
    }
}
