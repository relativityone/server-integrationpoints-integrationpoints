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
using Relativity.IntegrationPoints.FieldsMapping.Models;
using kCura.IntegrationPoints.Domain.Managers;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
	[TestFixture, Category("Unit")]
	public class DataReaderFactoryTests : TestBase
	{
		private DataReaderFactory _instance;
		private ImportSettingsBase _settings;
		private ISerializer _serializer;
		private IJobStopManager _jobStopManager;

		[SetUp]
		public override void SetUp()
		{
			_serializer = new JSONSerializer();
			_settings = new ImportSettingsBase();

			_jobStopManager = Substitute.For<IJobStopManager>();

			IWinEddsLoadFileFactory winEddsLoadFileFactory = Substitute.For<IWinEddsLoadFileFactory>();
			winEddsLoadFileFactory.GetImageLoadFile(Arg.Any<ImportSettingsBase>()).Returns(new ImageLoadFile());
			winEddsLoadFileFactory.GetLoadFile(Arg.Any<ImportSettingsBase>()).Returns(new LoadFile());

			IFieldParser fieldParser = Substitute.For<IFieldParser>();
			fieldParser.GetFields().Returns(new List<string>());

			IFieldParserFactory fieldParserFactory = Substitute.For<IFieldParserFactory>();
			fieldParserFactory.GetFieldParser(Arg.Any<ImportProviderSettings>()).Returns(fieldParser);

			IArtifactReader loadFileReader = Substitute.For<IArtifactReader>();
			loadFileReader.GetColumnNames(Arg.Any<object>()).Returns(new string[0]);

			IWinEddsFileReaderFactory winEddsFileReaderFactory = Substitute.For<IWinEddsFileReaderFactory>();
			winEddsFileReaderFactory.GetLoadFileReader(Arg.Any<LoadFile>()).Returns(loadFileReader);
			winEddsFileReaderFactory.GetOpticonFileReader(Arg.Any<ImageLoadFile>()).Returns(Substitute.For<IImageReader>());

			_instance = new DataReaderFactory(fieldParserFactory, winEddsLoadFileFactory, winEddsFileReaderFactory, _serializer);
		}

		[Test]
		public void ItShouldReturnImportDataReader_WhenImportingDocuments()
		{
			//Arrange
			_settings.ImportType = ((int)ImportType.ImportTypeValue.Document).ToString();

			//Act
			IDataReader reader = _instance.GetDataReader(new FieldMap[0], _serializer.Serialize(_settings), _jobStopManager);

			//Assert
			Assert.IsNotNull(reader as ImportDataReader);
		}

		[Test]
		public void ItShouldReturnOpticonDataReader_WhenImportingImages()
		{
			//Arrange
			_settings.ImportType = ((int)ImportType.ImportTypeValue.Image).ToString();

			//Act
			IDataReader reader = _instance.GetDataReader(new FieldMap[0], _serializer.Serialize(_settings), _jobStopManager);

			//Assert
			Assert.IsNotNull(reader as OpticonDataReader);
		}

		[Test]
		public void ItShouldReturnOpticonDataReader_WhenImportingProductions()
		{
			//Arrange
			_settings.ImportType = ((int)ImportType.ImportTypeValue.Production).ToString();

			//Act
			IDataReader reader = _instance.GetDataReader(new FieldMap[0], _serializer.Serialize(_settings), _jobStopManager);

			//Assert
			Assert.IsNotNull(reader as OpticonDataReader);
		}
	}
}
