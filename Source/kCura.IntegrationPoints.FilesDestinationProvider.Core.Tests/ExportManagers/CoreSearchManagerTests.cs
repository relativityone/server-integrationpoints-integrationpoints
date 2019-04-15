using System;
using System.Data;
using System.Linq;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.Core;
using Relativity.Services.FileField.Models;
using Relativity.Services.Interfaces.File.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Interfaces.ViewField.Models;
using Action = System.Action;
using ViewFieldInfo = kCura.WinEDDS.ViewFieldInfo;
using ColumnSourceType = Relativity.ViewFieldInfo.ColumnSourceType;
using FieldType = Relativity.FieldTypeHelper.FieldType;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.ExportManagers
{
	[TestFixture]
	public class CoreSearchManagerTests
	{
		private Mock<BaseServiceContext> _baseServiceContextMock;
		private Mock<IViewFieldRepository> _viewFieldRepositoryMock;
		private Mock<IFileRepository> _fileRepositoryMock;
		private Mock<IFileFieldRepository> _fileFieldRepositoryMock;

		private CoreSearchManager _sut;

		private readonly FileResponse[] _testFileResponses =
		{
			new FileResponse
			{
				DocumentArtifactID = 1700,
				Filename = "Filename12",
				Guid = "82644DB3-3865-4B99-9DB4-60CE40401BD1",
				Identifier = "Identifier22",
				Location = "Location33",
				Order = 0,
				Rotation = 1,
				Type = 2,
				InRepository = true,
				Size = 1234,
				Details = "Details232",
				Billable = true
			},
			new FileResponse
			{
				DocumentArtifactID = 17023,
				Filename = "Filename124",
				Guid = "82644DB1-3865-4B99-9DB4-60CE40401BD1",
				Identifier = "Identifier232",
				Location = "Location331",
				Order = 0,
				Rotation = 1,
				Type = 2,
				InRepository = true,
				Size = 1234,
				Details = "Details2325",
				Billable = true
			},
		};

		private readonly ProductionDocumentImageResponse[] _testProductionDocumentImageResponses =
		{
			new ProductionDocumentImageResponse
			{
				DocumentArtifactID = 1700,
				BatesNumber = "Bates123",
				Location = "Location33",
				ByteRange = 2,
				ImageFileName = "FileName1234",
				ImageGuid = "82644DB3-3865-4B99-9DB4-60CE40401BD1",
				ImageSize = 1234,
				NativeIdentifier = "NativeIdentifier888",
				PageID = 123,
				SourceGuid = "32644DB3-3865-4B99-9DB4-60CE40401BD1"
			},
			new ProductionDocumentImageResponse
			{
				DocumentArtifactID = 1702,
				BatesNumber = "Bates1233",
				Location = "Location313",
				ByteRange = 23,
				ImageFileName = "FileName12134",
				ImageGuid = "82644DB3-3865-4B99-9DB4-61CE40401BD1",
				ImageSize = 12234,
				NativeIdentifier = "NativeIdentifier1888",
				PageID = 1123,
				SourceGuid = "32644DB2-3865-4B99-9DB4-60CE40401BD1"
			},
		};

		private readonly DocumentImageResponse[] _testDocumentImageResponses =
		{
			new DocumentImageResponse
			{
				DocumentArtifactID = 1700,
				FileID = 12,
				FileName = "FileName123",
				Guid = "12644DB3-3865-4B99-9DB4-61CE40401BD1",
				Identifier = "Identifier234",
				Location = "Location22",
				Order = 1,
				Rotation = -1,
				Type = 2,
				InRepository = true,
				Size = 12344,
				Details = "Details999",
				Billable = true,
				PageID = 11,
				ByteRange = 4555
			},
			new DocumentImageResponse
			{
				DocumentArtifactID = 1701,
				FileID = 122,
				FileName = "FileName121",
				Guid = "11644DB3-3865-4B99-9DB4-61CE40401BD1",
				Identifier = "Identifier2341",
				Location = "Location221",
				Order = 12,
				Rotation = 0,
				Type = 3,
				InRepository = false,
				Size = 123441,
				Details = "Details9991",
				Billable = false,
				PageID = 121,
				ByteRange = 45155
			},
		};

		private readonly ExportProductionDocumentImageResponse[] _testExportProductionDocumentImageResponses =
		{
			new ExportProductionDocumentImageResponse
			{
				DocumentArtifactID = 1700,
				ProductionArtifactID = _PRODUCTION_ID,
				BatesNumber = "Bates123",
				Location = "Location33",
				ByteRange = 2,
				ImageFileName = "FileName1234",
				ImageGuid = "82644DB3-3865-4B99-9DB4-60CE40401BD1",
				ImageSize = 1234,
				PageID = 123,
				SourceGuid = "32644DB3-3865-4B99-9DB4-60CE40401BD1",
				Order = 1
			},
			new ExportProductionDocumentImageResponse
			{
				DocumentArtifactID = 1702,
				ProductionArtifactID = _PRODUCTION_ID_2,
				BatesNumber = "Bates1233",
				Location = "Location313",
				ByteRange = 23,
				ImageFileName = "FileName12134",
				ImageGuid = "82644DB3-3865-4B99-9DB4-61CE40401BD1",
				ImageSize = 12234,
				PageID = 1123,
				SourceGuid = "32644DB2-3865-4B99-9DB4-60CE40401BD1",
				Order = 2
			},
		};

		private readonly DynamicFileResponse[] _testDynamicFileResponses =
		{
			new DynamicFileResponse
			{
				FileID = 123,
				ObjectArtifactID = 12,
				FileName = "TestFileName1",
				Location = "Location11",
				Size = 23455,
			},
			new DynamicFileResponse
			{
				FileID = 321,
				ObjectArtifactID = 121,
				FileName = "TestFileName12",
				Location = "Location112",
				Size = 234551,
			}
		};

		private const int _WORKSPACE_ID = 1001000;
		private const int _PRODUCTION_ID = 1710;
		private const int _PRODUCTION_ID_2 = 1711;

		#region VIEW FIELD CONSTS

		private const int _ARTIFACT_ID = 1000100;
		private const int _ARTIFACT_ID_2 = 1002100;
		private const int _ARTIFACT_VIEW_FIELD_ID = 1000200;
		private const int _ARTIFACT_VIEW_FIELD_ID_2 = 1002200;
		private const FieldCategoryEnum _CATEGORY = FieldCategoryEnum.Batch;
		private const FieldCategory _CATEGORY_CONVERTED = FieldCategory.Batch;
		private const string _DISPLAY_NAME = "DisplayName";
		private const string _ARTIFACT_VIEW_FIELD_COLUMN_NAME = "AvfColumnName";
		private const string _ARTIFACT_VIEW_FIELD_HEADER_NAME = "AvfHeaderName";
		private const string _ALLOW_FIELD_NAME = "AllowFieldName";
		private const ColumnSourceTypeEnum _COLUMN_SOURCE_TYPE = ColumnSourceTypeEnum.Computed;
		private const ColumnSourceType _COLUMN_SOURCE_TYPE_CONVERTED = ColumnSourceType.Computed;
		private const string _DATA_SOURCE = "DataSource";
		private const string _SOURCE_FIELD_NAME = "SourceFieldName";
		private const int _SOURCE_FIELD_ARTIFACT_TYPE_ID = 10;
		private const int _SOURCE_FIELD_ARTIFACT_ID = 10004300;
		private const int _CONNECTOR_FIELD_ARTIFACT_ID = 1000400;
		private const string _SOURCE_FIELD_ARTIFACT_TYPE_TABLE_NAME = "SourceFieldArtifactTypeTableName";
		private const string _CONNECTOR_FIELD_NAME = "ConnectorFieldName";
		private const FieldCategoryEnum _CONNECTOR_FIELD_CATEGORY = FieldCategoryEnum.Comments;
		private const FieldCategory _CONNECTOR_FIELD_CATEGORY_CONVERTED = FieldCategory.Comments;
		private const FieldTypeEnum _FIELD_TYPE = FieldTypeEnum.Boolean;
		private const FieldType _FIELD_TYPE_CONVERTED = FieldType.Boolean;
		private const bool _IS_LINKED = false;
		private const int _FIELD_CODE_TYPE_ID = 11;
		private const int _ARTIFACT_TYPE_ID = 12;
		private const string _ARTIFACT_TYPE_TABLE_NAME = "ArtifactTypeTableName";
		private const bool _FIELD_IS_ARTIFACT_BASE_FIELD = true;
		private const string _FORMAT_STRING = "FormatString";
		private const bool _IS_UNICODE_ENABLED = false;
		private const bool _ALLOW_HTML = true;
		private const int _PARENT_FILE_FIELD_ARTIFACT_ID = 1000500;
		private const string _PARENT_FILE_FIELD_DISPLAY_NAME = "ParentFileFieldDisplayName";
		private const int _ASSOCIATIVE_ARTIFACT_TYPE_ID = 13;
		private const string _RELATIONAL_TABLE_NAME = "RelationalTableName";
		private const string _RELATIONAL_TABLE_COLUMN_NAME = "RelationalTableColumnName";
		private const string _RELATIONAL_TABLE_COLUMN_NAME_2 = "RelationalTableColumnName2";
		private const ParentReflectionTypeEnum _PARENT_REFLECTION_TYPE = ParentReflectionTypeEnum.GrandParent;
		private const ParentReflectionType _PARENT_REFLECTION_TYPE_CONVERTED = ParentReflectionType.GrandParent;
		private const string _REFLECTED_FIELD_ARTIFACT_TYPE_TABLE_NAME = "ReflectedFieldArtifactTypeTableName";
		private const string _REFLECTED_FIELD_IDENTIFIER_COLUMN_NAME = "ReflectedFieldIdentifierColumnName";
		private const string _REFLECTED_FIELD_CONNECTOR_FIELD_NAME = "ReflectedFieldConnectorFieldName";
		private const string _REFLECTED_CONNECTOR_IDENTIFIER_COLUMN_NAME = "ReflectedConnectorIdentifierColumnName";
		private const bool _ENABLE_DATA_GRID = false;
		private const bool _IS_VIRTUAL_ASSOCIATIVE_ARTIFACT_TYPE = true;

		#endregion VIEW FIELD CONSTS

		[SetUp]
		public void SetUp()
		{
			_baseServiceContextMock = new Mock<BaseServiceContext>();
			_viewFieldRepositoryMock = new Mock<IViewFieldRepository>();
			_fileFieldRepositoryMock = new Mock<IFileFieldRepository>();
			_fileRepositoryMock = new Mock<IFileRepository>();

			_sut = new CoreSearchManager(
				_baseServiceContextMock.Object,
				_fileRepositoryMock.Object,
				_fileFieldRepositoryMock.Object,
				_viewFieldRepositoryMock.Object
			);
		}

		[Test]
		public void RetrieveDefaultViewFieldIdsForSavedSearchTest()
		{
			// arrange
			ViewFieldIDResponse viewFieldIDResponse1 = CreateTestViewFieldIDResponse(_ARTIFACT_ID, _ARTIFACT_VIEW_FIELD_ID);
			ViewFieldIDResponse viewFieldIdResponse2 = CreateTestViewFieldIDResponse(_ARTIFACT_ID_2, _ARTIFACT_VIEW_FIELD_ID_2);
			ViewFieldIDResponse[] viewFieldIDResponseArray = { viewFieldIDResponse1, viewFieldIdResponse2 };
			_viewFieldRepositoryMock
				.Setup(x => x.ReadViewFieldIDsFromSearch(_WORKSPACE_ID, _ARTIFACT_TYPE_ID, _ARTIFACT_ID))
				.Returns(viewFieldIDResponseArray);

			// act
			int[] result = _sut.RetrieveDefaultViewFieldIds(
				_WORKSPACE_ID, 
				_ARTIFACT_ID, 
				_ARTIFACT_TYPE_ID, 
				isProduction: false
			);

			// assert
			_viewFieldRepositoryMock.Verify(
				x => x.ReadViewFieldIDsFromSearch(_WORKSPACE_ID, _ARTIFACT_TYPE_ID, _ARTIFACT_ID), Times.Once);
			result.Length.Should().Be(1);
			result[0].Should().Be(_ARTIFACT_VIEW_FIELD_ID);
		}

		[Test]
		public void RetrieveDefaultViewFieldIdsForProductionTest()
		{
			// arrange
			ViewFieldIDResponse viewFieldIDResponse1 = CreateTestViewFieldIDResponse(_ARTIFACT_ID, _ARTIFACT_VIEW_FIELD_ID);
			ViewFieldIDResponse viewFieldIdResponse2 = CreateTestViewFieldIDResponse(_ARTIFACT_ID_2, _ARTIFACT_VIEW_FIELD_ID_2);
			ViewFieldIDResponse[] viewFieldIDResponseArray = { viewFieldIDResponse1, viewFieldIdResponse2 };
			_viewFieldRepositoryMock
				.Setup(x => x.ReadViewFieldIDsFromProduction(_WORKSPACE_ID, _ARTIFACT_TYPE_ID, _ARTIFACT_ID))
				.Returns(viewFieldIDResponseArray);

			// act
			int[] result = _sut.RetrieveDefaultViewFieldIds(
				_WORKSPACE_ID, 
				_ARTIFACT_ID, 
				_ARTIFACT_TYPE_ID, 
				isProduction: true
			);

			// assert
			_viewFieldRepositoryMock.Verify(
				x => x.ReadViewFieldIDsFromProduction(
					_WORKSPACE_ID, 
					_ARTIFACT_TYPE_ID, 
					_ARTIFACT_ID
				), Times.Once);
			result.Length.Should().Be(1);
			result[0].Should().Be(_ARTIFACT_VIEW_FIELD_ID);
		}

		[Test]
		public void RetrieveAllExportableViewFieldsTest()
		{
			// arrange
			ViewFieldResponse viewFieldResponse = CreateTestViewFieldResponse();
			ViewFieldResponse[] viewFieldResponseArray = {viewFieldResponse};
			_viewFieldRepositoryMock.Setup(x => x.ReadExportableViewFields(_WORKSPACE_ID, _ARTIFACT_TYPE_ID))
				.Returns(viewFieldResponseArray);

			// act
			ViewFieldInfo[] result = _sut.RetrieveAllExportableViewFields(
				_WORKSPACE_ID, 
				_ARTIFACT_TYPE_ID
			);

			// assert
			_viewFieldRepositoryMock.Verify(
				x => x.ReadExportableViewFields(
					_WORKSPACE_ID, 
					_ARTIFACT_TYPE_ID
				), Times.Once);
			result.Length.Should().Be(1);
			AssertViewFieldInfoAreSameAsExpected(result[0]);
		}

		[Test]
		public void RetrieveNativesForSearch_ShouldReturnResponsesWhenCorrectDocumentIDsPassed()
		{
			//arrange
			int[] documentIDs = _testFileResponses.Select(x => x.DocumentArtifactID).ToArray();
			string documentIDsAsString = ConvertToCommaSeparatedString(documentIDs);
			_fileRepositoryMock
				.Setup(x => x.GetNativesForSearch(_WORKSPACE_ID, documentIDs))
				.Returns(_testFileResponses);
			
			//act
			DataSet result = _sut.RetrieveNativesForSearch(
				_WORKSPACE_ID, 
				documentIDsAsString
			);
			
			//assert
			_fileRepositoryMock.Verify(
				x => x.GetNativesForSearch(_WORKSPACE_ID, documentIDs), 
				Times.Once
			);
			AssertFileResponsesAreSameAsExpected(result, _testFileResponses);
		}

		[Test]
		[TestCase("")]
		[TestCase(null)]
		public void RetrieveNativesForSearch_ShouldThrowWhenInvalidDocumentIDsPassed(string documentIDsAsString)
		{
			//act
			Action action = () => _sut.RetrieveNativesForSearch(
				_WORKSPACE_ID,
				documentArtifactIDs: documentIDsAsString
			);

			//assert
			_fileRepositoryMock.Verify(
				x => x.GetNativesForSearch(It.IsAny<int>(), It.IsAny<int[]>()),
				Times.Never
			);
			action.ShouldThrow<ArgumentException>()
				.WithMessage("Invalid documentArtifactIDs");
		}

		[Test]
		public void RetrieveNativesForProduction_ShouldReturnResponsesWhenCorrectDocumentIDsPassed()
		{
			//arrange
			int[] documentIDs = _testFileResponses.Select(x => x.DocumentArtifactID).ToArray();
			string documentIDsAsString = ConvertToCommaSeparatedString(documentIDs);
			_fileRepositoryMock
				.Setup(x => x.GetNativesForProduction(_WORKSPACE_ID, _PRODUCTION_ID, documentIDs))
				.Returns(_testFileResponses);

			//act
			DataSet result = _sut.RetrieveNativesForProduction(
				_WORKSPACE_ID,
				_PRODUCTION_ID,
				documentIDsAsString
			);

			//assert
			_fileRepositoryMock.Verify(
				x => x.GetNativesForProduction(
					_WORKSPACE_ID,
					_PRODUCTION_ID, 
					documentIDs), Times.Once);
			AssertFileResponsesAreSameAsExpected(result, _testFileResponses);
		}

		[Test]
		[TestCase("")]
		[TestCase(null)]
		public void RetrieveNativesForProduction_ShouldThrowWhenInvalidDocumentIDsPassed(string documentIDsAsString)
		{
			//act
			Action action = () => _sut.RetrieveNativesForProduction(
				_WORKSPACE_ID,
				_PRODUCTION_ID,
				documentArtifactIDs: documentIDsAsString
			);

			//assert
			_fileRepositoryMock.Verify(
				x => x.GetNativesForProduction(
					It.IsAny<int>(), 
					It.IsAny<int>(), 
					It.IsAny<int[]>()),
				Times.Never
			);
			action.ShouldThrow<ArgumentException>()
				.WithMessage("Invalid documentArtifactIDs");
		}

		[Test]
		public void RetrieveProducedImagesForDocument_ShouldReturnResponsesWhenCorrectDocumentIDPassed()
		{
			//arrange
			FileResponse[] testFileResponses = _testFileResponses.Take(1).ToArray();
			int documentID = testFileResponses.First().DocumentArtifactID;
			_fileRepositoryMock
				.Setup(x => x.GetProducedImagesForDocument(_WORKSPACE_ID, documentID))
				.Returns(testFileResponses);

			//act
			DataSet result = _sut.RetrieveProducedImagesForDocument(
				_WORKSPACE_ID,
				documentID
			);

			//assert
			_fileRepositoryMock.Verify(
				x => x.GetProducedImagesForDocument(
					_WORKSPACE_ID,
					documentID), Times.Once);
			AssertFileResponsesAreSameAsExpected(result, testFileResponses);
		}

		[Test]
		public void RetrieveImagesForProductionDocuments_ShouldReturnResponsesWhenCorrectDocumentIDsPassed()
		{
			//arrange
			int[] documentIDs = _testProductionDocumentImageResponses
				.Select(x => x.DocumentArtifactID)
				.ToArray();
			_fileRepositoryMock
				.Setup(x => x.GetImagesForProductionDocuments(
					_WORKSPACE_ID, _PRODUCTION_ID, documentIDs))
				.Returns(_testProductionDocumentImageResponses);

			//act
			DataSet result = _sut.RetrieveImagesForProductionDocuments(
				_WORKSPACE_ID,
				documentIDs,
				_PRODUCTION_ID
			);

			//assert
			_fileRepositoryMock.Verify(
				x => x.GetImagesForProductionDocuments(
					_WORKSPACE_ID,
					_PRODUCTION_ID,
					documentIDs), Times.Once);
			AssertProductionDocumentImageResponsesAreSameAsExpected(
				result,
				_testProductionDocumentImageResponses
			);
		}

		[Test]
		public void RetrieveImagesForProductionDocuments_ShouldNotThrowWhenNullPassedAsDocumentIDs()
		{
			//act
			Action action = () => _sut.RetrieveImagesForProductionDocuments(
				_WORKSPACE_ID,
				documentArtifactIDs: null,
				productionArtifactID: _PRODUCTION_ID
			);

			//assert
			action.ShouldNotThrow();
			_fileRepositoryMock.Verify(
				x => x.GetImagesForProductionDocuments(
					_WORKSPACE_ID,
					_PRODUCTION_ID,
					null), 
				Times.Once);
		}

		[Test]
		public void RetrieveImagesForDocuments_ShouldReturnResponsesWhenCorrectDocumentIDsPassed()
		{
			//arrange
			int[] documentIDs = _testDocumentImageResponses
				.Select(x => x.DocumentArtifactID)
				.ToArray();
			_fileRepositoryMock
				.Setup(x => x.GetImagesForDocuments(_WORKSPACE_ID, documentIDs))
				.Returns(_testDocumentImageResponses);

			//act
			DataSet result = _sut.RetrieveImagesForDocuments(
				_WORKSPACE_ID,
				documentIDs
			);

			//assert
			_fileRepositoryMock.Verify(
				x => x.GetImagesForDocuments(_WORKSPACE_ID, documentIDs), 
				Times.Once
			);
			AssertDocumentImageResponsesAreSameAsExpected(
				result,
				_testDocumentImageResponses
			);
		}

		[Test]
		public void RetrieveImagesForDocuments_ShouldNotThrowWhenNullPassedAsDocumentIDs()
		{
			//act
			Action action = () => _sut.RetrieveImagesForDocuments(
				_WORKSPACE_ID,
				documentArtifactIDs: null
			);

			//assert
			action.ShouldNotThrow();
			_fileRepositoryMock.Verify(
				x => x.GetImagesForDocuments(_WORKSPACE_ID, null),
				Times.Once);
		}

		[Test]
		public void RetrieveImagesByProductionIDsAndDocumentIDsForExport_ShouldReturnResponsesWhenCorrectDocumentIDsAndProductionIDsPassed()
		{
			//arrange
			int[] documentIDs = _testExportProductionDocumentImageResponses
				.Select(x => x.DocumentArtifactID)
				.ToArray();
			int[] productionIDs = _testExportProductionDocumentImageResponses
				.Select(x => x.ProductionArtifactID)
				.ToArray();
			_fileRepositoryMock
				.Setup(x => x.GetImagesForExport(_WORKSPACE_ID, productionIDs, documentIDs))
				.Returns(_testExportProductionDocumentImageResponses);

			//act
			DataSet result = _sut.RetrieveImagesByProductionIDsAndDocumentIDsForExport(
				_WORKSPACE_ID,
				productionIDs,
				documentIDs
			);

			//assert
			_fileRepositoryMock.Verify(
				x => x.GetImagesForExport(_WORKSPACE_ID, productionIDs, documentIDs),
				Times.Once
			);
			AssertExportProductionDocumentImageResponsesAreSameAsExpected(
				result,
				_testExportProductionDocumentImageResponses
			);
		}

		[Test]
		public void RetrieveImagesByProductionIDsAndDocumentIDsForExport_ShouldNotThrowWhenNullPassedAsDocumentIDs()
		{
			//arrange
			int[] productionIDs = _testExportProductionDocumentImageResponses
				.Select(x => x.ProductionArtifactID)
				.ToArray();

			//act
			Action action = () => _sut.RetrieveImagesByProductionIDsAndDocumentIDsForExport(
				_WORKSPACE_ID,
				productionIDs,
				documentArtifactIDs: null
			);

			//assert
			action.ShouldNotThrow();
			_fileRepositoryMock.Verify(
				x => x.GetImagesForExport(_WORKSPACE_ID, productionIDs, null),
				Times.Once);
		}

		[Test]
		public void RetrieveImagesByProductionIDsAndDocumentIDsForExport_ShouldNotThrowWhenNullPassedAsProductionIDs()
		{
			//arrange
			int[] documentIDs = _testExportProductionDocumentImageResponses
				.Select(x => x.DocumentArtifactID)
				.ToArray();

			//act
			Action action = () => _sut.RetrieveImagesByProductionIDsAndDocumentIDsForExport(
				_WORKSPACE_ID,
				productionArtifactIDs: null,
				documentArtifactIDs: documentIDs
			);

			//assert
			action.ShouldNotThrow();
			_fileRepositoryMock.Verify(
				x => x.GetImagesForExport(_WORKSPACE_ID, null, documentIDs),
				Times.Once);
		}

		[Test]
		public void RetrieveFilesForDynamicObjects_ShouldReturnResponsesWhenCorrectObjectIDsPassed()
		{
			//arrange
			const int fileFieldID = 111;
			int[] objectIDs = _testDynamicFileResponses
				.Select(x => x.ObjectArtifactID)
				.ToArray();
			_fileFieldRepositoryMock
				.Setup(x => x.GetFilesForDynamicObjectsAsync(_WORKSPACE_ID, fileFieldID, objectIDs))
				.Returns(_testDynamicFileResponses);

			//act
			DataSet result = _sut.RetrieveFilesForDynamicObjects(
				_WORKSPACE_ID,
				fileFieldID,
				objectIDs
			);

			//assert
			_fileFieldRepositoryMock.Verify(
				x => x.GetFilesForDynamicObjectsAsync(_WORKSPACE_ID, fileFieldID, objectIDs),
				Times.Once
			);
			AssertDynamicFileResponsesAreSameAsExpected(
				result,
				_testDynamicFileResponses
			);
		}

		[Test]
		public void RetrieveFilesForDynamicObjects_ShouldNotThrowWhenNullPassedAsObjectIDs()
		{
			//arrange
			const int fileFieldID = 111;

			//act
			Action action = () => _sut.RetrieveFilesForDynamicObjects(
				_WORKSPACE_ID,
				fileFieldID,
				objectIds: null
			);

			//assert
			action.ShouldNotThrow();
			_fileFieldRepositoryMock.Verify(
				x => x.GetFilesForDynamicObjectsAsync(_WORKSPACE_ID, fileFieldID, null),
				Times.Once);
		}

		private static ViewFieldIDResponse CreateTestViewFieldIDResponse(int artifactID, int artifactViewFieldID)
		{
			var viewFieldIDResponse = new ViewFieldIDResponse
			{
				ArtifactID = artifactID,
				ArtifactViewFieldID = artifactViewFieldID
			};
			return viewFieldIDResponse;
		}

		private static ViewFieldResponse CreateTestViewFieldResponse()
		{
			var viewFieldResponse = new ViewFieldResponse
			{
				ArtifactID = _ARTIFACT_ID,
				ArtifactViewFieldID = _ARTIFACT_VIEW_FIELD_ID,
				Category = _CATEGORY,
				DisplayName = _DISPLAY_NAME,
				ArtifactViewFieldColumnName = _ARTIFACT_VIEW_FIELD_COLUMN_NAME,
				ArtifactViewFieldHeaderName = _ARTIFACT_VIEW_FIELD_HEADER_NAME,
				AllowFieldName = _ALLOW_FIELD_NAME,
				ColumnSourceType = _COLUMN_SOURCE_TYPE,
				DataSource = _DATA_SOURCE,
				SourceFieldName = _SOURCE_FIELD_NAME,
				SourceFieldArtifactTypeID = _SOURCE_FIELD_ARTIFACT_TYPE_ID,
				SourceFieldArtifactID = _SOURCE_FIELD_ARTIFACT_ID,
				ConnectorFieldArtifactID = _CONNECTOR_FIELD_ARTIFACT_ID,
				SourceFieldArtifactTypeTableName = _SOURCE_FIELD_ARTIFACT_TYPE_TABLE_NAME,
				ConnectorFieldName = _CONNECTOR_FIELD_NAME,
				ConnectorFieldCategory = _CONNECTOR_FIELD_CATEGORY,
				FieldType = _FIELD_TYPE,
				IsLinked = _IS_LINKED,
				FieldCodeTypeID = _FIELD_CODE_TYPE_ID,
				ArtifactTypeID = _ARTIFACT_TYPE_ID,
				ArtifactTypeTableName = _ARTIFACT_TYPE_TABLE_NAME,
				FieldIsArtifactBaseField = _FIELD_IS_ARTIFACT_BASE_FIELD,
				FormatString = _FORMAT_STRING,
				IsUnicodeEnabled = _IS_UNICODE_ENABLED,
				AllowHtml = _ALLOW_HTML,
				ParentFileFieldArtifactID = _PARENT_FILE_FIELD_ARTIFACT_ID,
				ParentFileFieldDisplayName = _PARENT_FILE_FIELD_DISPLAY_NAME,
				AssociativeArtifactTypeID = _ASSOCIATIVE_ARTIFACT_TYPE_ID,
				RelationalTableName = _RELATIONAL_TABLE_NAME,
				RelationalTableColumnName = _RELATIONAL_TABLE_COLUMN_NAME,
				RelationalTableColumnName2 = _RELATIONAL_TABLE_COLUMN_NAME_2,
				ParentReflectionType = _PARENT_REFLECTION_TYPE,
				ReflectedFieldArtifactTypeTableName = _REFLECTED_FIELD_ARTIFACT_TYPE_TABLE_NAME,
				ReflectedFieldIdentifierColumnName = _REFLECTED_FIELD_IDENTIFIER_COLUMN_NAME,
				ReflectedFieldConnectorFieldName = _REFLECTED_FIELD_CONNECTOR_FIELD_NAME,
				ReflectedConnectorIdentifierColumnName = _REFLECTED_CONNECTOR_IDENTIFIER_COLUMN_NAME,
				EnableDataGrid = _ENABLE_DATA_GRID,
				IsVirtualAssociativeArtifactType = _IS_VIRTUAL_ASSOCIATIVE_ARTIFACT_TYPE
			};
			return viewFieldResponse;
		}

		private static string ConvertToCommaSeparatedString(int[] values) => string.Join(",", values);

		private static void AssertViewFieldInfoAreSameAsExpected(ViewFieldInfo viewFieldInfo)
		{
			viewFieldInfo.FieldArtifactId.Should().Be(_ARTIFACT_ID);
			viewFieldInfo.AvfId.Should().Be(_ARTIFACT_VIEW_FIELD_ID);
			viewFieldInfo.Category.Should().Be(_CATEGORY_CONVERTED);
			viewFieldInfo.DisplayName.Should().Be(_DISPLAY_NAME);
			viewFieldInfo.AvfColumnName.Should().Be(_ARTIFACT_VIEW_FIELD_COLUMN_NAME);
			viewFieldInfo.AvfHeaderName.Should().Be(_ARTIFACT_VIEW_FIELD_HEADER_NAME);
			viewFieldInfo.AllowFieldName.Should().Be(_ALLOW_FIELD_NAME);
			viewFieldInfo.ColumnSource.Should().Be(_COLUMN_SOURCE_TYPE_CONVERTED);
			viewFieldInfo.DataSource.Should().Be(_DATA_SOURCE);
			viewFieldInfo.SourceFieldName.Should().Be(_SOURCE_FIELD_NAME);
			viewFieldInfo.SourceFieldArtifactTypeID.Should().Be(_SOURCE_FIELD_ARTIFACT_TYPE_ID);
			viewFieldInfo.SourceFieldArtifactID.Should().Be(_SOURCE_FIELD_ARTIFACT_ID);
			viewFieldInfo.ConnectorFieldArtifactID.Should().Be(_CONNECTOR_FIELD_ARTIFACT_ID);
			viewFieldInfo.SourceFieldArtifactTypeTableName.Should().Be(_SOURCE_FIELD_ARTIFACT_TYPE_TABLE_NAME);
			viewFieldInfo.ConnectorFieldName.Should().Be(_CONNECTOR_FIELD_NAME);
			viewFieldInfo.ConnectorFieldCategory.Should().Be(_CONNECTOR_FIELD_CATEGORY_CONVERTED);
			viewFieldInfo.FieldType.Should().Be(_FIELD_TYPE_CONVERTED);
			viewFieldInfo.IsLinked.Should().Be(_IS_LINKED);
			viewFieldInfo.FieldCodeTypeID.Should().Be(_FIELD_CODE_TYPE_ID);
			viewFieldInfo.ArtifactTypeID.Should().Be(_ARTIFACT_TYPE_ID);
			viewFieldInfo.ArtifactTypeTableName.Should().Be(_ARTIFACT_TYPE_TABLE_NAME);
			viewFieldInfo.FieldIsArtifactBaseField.Should().Be(_FIELD_IS_ARTIFACT_BASE_FIELD);
			viewFieldInfo.FormatString.Should().Be(_FORMAT_STRING);
			viewFieldInfo.IsUnicodeEnabled.Should().Be(_IS_UNICODE_ENABLED);
			viewFieldInfo.AllowHtml.Should().Be(_ALLOW_HTML);
			viewFieldInfo.ParentFileFieldArtifactID.Should().Be(_PARENT_FILE_FIELD_ARTIFACT_ID);
			viewFieldInfo.ParentFileFieldDisplayName.Should().Be(_PARENT_FILE_FIELD_DISPLAY_NAME);
			viewFieldInfo.AssociativeArtifactTypeID.Should().Be(_ASSOCIATIVE_ARTIFACT_TYPE_ID);
			viewFieldInfo.RelationalTableName.Should().Be(_RELATIONAL_TABLE_NAME);
			viewFieldInfo.RelationalTableColumnName.Should().Be(_RELATIONAL_TABLE_COLUMN_NAME);
			viewFieldInfo.RelationalTableColumnName2.Should().Be(_RELATIONAL_TABLE_COLUMN_NAME_2);
			viewFieldInfo.ParentReflectionType.Should().Be(_PARENT_REFLECTION_TYPE_CONVERTED);
			viewFieldInfo.ReflectedFieldArtifactTypeTableName.Should().Be(_REFLECTED_FIELD_ARTIFACT_TYPE_TABLE_NAME);
			viewFieldInfo.ReflectedFieldIdentifierColumnName.Should().Be(_REFLECTED_FIELD_IDENTIFIER_COLUMN_NAME);
			viewFieldInfo.ReflectedFieldConnectorFieldName.Should().Be(_REFLECTED_FIELD_CONNECTOR_FIELD_NAME);
			viewFieldInfo.ReflectedConnectorIdentifierColumnName.Should().Be(_REFLECTED_CONNECTOR_IDENTIFIER_COLUMN_NAME);
			viewFieldInfo.EnableDataGrid.Should().Be(_ENABLE_DATA_GRID);
			viewFieldInfo.IsVirtualAssociativeArtifactType.Should().Be(_IS_VIRTUAL_ASSOCIATIVE_ARTIFACT_TYPE);
		}

		private static void AssertFileResponsesAreSameAsExpected(
			DataSet dataSet, 
			FileResponse[] responses)
		{
			DataRow[] rows = dataSet.Tables[0].Rows.Cast<DataRow>().ToArray();

			rows.Length.Should().Be(responses.Length);

			var asserts = responses.Zip(rows, (response, actual) => new
			{
				Expected = response,
				Actual = actual
			});

			foreach (var assert in asserts)
			{
				DataRow actual = assert.Actual;
				FileResponse expected = assert.Expected;

				actual[nameof(FileResponse.DocumentArtifactID)].Should().Be(expected.DocumentArtifactID);
				actual[nameof(FileResponse.Filename)].Should().Be(expected.Filename);
				actual[nameof(FileResponse.Guid)].Should().Be(expected.Guid);
				actual[nameof(FileResponse.Identifier)].Should().Be(expected.Identifier);
				actual[nameof(FileResponse.Location)].Should().Be(expected.Location);
				actual[nameof(FileResponse.Order)].Should().Be(expected.Order);
				actual[nameof(FileResponse.Rotation)].Should().Be(expected.Rotation);
				actual[nameof(FileResponse.Type)].Should().Be(expected.Type);
				actual[nameof(FileResponse.InRepository)].Should().Be(expected.InRepository);
				actual[nameof(FileResponse.Size)].Should().Be(expected.Size);
				actual[nameof(FileResponse.Details)].Should().Be(expected.Details);
				actual[nameof(FileResponse.Billable)].Should().Be(expected.Billable);
			}
		}

		private void AssertProductionDocumentImageResponsesAreSameAsExpected(
			DataSet dataSet, 
			ProductionDocumentImageResponse[] responses)
		{
			DataRow[] rows = dataSet.Tables[0].Rows.Cast<DataRow>().ToArray();

			rows.Length.Should().Be(responses.Length);

			var asserts = responses.Zip(rows, (response, actual) => new
			{
				Expected = response,
				Actual = actual
			});

			foreach (var assert in asserts)
			{
				DataRow actual = assert.Actual;
				ProductionDocumentImageResponse expected = assert.Expected;

				actual[nameof(ProductionDocumentImageResponse.DocumentArtifactID)]
					.Should().Be(expected.DocumentArtifactID);
				actual[nameof(ProductionDocumentImageResponse.SourceGuid)]
					.Should().Be(expected.SourceGuid);
				actual[nameof(ProductionDocumentImageResponse.BatesNumber)]
					.Should().Be(expected.BatesNumber);
				actual[nameof(ProductionDocumentImageResponse.ImageSize)]
					.Should().Be(expected.ImageSize);
				actual[nameof(ProductionDocumentImageResponse.ImageGuid)]
					.Should().Be(expected.ImageGuid);
				actual[nameof(ProductionDocumentImageResponse.ImageFileName)]
					.Should().Be(expected.ImageFileName);
				actual[nameof(ProductionDocumentImageResponse.Location)]
					.Should().Be(expected.Location);
				actual[nameof(ProductionDocumentImageResponse.PageID)]
					.Should().Be(expected.PageID);
				actual[nameof(ProductionDocumentImageResponse.ByteRange)]
					.Should().Be(expected.ByteRange);
				actual[nameof(ProductionDocumentImageResponse.NativeIdentifier)]
					.Should().Be(expected.NativeIdentifier);
			}
		}

		private void AssertDocumentImageResponsesAreSameAsExpected(
			DataSet dataSet, 
			DocumentImageResponse[] responses)
		{
			DataRow[] rows = dataSet.Tables[0].Rows.Cast<DataRow>().ToArray();

			rows.Length.Should().Be(responses.Length);

			var asserts = responses.Zip(rows, (response, actual) => new
			{
				Expected = response,
				Actual = actual
			});

			foreach (var assert in asserts)
			{
				DataRow actual = assert.Actual;
				DocumentImageResponse expected = assert.Expected;

				actual[nameof(DocumentImageResponse.DocumentArtifactID)]
					.Should().Be(expected.DocumentArtifactID);
				actual[nameof(DocumentImageResponse.FileID)]
					.Should().Be(expected.FileID);
				actual["Filename"] //Response model differs from Db model here
					.Should().Be(expected.FileName);
				actual[nameof(DocumentImageResponse.Guid)]
					.Should().Be(expected.Guid);
				actual[nameof(DocumentImageResponse.Identifier)]
					.Should().Be(expected.Identifier);
				actual[nameof(DocumentImageResponse.Location)]
					.Should().Be(expected.Location);
				actual[nameof(DocumentImageResponse.Order)]
					.Should().Be(expected.Order);
				actual[nameof(DocumentImageResponse.Rotation)]
					.Should().Be(expected.Rotation);
				actual[nameof(DocumentImageResponse.Type)]
					.Should().Be(expected.Type);
				actual[nameof(DocumentImageResponse.InRepository)]
					.Should().Be(expected.InRepository);
				actual[nameof(DocumentImageResponse.Size)]
					.Should().Be(expected.Size);
				actual[nameof(DocumentImageResponse.Details)]
					.Should().Be(expected.Details);
				actual[nameof(DocumentImageResponse.Billable)]
					.Should().Be(expected.Billable);
				actual[nameof(DocumentImageResponse.PageID)]
					.Should().Be(expected.PageID);
				actual[nameof(DocumentImageResponse.ByteRange)]
					.Should().Be(expected.ByteRange);
			}
		}

		private void AssertExportProductionDocumentImageResponsesAreSameAsExpected(
			DataSet dataSet, 
			ExportProductionDocumentImageResponse[] responses)
		{
			DataRow[] rows = dataSet.Tables[0].Rows.Cast<DataRow>().ToArray();

			rows.Length.Should().Be(responses.Length);

			var asserts = responses.Zip(rows, (response, actual) => new
			{
				Expected = response,
				Actual = actual
			});

			foreach (var assert in asserts)
			{
				DataRow actual = assert.Actual;
				ExportProductionDocumentImageResponse expected = assert.Expected;

				actual[nameof(ExportProductionDocumentImageResponse.DocumentArtifactID)]
					.Should().Be(expected.DocumentArtifactID);
				actual[nameof(ExportProductionDocumentImageResponse.ProductionArtifactID)]
					.Should().Be(expected.ProductionArtifactID);
				actual[nameof(ExportProductionDocumentImageResponse.SourceGuid)]
					.Should().Be(expected.SourceGuid);
				actual[nameof(ExportProductionDocumentImageResponse.BatesNumber)]
					.Should().Be(expected.BatesNumber);
				actual[nameof(ExportProductionDocumentImageResponse.ImageSize)]
					.Should().Be(expected.ImageSize);
				actual[nameof(ExportProductionDocumentImageResponse.ImageGuid)]
					.Should().Be(expected.ImageGuid);
				actual[nameof(ExportProductionDocumentImageResponse.ImageFileName)]
					.Should().Be(expected.ImageFileName);
				actual[nameof(ExportProductionDocumentImageResponse.Location)]
					.Should().Be(expected.Location);
				actual[nameof(ExportProductionDocumentImageResponse.PageID)]
					.Should().Be(expected.PageID);
				actual[nameof(ExportProductionDocumentImageResponse.ByteRange)]
					.Should().Be(expected.ByteRange);
				actual[nameof(ExportProductionDocumentImageResponse.Order)]
					.Should().Be(expected.Order);
			}
		}

		private void AssertDynamicFileResponsesAreSameAsExpected(
			DataSet dataSet,
			DynamicFileResponse[] responses)
		{
			DataRow[] rows = dataSet.Tables[0].Rows.Cast<DataRow>().ToArray();

			rows.Length.Should().Be(responses.Length);

			var asserts = responses.Zip(rows, (response, actual) => new
			{
				Expected = response,
				Actual = actual
			});

			foreach (var assert in asserts)
			{
				DataRow actual = assert.Actual;
				DynamicFileResponse expected = assert.Expected;

				actual[nameof(DynamicFileResponse.FileID)]
					.Should().Be(expected.FileID);
				actual[nameof(DynamicFileResponse.ObjectArtifactID)]
					.Should().Be(expected.ObjectArtifactID);
				actual[nameof(DynamicFileResponse.FileName)]
					.Should().Be(expected.FileName);
				actual[nameof(DynamicFileResponse.Location)]
					.Should().Be(expected.Location);
				actual[nameof(DynamicFileResponse.Size)]
					.Should().Be(expected.Size);
			}
		}
	}
}
