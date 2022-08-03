using System.Data;
using System.IO;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;
using NSubstitute;
using kCura.IntegrationPoints.Domain.Models;
using kCura.WinEDDS.Api;
using kCura.IntegrationPoints.Domain.Managers;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
    [TestFixture, Category("Unit")]
    public class OpticonDataReaderTests : TestBase
    {
        private const string _LOAD_FILE_FULL_PATH = @"C:\LoadFileDirectory\ExampleLoadFile.opt";
        private const string _ROOTED_IMAGE_PATH = @"C:\Images\ExampleImage.PNG";
        private const string _UNROOTED_IMAGE_PATH = @"ExampleImage.PNG";
        private const string _BATES_NUMBER = @"BATES_NUMBER";
        private const int _RECORD_COUNT = 42;

        OpticonDataReader _instance;
        IImageReader _opticonFileReader;
        IJobStopManager _jobStopManager;
        ImageRecord _imageRecord;

        [SetUp]
        public override void SetUp()
        {
            ImportProviderSettings providerSettings = new ImportProviderSettings();
            _imageRecord = new ImageRecord
            {
                BatesNumber = _BATES_NUMBER,
                FileLocation = _UNROOTED_IMAGE_PATH,
                IsNewDoc = true
            };
            providerSettings.LoadFile = _LOAD_FILE_FULL_PATH;

            _opticonFileReader = Substitute.For<IImageReader>();

            _jobStopManager = Substitute.For<IJobStopManager>();

            _opticonFileReader.GetImageRecord().Returns(_imageRecord);
            _opticonFileReader.HasMoreRecords.Returns(true);
            _opticonFileReader.CountRecords().Returns(_RECORD_COUNT);

            _instance = new OpticonDataReader(providerSettings, _opticonFileReader, _jobStopManager);
        }

        [Test]
        public void ItShouldHandleEmptyFiles()
        {
            //Arrange
            _opticonFileReader.HasMoreRecords.Returns(false);

            //Act
            _instance.Init();

            //Assert
            Assert.IsFalse(_instance.Read());
        }


        [Test]
        public void ItShouldCallGetImageRecord_IfReaderHasMoreRecords()
        {
            //Arrange

            //Act
            _instance.Init();

            //Assert
            Assert.IsTrue(_instance.Read());
            _opticonFileReader.Received(1).GetImageRecord();
        }

        [Test]
        public void ItShouldRewriteFileLocation_WithoutRootedPath()
        {
            //Arrange

            //Act
            _instance.Init();

            //Assert
            Assert.IsTrue(_instance.Read());
            Assert.AreEqual(_instance.GetValue(OpticonInfo.FILE_LOCATION_FIELD_INDEX), Path.Combine(Path.GetDirectoryName(_LOAD_FILE_FULL_PATH), _imageRecord.FileLocation));
        }

        [Test]
        public void ItShouldNotRewriteFileLocation_WithRootedPath()
        {
            //Arrange
            _imageRecord.FileLocation = _ROOTED_IMAGE_PATH;

            //Act
            _instance.Init();

            //Assert
            Assert.IsTrue(_instance.Read());
            Assert.AreEqual(_instance.GetValue(OpticonInfo.FILE_LOCATION_FIELD_INDEX), _ROOTED_IMAGE_PATH);
        }

        [Test]
        public void ItShouldNotAdvanceDocumentId_WhenIsNewDocFalse()
        {
            //Arrange
            _imageRecord.IsNewDoc = false;

            //Act
            _instance.Init();

            //Assert
            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(_instance.Read());
                Assert.AreEqual(0.ToString(), _instance.GetValue(OpticonInfo.DOCUMENT_ID_FIELD_INDEX));
            }
        }

        [Test]
        public void ItShouldAdvanceDocumentId_WhenIsNewDocTrue()
        {
            //Arrange

            //Act
            _instance.Init();

            //Assert
            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(_instance.Read());
                Assert.AreEqual((i+1).ToString(), _instance.GetValue(OpticonInfo.DOCUMENT_ID_FIELD_INDEX));
            }
        }

        [Test]
        public void ItShouldReturnBatesNumberUnchanged()
        {
            //Arrange

            //Act
            _instance.Init();

            //Assert
            Assert.IsTrue(_instance.Read());
            Assert.AreEqual(_imageRecord.BatesNumber, _instance.GetValue(OpticonInfo.BATES_NUMBER_FIELD_INDEX));
        }

        [Test]
        public void ItShouldHaveCorrectSchemaTable()
        {
            //Arrange

            //Act
            _instance.Init();

            //Assert
            DataTable schemaTable = _instance.GetSchemaTable();
            Assert.AreEqual(3, schemaTable.Columns.Count);
            Assert.AreEqual(OpticonInfo.DOCUMENT_ID_FIELD_NAME, schemaTable.Columns[OpticonInfo.DOCUMENT_ID_FIELD_INDEX].ColumnName);
            Assert.AreEqual(OpticonInfo.BATES_NUMBER_FIELD_NAME, schemaTable.Columns[OpticonInfo.BATES_NUMBER_FIELD_INDEX].ColumnName);
            Assert.AreEqual(OpticonInfo.FILE_LOCATION_FIELD_NAME, schemaTable.Columns[OpticonInfo.FILE_LOCATION_FIELD_INDEX].ColumnName);
        }

        [Test]
        public void ItShouldHaveCorrectFieldCount()
        {
            //Arrange

            //Act
            _instance.Init();

            //Assert
            Assert.AreEqual(3, _instance.FieldCount);
        }

        [Test]
        public void ItShouldBeClosedAfterCloseCalled()
        {
            //Arrange

            //Act
            _instance.Init();
            _instance.Close();

            //Assert
            Assert.IsTrue(_instance.IsClosed);
        }

        [Test]
        public void ItShouldReturnCorrectOrdinal()
        {
            //Arrange

            //Act
            _instance.Init();

            //Assert
            Assert.AreEqual(OpticonInfo.BATES_NUMBER_FIELD_INDEX, _instance.GetOrdinal(OpticonInfo.BATES_NUMBER_FIELD_NAME));
            Assert.AreEqual(OpticonInfo.DOCUMENT_ID_FIELD_INDEX, _instance.GetOrdinal(OpticonInfo.DOCUMENT_ID_FIELD_NAME));
            Assert.AreEqual(OpticonInfo.FILE_LOCATION_FIELD_INDEX, _instance.GetOrdinal(OpticonInfo.FILE_LOCATION_FIELD_NAME));
        }

        [Test]
        public void ItShouldReturnCorrectName()
        {
            //Arrange

            //Act
            _instance.Init();

            //Assert
            Assert.AreEqual(OpticonInfo.BATES_NUMBER_FIELD_NAME, _instance.GetName(OpticonInfo.BATES_NUMBER_FIELD_INDEX));
            Assert.AreEqual(OpticonInfo.DOCUMENT_ID_FIELD_NAME, _instance.GetName(OpticonInfo.DOCUMENT_ID_FIELD_INDEX));
            Assert.AreEqual(OpticonInfo.FILE_LOCATION_FIELD_NAME, _instance.GetName(OpticonInfo.FILE_LOCATION_FIELD_INDEX));
        }

        [Test]
        public void ItShouldReturnCorrectRecordCountFromOpticonFileReader()
        {
            //Arrange

            //Act
            _instance.Init();

            //Assert
            Assert.AreEqual(_RECORD_COUNT, _instance.CountRecords());
        }

        [Test]
        public void ItShouldThrowNotImplementedForUnusedMethods()
        {
            //Arrange

            //Act
            _instance.Init();

            //Assert

            //Methods
            Assert.Throws(typeof(System.NotImplementedException), () => _instance.GetDataTypeName(0));
            Assert.Throws(typeof(System.NotImplementedException), () => _instance.GetFieldType(0));
        }

        [TestCase(false, true)]
        [TestCase(true, false)]
        public void Read_ShouldHandleRead_WhenDrainStopWasTriggered(bool isNewDoc, bool expectedReadResult)
        {
            // Arrange
            _imageRecord.IsNewDoc = isNewDoc;
            _jobStopManager.ShouldDrainStop.Returns(true);

            _instance.Init();

            // Act
            bool readResult = _instance.Read();

            // Assert
            Assert.AreEqual(readResult, expectedReadResult);
        }
    }
}
