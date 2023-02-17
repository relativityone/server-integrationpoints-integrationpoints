
using System;
using System.IO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using NSubstitute;
using NUnit.Framework;
using Relativity.DataExchange.Io;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Helpers
{
    [TestFixture, Category("Unit")]
    public class DestinationFolderHelperTests
    {
        private const string _FOLDER_PATH = @"\\localhost\Export";
        private const string _JOB_NAME = "JobName";
        private readonly DateTime _jobStartTime = DateTime.Now;
        private DestinationFolderHelper _instnaceUnderTest;
        private ExportSettings _exportSettings;
        private IDirectory _driHelperMock;
        private IJobInfo _jobInfoMock;

        [SetUp]
        public void Init()
        {
            _jobInfoMock = Substitute.For<IJobInfo>();
            _driHelperMock = Substitute.For<IDirectory>();

            _instnaceUnderTest = new DestinationFolderHelper(_jobInfoMock, _driHelperMock);

            _exportSettings = new ExportSettings()
            {
                ExportFilesLocation = _FOLDER_PATH
            };
        }

        [Test]
        public void ItShouldNotCreateSubFolder()
        {
            // Arrange
            _exportSettings.IsAutomaticFolderCreationEnabled = false;
            // Act

            string retFolderPath = _instnaceUnderTest.GetFolder(_exportSettings);

            // Assert
            Assert.That(retFolderPath, Is.EqualTo(_FOLDER_PATH));
        }

        [Test]
        public void ItShouldCreateSubFolder()
        {
            // Arrange
            _exportSettings.IsAutomaticFolderCreationEnabled = true;

            _jobInfoMock.GetStartTimeUtc().Returns(_jobStartTime);
            _jobInfoMock.GetName().Returns(_JOB_NAME);
            // Act

            string retFolderPath = _instnaceUnderTest.GetFolder(_exportSettings);

            // Assert
            Assert.That(retFolderPath, Is.EqualTo(
                    Path.Combine(_FOLDER_PATH,
                        string.Format(
                            $"{_JOB_NAME}_{_jobStartTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture).Replace(":", "")}"))
                )
            );
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ItShouldCreateFolder(bool createSubFolder)
        {
            _exportSettings.IsAutomaticFolderCreationEnabled = createSubFolder;
            // Act
            _instnaceUnderTest.CreateDestinationSubFolderIfNeeded(_exportSettings, _FOLDER_PATH);

            // Assert
            _driHelperMock.Received(createSubFolder ? 1 : 0).CreateDirectory(_FOLDER_PATH);
        }
    }
}
