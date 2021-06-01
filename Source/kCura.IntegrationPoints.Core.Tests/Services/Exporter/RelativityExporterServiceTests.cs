using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter
{
	[TestFixture, Category("Unit")]
	public class RelativityExporterServiceTests : TestBase
	{
		private ArtifactDTO _goldFlowExpectedDto;
		private FieldMap[] _mappedFields;
		private IList<RelativityObjectSlimDto> _goldFlowRetrievableData;
		private ExportInitializationResultsDto _exportApiResult;
		private Mock<IDocumentRepository> _documentRepository;
		private Mock<IFileRepository> _fileRepository;
		private Mock<IRepositoryFactory> _repositoryFactoryMock;
		private Mock<IFolderPathReader> _folderPathReader;
		private Mock<IHelper> _helper;
		private Mock<IJobStopManager> _jobStopManager;
		private Mock<IQueryFieldLookupRepository> _queryFieldLookupRepository;
		private Mock<IRelativityObjectManager> _relativityObjectManager;
		private Mock<ISerializer> _serializer;
		private Mock<IExportDataSanitizer> _exportDataSanitizer;
		private RelativityExporterService _instance;
		private IDictionary<string, object> _fieldValues;

		private const string _CONTROL_NUMBER = "Control Number";
		private const string _CONTROL_NUMBER_VALUE = "REL01";
		private const string _EMAIL = "Email";
		private const string _EMAIL_VALUE = "test@relativity.com";
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1;
		private const int _TARGET_WORKSPACE_ARTIFACT_ID = 2;
		private const int _FIELD_IDENTIFIER = 12345;
		private const int _START_AT = 0;
		private const int _SEARCH_ARTIFACT_ID = 0;

		[SetUp]
		public override void SetUp()
		{
			var apiLogMock = new Mock<IAPILog>();
			_helper = new Mock<IHelper>();
			_helper
				.Setup(x => x.GetLoggerFactory().GetLogger().ForContext<RelativityExporterService>())
				.Returns(apiLogMock.Object);
			_documentRepository = new Mock<IDocumentRepository>();
			_fileRepository = new Mock<IFileRepository>();
			_repositoryFactoryMock = new Mock<IRepositoryFactory>();
			Mock<IFieldQueryRepository> fieldQueryRepositoryMock = new Mock<IFieldQueryRepository>();
			_repositoryFactoryMock.Setup(x => x.GetFieldQueryRepository(It.IsAny<int>()))
				.Returns(fieldQueryRepositoryMock.Object);

			// source identifier is the only thing that matters
			_mappedFields = new[]
			{
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						FieldIdentifier = "123",
						IsIdentifier = true,
						DisplayName = "Control Number [Object Identifier]"
					}
				},
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						FieldIdentifier = "456"
					}
				}
			};

			_fieldValues = new Dictionary<string, object>
			{
				{_CONTROL_NUMBER, _CONTROL_NUMBER_VALUE},
				{_EMAIL, _EMAIL_VALUE}
			};

			const int artifactID = 1111;
			_goldFlowRetrievableData = new List<RelativityObjectSlimDto>
			{
				new RelativityObjectSlimDto(artifactID, _fieldValues)
			};
			_goldFlowExpectedDto = new ArtifactDTO(1111, 10, "Document", new[]
			{
				new ArtifactFieldDTO()
				{
					ArtifactId = 123,
					Value = _CONTROL_NUMBER_VALUE,
					Name = _CONTROL_NUMBER,
					FieldType = FieldTypeHelper.FieldType.Empty
				},
				new ArtifactFieldDTO()
				{
					ArtifactId = 456,
					Value = _EMAIL_VALUE,
					Name = _EMAIL,
					FieldType = FieldTypeHelper.FieldType.Empty
				},
			});

			_jobStopManager = new Mock<IJobStopManager>();
			_jobStopManager
				.Setup(x => x.IsStopRequested())
				.Returns(false);
			_folderPathReader = new Mock<IFolderPathReader>();

			_queryFieldLookupRepository = new Mock<IQueryFieldLookupRepository>();
			var viewFieldInfo = new ViewFieldInfo("", "", FieldTypeHelper.FieldType.Empty);
			_queryFieldLookupRepository.Setup(x => x.GetFieldByArtifactID(_FIELD_IDENTIFIER)).Returns(viewFieldInfo);
			foreach (var artifactFieldDto in _goldFlowExpectedDto.Fields)
			{
				_queryFieldLookupRepository.Setup(x => x.GetFieldByArtifactID(artifactFieldDto.ArtifactId)).Returns(viewFieldInfo);
			}
			_repositoryFactoryMock.Setup(x => x.GetQueryFieldLookupRepository(_SOURCE_WORKSPACE_ARTIFACT_ID))
				.Returns(_queryFieldLookupRepository.Object);

			_relativityObjectManager = new Mock<IRelativityObjectManager>();
			_serializer = new Mock<ISerializer>();
			_exportDataSanitizer = new Mock<IExportDataSanitizer>();
		}

		[Test]
		public void HasDataToRetrieve_ReturnsFalseOnJobThatDoesNotHaveAnythingToRead()
		{
			// arrange
			SetupInitializationResultAndTestInstance(0);

			_queryFieldLookupRepository
				.Setup(x => x.GetFieldByArtifactID(It.IsAny<int>()))
				.Returns(new ViewFieldInfo(string.Empty, string.Empty, FieldTypeHelper.FieldType.Code));

			// act
			bool hasDataToRetrieve = _instance.HasDataToRetrieve;

			// assert
			Assert.IsFalse(hasDataToRetrieve);
		}


		[Test]
		public void HasDataToRetrieve_ReturnsFalseOnFinishedJob()
		{
			// arrange
			SetupInitializationResultAndTestInstance(1);

			_documentRepository
				.Setup(x => x.RetrieveResultsBlockFromExportAsync(_exportApiResult, 1, 0))
				.ReturnsAsync(_goldFlowRetrievableData);

			_queryFieldLookupRepository
				.Setup(x => x.GetFieldByArtifactID(It.IsAny<int>()))
				.Returns(new ViewFieldInfo(string.Empty, string.Empty, FieldTypeHelper.FieldType.Empty));

			_queryFieldLookupRepository
				.Setup(x => x.GetFieldTypeByArtifactID(It.IsAny<int>()))
				.Returns(FieldTypeHelper.FieldType.Empty);

			// act
			ArtifactDTO[] retrievedData = _instance.RetrieveData(1);
			bool hasDataToRetrieve = _instance.HasDataToRetrieve;

			// assert
			ValidateArtifact(_goldFlowExpectedDto, retrievedData[0]);
			Assert.IsFalse(hasDataToRetrieve);

		}

		[Test]
		public void HasDataToRetrieve_ReturnsTrueOnRunningJob_WithIntMaxData()
		{
			// arrange
			SetupInitializationResultAndTestInstance(int.MaxValue);

			// act
			bool hasDataToRetrieve = _instance.HasDataToRetrieve;

			// assert
			Assert.IsTrue(hasDataToRetrieve);
		}

		[Test]
		public void HasDataToRetrieve_ReturnsFalseOnRunningJob_WithLongMaxData()
		{
			// arrange
			SetupInitializationResultAndTestInstance(long.MaxValue);

			// act
			bool hasDataToRetrieve = _instance.HasDataToRetrieve;

			// assert
			Assert.IsFalse(hasDataToRetrieve);
		}

		[Test]
		public void HasDataToRetrieve_ReturnsFalseOnStoppedJob()
		{
			// arrange
			SetupInitializationResultAndTestInstance(long.MaxValue);

			_jobStopManager
				.Setup(x => x.IsStopRequested())
				.Returns(true);

			// act
			bool hasDataToRetrieve = _instance.HasDataToRetrieve;

			// assert
			Assert.IsFalse(hasDataToRetrieve);
		}

		[Test]
		public void RetrieveData_GoldFlow()
		{
			// Arrange
			SetupInitializationResultAndTestInstance(0);

			const int artifactID = 1111;
			var obj = new List<RelativityObjectSlimDto>
			{
				new RelativityObjectSlimDto(artifactID, _fieldValues)
			};

			_documentRepository
				.Setup(x => x.RetrieveResultsBlockFromExportAsync(_exportApiResult, 1, 0))
				.ReturnsAsync(obj);

			ArtifactDTO expectedDto = new ArtifactDTO(1111, 10, "Document", new[]
			{
				new ArtifactFieldDTO
				{
					ArtifactId = 123,
					Value = _CONTROL_NUMBER_VALUE,
					Name = _CONTROL_NUMBER,
					FieldType = FieldTypeHelper.FieldType.Empty
				},
				new ArtifactFieldDTO
				{
					ArtifactId = 456,
					Value = _EMAIL_VALUE,
					Name = _EMAIL,
					FieldType = FieldTypeHelper.FieldType.Empty
				},
			});

			_queryFieldLookupRepository
				.Setup(x => x.GetFieldByArtifactID(It.IsAny<int>()))
				.Returns(new ViewFieldInfo(string.Empty, string.Empty, FieldTypeHelper.FieldType.Empty));

			_queryFieldLookupRepository
				.Setup(x => x.GetFieldTypeByArtifactID(It.IsAny<int>()))
				.Returns(FieldTypeHelper.FieldType.Empty);

			// Act
			ArtifactDTO[] data = _instance.RetrieveData(1);

			// Assert
			Assert.NotNull(data);
			Assert.AreEqual(1, data.Length);
			ArtifactDTO artifact = data[0];

			ValidateArtifact(expectedDto, artifact);
		}

		[Test]
		public void RetrieveData_NoDataReturned()
		{
			// Arrange
			SetupInitializationResultAndTestInstance(0);
			_documentRepository
				.Setup(x => x.RetrieveResultsBlockFromExportAsync(_exportApiResult, 1, 0))
				.ReturnsAsync(new List<RelativityObjectSlimDto>());

			// Act
			ArtifactDTO[] data = _instance.RetrieveData(1);

			// Assert
			Assert.NotNull(data);
			Assert.AreEqual(0, data.Length);
		}

		[Test]
		public void RetrieveData_ShouldSetFolderPath()
		{
			// Arrange
			SetupInitializationResultAndTestInstance(0);
			_documentRepository
				.Setup(x => x.RetrieveResultsBlockFromExportAsync(_exportApiResult, 1, 0))
				.ReturnsAsync(new List<RelativityObjectSlimDto>());

			// Act
			_instance.RetrieveData(1);

			// Assert
			_folderPathReader
				.Verify(x => x.SetFolderPaths(It.IsAny<List<ArtifactDTO>>()), Times.Once);
		}

		private void SetupInitializationResultAndTestInstance(long recordCount)
		{
			string[] fieldNames = { _CONTROL_NUMBER, _EMAIL };
			_exportApiResult = new ExportInitializationResultsDto(
				new Guid("3A51AF56-0813-4E25-89DD-E08EC0C8526C"),
				recordCount,
				fieldNames);

			_documentRepository
				.Setup(x => x.InitializeSearchExportAsync(_SEARCH_ARTIFACT_ID, It.IsAny<int[]>(), _START_AT))
				.ReturnsAsync(_exportApiResult);
			_documentRepository
				.Setup(x => x.InitializeProductionExportAsync(_SEARCH_ARTIFACT_ID, It.IsAny<int[]>(), _START_AT))
				.ReturnsAsync(_exportApiResult);

			SourceConfiguration config = GetConfig(SourceConfiguration.ExportType.SavedSearch);

			_instance = new RelativityExporterService(
				_documentRepository.Object,
				_relativityObjectManager.Object,
				_repositoryFactoryMock.Object,
				_jobStopManager.Object,
				_helper.Object,
				_folderPathReader.Object,
				_fileRepository.Object,
				_serializer.Object,
				_exportDataSanitizer.Object,
				_mappedFields,
				_START_AT,
				config,
				_SEARCH_ARTIFACT_ID);
		}

		private static SourceConfiguration GetConfig(SourceConfiguration.ExportType exportType, int sourceProductionID = 0)
		{
			var sourceConfiguration = new SourceConfiguration()
			{
				SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ARTIFACT_ID,
				TargetWorkspaceArtifactId = _TARGET_WORKSPACE_ARTIFACT_ID,
				TypeOfExport = exportType,
				SourceProductionId = sourceProductionID
			};

			return sourceConfiguration;
		}

		private static void ValidateArtifact(ArtifactDTO expect, ArtifactDTO actual)
		{
			Assert.AreEqual(expect.ArtifactId, actual.ArtifactId);
			Assert.AreEqual(expect.ArtifactTypeId, actual.ArtifactTypeId);
			for (int i = 0; i < expect.Fields.Count; i++)
			{
				ArtifactFieldDTO expectedField = expect.Fields[i];
				ArtifactFieldDTO actualField = actual.Fields[i];

				Assert.AreEqual(expectedField.Name, actualField.Name);
				Assert.AreEqual(expectedField.Value, actualField.Value);
				Assert.AreEqual(expectedField.ArtifactId, actualField.ArtifactId);
				Assert.AreEqual(expectedField.FieldType, actualField.FieldType);
			}
		}
	}
}