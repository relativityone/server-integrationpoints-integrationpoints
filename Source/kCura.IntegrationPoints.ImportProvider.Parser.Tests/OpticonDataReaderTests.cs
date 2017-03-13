using System.Data;
using System.IO;
using System.Reflection;

using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;
using NSubstitute;
using kCura.IntegrationPoints.ImportProvider.Parser.Services;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;

using kCura.WinEDDS;
using kCura.WinEDDS.Api;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
	public class OpticonDataReaderTests : TestBase
	{
		private const string _LOAD_FILE_FULL_PATH = @"C:\LoadFileDirectory\ExampleLoadFile.opt";
		private const string _ROOTED_IMAGE_PATH = @"C:\Images\ExampleImage.PNG";
		private const string _UNROOTED_IMAGE_PATH = @"ExampleImage.PNG";
		private const string _BATES_NUMBER = @"BATES_NUMBER";

		OpticonDataReader _instance;
		IImageReader _opticonFileReader;
		ImageRecord _imageRecord;
		ImageLoadFile _imageLoadFile;
		ImportProviderSettings _providerSettings;

		[SetUp]
		public override void SetUp()
		{
			_providerSettings = new ImportProviderSettings();
			_imageLoadFile = new ImageLoadFile();
			_imageRecord = new ImageRecord
			{
				BatesNumber = _BATES_NUMBER,
				FileLocation = _UNROOTED_IMAGE_PATH,
				IsNewDoc = true
			};
			_providerSettings.LoadFile = _LOAD_FILE_FULL_PATH;

			_opticonFileReader = Substitute.For<IImageReader>();

			_opticonFileReader.GetImageRecord().Returns(_imageRecord);
			_opticonFileReader.HasMoreRecords.Returns(true);

			_instance = new OpticonDataReader(_providerSettings, _imageLoadFile, _opticonFileReader);
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
			Assert.IsTrue(_instance.Read());
			Assert.AreEqual(_instance[OpticonInfo.BATES_NUMBER_FIELD_NAME], _instance[OpticonInfo.BATES_NUMBER_FIELD_INDEX]);
			Assert.AreEqual(_instance[OpticonInfo.DOCUMENT_ID_FIELD_NAME], _instance[OpticonInfo.DOCUMENT_ID_FIELD_INDEX]);
			Assert.AreEqual(_instance[OpticonInfo.FILE_LOCATION_FIELD_NAME], _instance[OpticonInfo.FILE_LOCATION_FIELD_INDEX]);
		}
	}
}
