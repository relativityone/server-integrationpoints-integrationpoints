using System;
using System.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.Exporter.Images;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Api.Shared.Manager.Export;
using DataView = kCura.Data.DataView;
using ExportSettings = kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportSettings;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Images
{
	[TestFixture]
	public class ImageExporterServiceTests : TestBase
	{
		private ImageExporterService _instance;

		#region "Dependencies"
		private IExporter _exporter;
		private IRepositoryFactory _sourceRepositoryFactory;
		private IRepositoryFactory _targetRepositoryFactory;
		private IJobStopManager _jobStopManager;
		private IHelper _helper;
		private IBaseServiceContextProvider _baseServiceContextProvider;
		private FieldMap[] _mappedFields;
		private IFileRepository _fileRepository;

		#endregion


		private const int _START_AT = 0;
		private const int _SEARCH_ARTIFACT_ID = 0;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1;
		private const int _TARGET_WORKSPACE_ARTIFACT_ID = 2;
		private const int _FIELD_IDENTIFIER = 12345;

		[SetUp]
		public override void SetUp()
		{
			_exporter = Substitute.For<IExporter>();
			_sourceRepositoryFactory = Substitute.For<IRepositoryFactory>();
			_targetRepositoryFactory = Substitute.For<IRepositoryFactory>();
			_jobStopManager = Substitute.For<IJobStopManager>();
			_helper = Substitute.For<IHelper>();

			_baseServiceContextProvider = Substitute.For<IBaseServiceContextProvider>();

			_mappedFields = new[]
			{
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						FieldIdentifier = _FIELD_IDENTIFIER.ToString()
					}
				}
			};

			var exportJobInfo = new Export.InitializationResults()
			{
				RunId = new Guid(),
				RowCount = 1,
				ColumnNames = new string[] { "Name", "Identifier" }
			};

			_exporter.InitializeExport(_SEARCH_ARTIFACT_ID,
				Arg.Any<int[]>(), _START_AT).Returns(exportJobInfo);


			var queryFieldLookupRepository = Substitute.For<IQueryFieldLookupRepository>();
			var viewFieldInfo = new ViewFieldInfo("", "", FieldTypeHelper.FieldType.Empty);
			queryFieldLookupRepository.GetFieldByArtifactId(_FIELD_IDENTIFIER).Returns(viewFieldInfo);
			_sourceRepositoryFactory.GetQueryFieldLookupRepository(_SOURCE_WORKSPACE_ARTIFACT_ID).Returns(queryFieldLookupRepository);

			_fileRepository = Substitute.For<IFileRepository>();
			_sourceRepositoryFactory.GetFileRepository(_SOURCE_WORKSPACE_ARTIFACT_ID).Returns(_fileRepository);
			}


		#region "Tests"

		[Test]
		public void ItShouldGetDataTransferContext()
		{
			// Arrange
			string config = GetConfig(SourceConfiguration.ExportType.SavedSearch);

			_instance = new ImageExporterService(_exporter, _sourceRepositoryFactory, _targetRepositoryFactory,
				_jobStopManager, _helper, _baseServiceContextProvider, _mappedFields, _START_AT, config, _SEARCH_ARTIFACT_ID, settings: null);

			var transferConfiguration = Substitute.For<IExporterTransferConfiguration>();

			// Act
			IDataTransferContext actual = _instance.GetDataTransferContext(transferConfiguration);

			// Assert
			Assert.NotNull(actual);
			Assert.IsInstanceOf(typeof(ExporterTransferContext), actual);
		}

		[Test]
		public void ItShouldRetrieveOriginalImages()
		{
			// Arrange
			string config = GetConfig(SourceConfiguration.ExportType.SavedSearch);
			ImportSettings _settings = new ImportSettings()
			{
				ProductionPrecedence = String.Empty
			};
			int documentArtifactId = 10000;

			_exporter.RetrieveResults(Arg.Any<Guid>(), Arg.Any<int[]>(), Arg.Any<int>()).Returns(PrepareRetrievedData(documentArtifactId));

			string imageIdentifier = "AZIPPER_0007293";
			_fileRepository.RetrieveAllImagesForDocuments(documentArtifactId).Returns(
				CreateDocumentImageDataView(SourceConfiguration.ExportType.SavedSearch, imageIdentifier));

			_instance = new ImageExporterService(_exporter, _sourceRepositoryFactory, _targetRepositoryFactory,
				_jobStopManager, _helper, _baseServiceContextProvider, _mappedFields, _START_AT, config, _SEARCH_ARTIFACT_ID, _settings);

			_instance.GetDataTransferContext(Substitute.For<IExporterTransferConfiguration>());

			// Act
			ArtifactDTO[] actual = _instance.RetrieveData(0);

			// Assert
			Assert.That(actual.Length, Is.EqualTo(1));
			Assert.That(actual[0].GetFieldByName(IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD_NAME).Value, Is.EqualTo("AZIPPER_0007293"));
		}

		[Test]
		public void ItShouldRetrieveProductionImages()
		{
			// Arrange
			int productionArtifactId = 10010;
			string config = GetConfig(SourceConfiguration.ExportType.ProductionSet, productionArtifactId);

			ImportSettings _settings = new ImportSettings()
			{
				ProductionPrecedence = ExportSettings.ProductionPrecedenceType.Produced.ToString(),
				ImagePrecedence = new[] { new ProductionDTO() { ArtifactID = productionArtifactId.ToString() } },
			};
			int documentArtifactId = 10000;

			_exporter.RetrieveResults(Arg.Any<Guid>(), Arg.Any<int[]>(), Arg.Any<int>()).Returns(PrepareRetrievedData(documentArtifactId));

			string imageIdentifier = "AZIPPER_0007293";
			_fileRepository.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSet(productionArtifactId, documentArtifactId).Returns(
				CreateDocumentImageDataView(SourceConfiguration.ExportType.ProductionSet, imageIdentifier));

			_instance = new ImageExporterService(_exporter, _sourceRepositoryFactory, _targetRepositoryFactory,
				_jobStopManager, _helper, _baseServiceContextProvider, _mappedFields, _START_AT, config, _SEARCH_ARTIFACT_ID, _settings);

			_instance.GetDataTransferContext(Substitute.For<IExporterTransferConfiguration>());

			// Act
			ArtifactDTO[] actual = _instance.RetrieveData(0);

			// Assert
			Assert.That(actual.Length, Is.EqualTo(1));
			Assert.That(actual[0].GetFieldByName(IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD_NAME).Value, Is.EqualTo("AZIPPER_0007293"));
		}

		#endregion

		#region "Helpers"

		//private void MockRetrievedViewFieldInfoInExporterServiceBaseConstructor(ViewFieldInfo viewFieldInfo)
		//{
		//    var queryFieldLookupRepository = Substitute.For<IQueryFieldLookupRepository>();
		//    queryFieldLookupRepository.GetFieldByArtifactId(_FIELD_IDENTIFIER).Returns(viewFieldInfo);
		//    _sourceRepositoryFactory.GetQueryFieldLookupRepository(_SOURCE_WORKSPACE_ARTIFACT_ID).Returns(queryFieldLookupRepository);
		//}

		private string GetConfig(SourceConfiguration.ExportType exportType, int sourceProductionId = 0)
		{
			var sourceConfiguration = new SourceConfiguration()
			{
				SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ARTIFACT_ID,
				TargetWorkspaceArtifactId = _TARGET_WORKSPACE_ARTIFACT_ID,
				TypeOfExport = exportType,
				SourceProductionId = sourceProductionId
			};

			return JsonConvert.SerializeObject(sourceConfiguration);
		}

		private object[] PrepareRetrievedData(int documentArtifactId)
		{
			object[] data = { "AZIPPER_0007293", documentArtifactId };
			object[] retrievedData = { data };

			return retrievedData;
		}

		private DataView CreateDocumentImageDataView(SourceConfiguration.ExportType exportType, string identifier)
		{
			var dataTable = new DataTable();

			string identifierColumn = null;
			if (exportType == SourceConfiguration.ExportType.SavedSearch)
				identifierColumn = "Identifier";
			else
				identifierColumn = "NativeIdentifier"; // for production

			dataTable.Columns.Add(identifierColumn);
			dataTable.Columns.Add("Location");
			DataRow row = dataTable.NewRow();
			row[identifierColumn] = "AZIPPER_0007293";
			row["Location"] = "\\somelocation";
			dataTable.Rows.Add(row);
			return new DataView(dataTable);
		}

		#endregion
	}
}
