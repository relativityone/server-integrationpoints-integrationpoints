using System;
using System.Linq;
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
using Relativity.Services.Interfaces.File.Models;
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
		private IRelativityObjectManager _relativityObjectManager;
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
			_relativityObjectManager = Substitute.For<IRelativityObjectManager>();

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


			IQueryFieldLookupRepository queryFieldLookupRepository = Substitute.For<IQueryFieldLookupRepository>();
			var viewFieldInfo = new ViewFieldInfo("", "", FieldTypeHelper.FieldType.Empty);
			queryFieldLookupRepository.GetFieldByArtifactId(_FIELD_IDENTIFIER).Returns(viewFieldInfo);
			_sourceRepositoryFactory.GetQueryFieldLookupRepository(_SOURCE_WORKSPACE_ARTIFACT_ID).Returns(queryFieldLookupRepository);

			_fileRepository = Substitute.For<IFileRepository>();
			_sourceRepositoryFactory.GetFileRepository().Returns(_fileRepository);
		}


		#region "Tests"

		[Test]
		public void ItShouldGetDataTransferContext()
		{
			// Arrange
			string config = GetConfig(SourceConfiguration.ExportType.SavedSearch);

			_instance = new ImageExporterService(_exporter, _relativityObjectManager, _sourceRepositoryFactory, _targetRepositoryFactory,
				_jobStopManager, _helper, _baseServiceContextProvider, _mappedFields, _START_AT, config, _SEARCH_ARTIFACT_ID, settings: null);

			IExporterTransferConfiguration transferConfiguration = Substitute.For<IExporterTransferConfiguration>();

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
			ImportSettings settings = new ImportSettings
			{
				ProductionPrecedence = string.Empty
			};
			const int documentArtifactID = 10000;

			_exporter
				.RetrieveResults(Arg.Any<Guid>(), Arg.Any<int[]>(), Arg.Any<int>())
				.Returns(PrepareRetrievedData(documentArtifactID));

			_fileRepository
				.GetImagesForDocuments(
					_SOURCE_WORKSPACE_ARTIFACT_ID,
					Arg.Is<int[]>(x => x.Single() == documentArtifactID))
				.Returns(CreateDocumentImageResponses());

			_instance = new ImageExporterService(
				_exporter, 
				_relativityObjectManager, 
				_sourceRepositoryFactory, 
				_targetRepositoryFactory,
				_jobStopManager, 
				_helper, 
				_baseServiceContextProvider, 
				_mappedFields, 
				_START_AT, 
				config, 
				_SEARCH_ARTIFACT_ID,
				settings
			);

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
			const int productionArtifactID = 10010;
			const int documentArtifactID = 10000;

			string config = GetConfig(SourceConfiguration.ExportType.ProductionSet, productionArtifactID);

			ImportSettings _settings = new ImportSettings()
			{
				ProductionPrecedence = ExportSettings.ProductionPrecedenceType.Produced.ToString(),
				ImagePrecedence = new[] { new ProductionDTO() { ArtifactID = productionArtifactID.ToString() } },
			};

			_exporter.RetrieveResults(Arg.Any<Guid>(), Arg.Any<int[]>(), Arg.Any<int>()).Returns(PrepareRetrievedData(documentArtifactID));

			_fileRepository
				.GetImagesForProductionDocuments(
					_SOURCE_WORKSPACE_ARTIFACT_ID, 
					productionArtifactID, 
					Arg.Is<int[]>(x => x.Single() == documentArtifactID))
				.Returns(CreateProductionDocumentImageResponses());

			_instance = new ImageExporterService(_exporter, _relativityObjectManager, _sourceRepositoryFactory, _targetRepositoryFactory,
				_jobStopManager, _helper, _baseServiceContextProvider, _mappedFields, _START_AT, config, _SEARCH_ARTIFACT_ID, _settings);

			_instance.GetDataTransferContext(Substitute.For<IExporterTransferConfiguration>());

			// Act
			ArtifactDTO[] actual = _instance.RetrieveData(0);

			// Assert
			Assert.That(actual.Length, Is.EqualTo(1));
			Assert.That(actual[0].GetFieldByName(IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD_NAME).Value, Is.EqualTo("AZIPPER_0007293"));
		}

		[Test]
		public void ItShouldReturnEmptyImageLocationWhenProductionImageLocationIsDbNull()
		{
			// Arrange
			const int productionArtifactID = 10010;
			const int documentArtifactID = 10000;

			string config = GetConfig(SourceConfiguration.ExportType.ProductionSet, productionArtifactID);

			ImportSettings settings = new ImportSettings()
			{
				ProductionPrecedence = ExportSettings.ProductionPrecedenceType.Produced.ToString(),
				ImagePrecedence = new[] { new ProductionDTO() { ArtifactID = productionArtifactID.ToString() } },
			};		

			_exporter
				.RetrieveResults(Arg.Any<Guid>(), Arg.Any<int[]>(), Arg.Any<int>())
				.Returns(PrepareRetrievedData(documentArtifactID));
			
			_fileRepository.GetImagesForProductionDocuments(
					_SOURCE_WORKSPACE_ARTIFACT_ID, 
					productionArtifactID, 
					Arg.Is<int[]>(x => x.Single() == documentArtifactID))
				.Returns(info =>
				{
					ProductionDocumentImageResponse[] responses = CreateProductionDocumentImageResponses();
					responses.First().Location = null;
					return responses;
				});

			_instance = new ImageExporterService(_exporter, _relativityObjectManager, _sourceRepositoryFactory, _targetRepositoryFactory,
				_jobStopManager, _helper, _baseServiceContextProvider, _mappedFields, _START_AT, config, _SEARCH_ARTIFACT_ID, settings);

			_instance.GetDataTransferContext(Substitute.For<IExporterTransferConfiguration>());

			// Act
			ArtifactDTO[] actual = _instance.RetrieveData(0);

			// Assert
			Assert.That(actual.Length, Is.EqualTo(1));
			Assert.That(actual[0].GetFieldByName(IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME).Value, Is.EqualTo(string.Empty));
		}

		#endregion

		#region "Helpers"

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

		private DocumentImageResponse[] CreateDocumentImageResponses()
		{
			return new[]
			{
				new DocumentImageResponse
				{
					Identifier = "AZIPPER_0007293",
					Location = "\\somelocation"
				}
			};
		}

		private ProductionDocumentImageResponse[] CreateProductionDocumentImageResponses()
		{
			return new[]
			{
				new ProductionDocumentImageResponse
				{
					NativeIdentifier = "AZIPPER_0007293",
					Location = "\\somelocation"
				}
			};
		}

		#endregion
	}
}
