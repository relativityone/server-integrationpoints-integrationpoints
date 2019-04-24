using System;
using System.Data;
using System.Linq;
using FluentAssertions;
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

		private readonly ViewFieldIDResponse[] _testViewFieldIDResponses =
		{
			new ViewFieldIDResponse
			{
				ArtifactID = 1000100,
				ArtifactViewFieldID = 1000200
			},
			new ViewFieldIDResponse
			{
				ArtifactID = 1002100,
				ArtifactViewFieldID = 1002200
			}
		};

		private readonly ViewFieldResponse[] _testViewFieldResponses =
		{
			new ViewFieldResponse
			{
				ArtifactID = 121,
				ArtifactViewFieldID = 133,
				Category = FieldCategoryEnum.Batch,
				DisplayName = "DisplayName",
				ArtifactViewFieldColumnName = "AvfColumnName",
				ArtifactViewFieldHeaderName = "AvfHeaderName",
				AllowFieldName = "AllowFieldName",
				ColumnSourceType = ColumnSourceTypeEnum.Computed,
				DataSource = "DataSource",
				SourceFieldName = "SourceFieldName",
				SourceFieldArtifactTypeID = 10,
				SourceFieldArtifactID = 10004300,
				ConnectorFieldArtifactID = 1000400,
				SourceFieldArtifactTypeTableName = "SourceFieldArtifactTypeTableName",
				ConnectorFieldName = "ConnectorFieldName",
				ConnectorFieldCategory = FieldCategoryEnum.Comments,
				FieldType = FieldTypeEnum.Boolean,
				IsLinked = false,
				FieldCodeTypeID = 11,
				ArtifactTypeID = 12,
				ArtifactTypeTableName = "ArtifactTypeTableName",
				FieldIsArtifactBaseField = true,
				FormatString = "FormatString",
				IsUnicodeEnabled = false,
				AllowHtml = true,
				ParentFileFieldArtifactID = 1000500,
				ParentFileFieldDisplayName = "ParentFileFieldDisplayName",
				AssociativeArtifactTypeID = 13,
				RelationalTableName = "RelationalTableName",
				RelationalTableColumnName = "RelationalTableColumnName",
				RelationalTableColumnName2 = "RelationalTableColumnName2",
				ParentReflectionType = ParentReflectionTypeEnum.GrandParent,
				ReflectedFieldArtifactTypeTableName = "ReflectedFieldArtifactTypeTableName",
				ReflectedFieldIdentifierColumnName = "ReflectedFieldIdentifierColumnName",
				ReflectedFieldConnectorFieldName = "ReflectedFieldConnectorFieldName",
				ReflectedConnectorIdentifierColumnName = "ReflectedConnectorIdentifierColumnName",
				EnableDataGrid = false,
				IsVirtualAssociativeArtifactType = true
			},
			new ViewFieldResponse
			{
				ArtifactID = 1211,
				ArtifactViewFieldID = 1332,
				Category = FieldCategoryEnum.Batch,
				DisplayName = "DisplayName1",
				ArtifactViewFieldColumnName = "AvfColumnName1",
				ArtifactViewFieldHeaderName = "AvfHeaderName1",
				AllowFieldName = "AllowFieldName2",
				ColumnSourceType = ColumnSourceTypeEnum.Computed,
				DataSource = "DataSource2",
				SourceFieldName = "SourceFieldName2",
				SourceFieldArtifactTypeID = 101,
				SourceFieldArtifactID = 100043001,
				ConnectorFieldArtifactID = 10004001,
				SourceFieldArtifactTypeTableName = "SourceFieldArtifactTypeTableName1",
				ConnectorFieldName = "ConnectorFieldName",
				ConnectorFieldCategory = FieldCategoryEnum.Comments,
				FieldType = FieldTypeEnum.Boolean,
				IsLinked = true,
				FieldCodeTypeID = 111,
				ArtifactTypeID = 112,
				ArtifactTypeTableName = "ArtifactTypeTableName1",
				FieldIsArtifactBaseField = true,
				FormatString = "FormatString1",
				IsUnicodeEnabled = false,
				AllowHtml = true,
				ParentFileFieldArtifactID = 10005020,
				ParentFileFieldDisplayName = "ParentFileFieldDisplayName1",
				AssociativeArtifactTypeID = 131,
				RelationalTableName = "RelationalTableName1",
				RelationalTableColumnName = "RelationalTableColumnName1",
				RelationalTableColumnName2 = "RelationalTableColumnName21",
				ParentReflectionType = ParentReflectionTypeEnum.GrandParent,
				ReflectedFieldArtifactTypeTableName = "ReflectedFieldArtifactTypeTableName1",
				ReflectedFieldIdentifierColumnName = "ReflectedFieldIdentifierColumnName1",
				ReflectedFieldConnectorFieldName = "ReflectedFieldConnectorFieldName1",
				ReflectedConnectorIdentifierColumnName = "ReflectedConnectorIdentifierColumnName1",
				EnableDataGrid = true,
				IsVirtualAssociativeArtifactType = false
			}
		};

		private const int _WORKSPACE_ID = 1001000;
		private const int _PRODUCTION_ID = 1710;
		private const int _PRODUCTION_ID_2 = 1711;

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
		public void RetrieveDefaultViewFieldIdsForSavedSearch_ShouldReturnViewFieldIDForSavedSearch()
		{
			// arrange
			const int artifactTypeID = 12;
			int viewID = _testViewFieldIDResponses.First().ArtifactID;
			int viewFieldID = _testViewFieldIDResponses.First().ArtifactViewFieldID;
			_viewFieldRepositoryMock
				.Setup(x => x.ReadViewFieldIDsFromSearch(_WORKSPACE_ID, artifactTypeID, viewID))
				.Returns(_testViewFieldIDResponses);

			// act
			int[] result = _sut.RetrieveDefaultViewFieldIds(
				_WORKSPACE_ID,
				viewID,
				artifactTypeID, 
				isProduction: false
			);

			// assert
			_viewFieldRepositoryMock.Verify(
				x => x.ReadViewFieldIDsFromSearch(_WORKSPACE_ID, artifactTypeID, viewID), 
				Times.Once
			);
			result.Length.Should().Be(1);
			result[0].Should().Be(viewFieldID);
		}

		[Test]
		public void RetrieveDefaultViewFieldIdsForSavedSearch_ShouldReturnViewFieldIDForProduction()
		{
			// arrange
			const int artifactTypeID = 12;
			int viewID = _testViewFieldIDResponses.First().ArtifactID;
			int viewFieldID = _testViewFieldIDResponses.First().ArtifactViewFieldID;
			_viewFieldRepositoryMock
				.Setup(x => x.ReadViewFieldIDsFromProduction(_WORKSPACE_ID, artifactTypeID, viewID))
				.Returns(_testViewFieldIDResponses);

			// act
			int[] result = _sut.RetrieveDefaultViewFieldIds(
				_WORKSPACE_ID,
				viewID,
				artifactTypeID, 
				isProduction: true
			);

			// assert
			_viewFieldRepositoryMock.Verify(
				x => x.ReadViewFieldIDsFromProduction(
					_WORKSPACE_ID,
					artifactTypeID,
					viewID
				), Times.Once);
			result.Length.Should().Be(1);
			result[0].Should().Be(viewFieldID);
		}

		[Test]
		public void RetrieveAllExportableViewFields_ShouldReturnCorrectInfos()
		{
			// arrange
			const int artifactTypeID = 12;
			ViewFieldResponse[] viewFieldResponses = _testViewFieldResponses.Take(1).ToArray();
			_viewFieldRepositoryMock.Setup(x => x.ReadExportableViewFields(_WORKSPACE_ID, artifactTypeID))
				.Returns(viewFieldResponses);

			// act
			ViewFieldInfo[] result = _sut.RetrieveAllExportableViewFields(
				_WORKSPACE_ID,
				artifactTypeID
			);

			// assert
			_viewFieldRepositoryMock.Verify(
				x => x.ReadExportableViewFields(
					_WORKSPACE_ID,
					artifactTypeID
				), Times.Once);
			AssertViewFieldInfosAreSameAsExpected(viewFieldResponses, result);
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
				.Setup(x => x.GetFilesForDynamicObjects(_WORKSPACE_ID, fileFieldID, objectIDs))
				.Returns(_testDynamicFileResponses);

			//act
			DataSet result = _sut.RetrieveFilesForDynamicObjects(
				_WORKSPACE_ID,
				fileFieldID,
				objectIDs
			);

			//assert
			_fileFieldRepositoryMock.Verify(
				x => x.GetFilesForDynamicObjects(_WORKSPACE_ID, fileFieldID, objectIDs),
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
				x => x.GetFilesForDynamicObjects(_WORKSPACE_ID, fileFieldID, null),
				Times.Once);
		}



		private static void AssertViewFieldInfosAreSameAsExpected(ViewFieldResponse[] expectedInfos, ViewFieldInfo[] currentInfos)
		{
			currentInfos.Length.Should().Be(expectedInfos.Length);

			var asserts = expectedInfos.Zip(currentInfos, (e, a) => new
			{
				Expected = e,
				Actual = a
			});

			foreach (var assert in asserts)
			{
				ViewFieldInfo actual = assert.Actual;
				ViewFieldResponse expected = assert.Expected;

				actual.FieldArtifactId.Should().Be(expected.ArtifactID);
				actual.AvfId.Should().Be(expected.ArtifactViewFieldID);
				actual.Category.Should().Be(ConvertEnum<FieldCategory>(expected.Category));
				actual.DisplayName.Should().Be(expected.DisplayName);
				actual.AvfColumnName.Should().Be(expected.ArtifactViewFieldColumnName);
				actual.AvfHeaderName.Should().Be(expected.ArtifactViewFieldHeaderName);
				actual.AllowFieldName.Should().Be(expected.AllowFieldName);
				actual.ColumnSource.Should().Be(ConvertEnum<global::Relativity.ViewFieldInfo.ColumnSourceType>(expected.ColumnSourceType));
				actual.DataSource.Should().Be(expected.DataSource);
				actual.SourceFieldName.Should().Be(expected.SourceFieldName);
				actual.SourceFieldArtifactTypeID.Should().Be(expected.SourceFieldArtifactTypeID);
				actual.SourceFieldArtifactID.Should().Be(expected.SourceFieldArtifactID);
				actual.ConnectorFieldArtifactID.Should().Be(expected.ConnectorFieldArtifactID);
				actual.SourceFieldArtifactTypeTableName.Should().Be(expected.SourceFieldArtifactTypeTableName);
				actual.ConnectorFieldName.Should().Be(expected.ConnectorFieldName);
				actual.ConnectorFieldCategory.Should().Be(ConvertEnum<FieldCategory>(expected.ConnectorFieldCategory));
				actual.FieldType.Should().Be(ConvertEnum<FieldTypeHelper.FieldType>(expected.FieldType));
				actual.IsLinked.Should().Be(expected.IsLinked);
				actual.FieldCodeTypeID.Should().Be(expected.FieldCodeTypeID);
				actual.ArtifactTypeID.Should().Be(expected.ArtifactTypeID);
				actual.ArtifactTypeTableName.Should().Be(expected.ArtifactTypeTableName);
				actual.FieldIsArtifactBaseField.Should().Be(expected.FieldIsArtifactBaseField);
				actual.FormatString.Should().Be(expected.FormatString);
				actual.IsUnicodeEnabled.Should().Be(expected.IsUnicodeEnabled);
				actual.AllowHtml.Should().Be(expected.AllowHtml);
				actual.ParentFileFieldArtifactID.Should().Be(expected.ParentFileFieldArtifactID);
				actual.ParentFileFieldDisplayName.Should().Be(expected.ParentFileFieldDisplayName);
				actual.AssociativeArtifactTypeID.Should().Be(expected.AssociativeArtifactTypeID);
				actual.RelationalTableName.Should().Be(expected.RelationalTableName);
				actual.RelationalTableColumnName.Should().Be(expected.RelationalTableColumnName);
				actual.RelationalTableColumnName2.Should().Be(expected.RelationalTableColumnName2);
				actual.ParentReflectionType.Should().Be(ConvertEnum<ParentReflectionType>(expected.ParentReflectionType));
				actual.ReflectedFieldArtifactTypeTableName.Should().Be(expected.ReflectedFieldArtifactTypeTableName);
				actual.ReflectedFieldIdentifierColumnName.Should().Be(expected.ReflectedFieldIdentifierColumnName);
				actual.ReflectedFieldConnectorFieldName.Should().Be(expected.ReflectedFieldConnectorFieldName);
				actual.ReflectedConnectorIdentifierColumnName.Should().Be(expected.ReflectedConnectorIdentifierColumnName);
				actual.EnableDataGrid.Should().Be(expected.EnableDataGrid);
				actual.IsVirtualAssociativeArtifactType.Should().Be(expected.IsVirtualAssociativeArtifactType);
			}
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

		private static string ConvertToCommaSeparatedString(int[] values) => string.Join(",", values);

		private static TEnum ConvertEnum<TEnum>(Enum source)
		{
			return (TEnum)Enum.Parse(typeof(TEnum), source.ToString(), true);
		}
	}
}
