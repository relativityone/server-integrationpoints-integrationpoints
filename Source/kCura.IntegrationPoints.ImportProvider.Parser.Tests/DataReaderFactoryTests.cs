using System.Collections.Generic;
using System.Data;

using NUnit.Framework;
using NSubstitute;

using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoint.Tests.Core;
using kCura.WinEDDS;
using kCura.WinEDDS.Api;
using kCura.Apps.Common.Utils.Serializers;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
	public class DataReaderFactoryTests : TestBase
	{
		private DataReaderFactory _instance;
		private IWinEddsLoadFileFactory _winEddsLoadFileFactory;
		private IWinEddsFileReaderFactory _winEddsFileReaderFactory;
		private IFieldParserFactory _fieldParserFactory;
		private IFieldParser _fieldParser;
		private IArtifactReader _loadFileReader;
		private IImageReader _opticonFileReader;
		private LoadFile _loadFile;
		private ImageLoadFile _imageLoadFile;
		private ImportSettingsBase _settings;
		private JSONSerializer _serializer;

		[SetUp]
		public override void SetUp()
		{
			_serializer = new JSONSerializer();
			_settings = new ImportSettingsBase();

			_loadFile = new LoadFile();
			_imageLoadFile = new ImageLoadFile();

			_winEddsLoadFileFactory = Substitute.For<IWinEddsLoadFileFactory>();
			_winEddsLoadFileFactory.GetImageLoadFile(Arg.Any<ImportSettingsBase>()).Returns(_imageLoadFile);
			_winEddsLoadFileFactory.GetLoadFile(Arg.Any<ImportSettingsBase>()).Returns(_loadFile);

			_fieldParser = Substitute.For<IFieldParser>();
			_fieldParser.GetFields().Returns(new List<string>());

			_fieldParserFactory = Substitute.For<IFieldParserFactory>();
			_fieldParserFactory.GetFieldParser(Arg.Any<ImportProviderSettings>()).Returns(_fieldParser);

			_opticonFileReader = Substitute.For<IImageReader>();
			_loadFileReader = Substitute.For<IArtifactReader>();
			_loadFileReader.GetColumnNames(Arg.Any<object>()).Returns(new string[0]);

			_winEddsFileReaderFactory = Substitute.For<IWinEddsFileReaderFactory>();
			_winEddsFileReaderFactory.GetLoadFileReader(Arg.Any<LoadFile>()).Returns(_loadFileReader);
			_winEddsFileReaderFactory.GetOpticonFileReader(Arg.Any<ImageLoadFile>()).Returns(_opticonFileReader);

			_instance = new DataReaderFactory(_fieldParserFactory, _winEddsLoadFileFactory, _winEddsFileReaderFactory);
		}

		[Test]
		public void ItShouldReturnImportDataReader_WhenImportingDocuments()
		{
			//Arrange
			_settings.ImportType = ((int)ImportType.ImportTypeValue.Document).ToString();

			//Act
			IDataReader reader = _instance.GetDataReader(new FieldMap[0], _serializer.Serialize(_settings));

			//Assert
			Assert.IsNotNull(reader as ImportDataReader);
		}

		[Test]
		public void ItShouldReturnOpticonDataReader_WhenImportingImages()
		{
			//Arrange
			_settings.ImportType = ((int)ImportType.ImportTypeValue.Image).ToString();

			//Act
			IDataReader reader = _instance.GetDataReader(new FieldMap[0], _serializer.Serialize(_settings));

			//Assert
			Assert.IsNotNull(reader as OpticonDataReader);
		}

		[Test]
		public void ItShouldReturnOpticonDataReader_WhenImportingProductions()
		{
			//Arrange
			_settings.ImportType = ((int)ImportType.ImportTypeValue.Production).ToString();

			//Act
			IDataReader reader = _instance.GetDataReader(new FieldMap[0], _serializer.Serialize(_settings));

			//Assert
			Assert.IsNotNull(reader as OpticonDataReader);
		}
	}
}
