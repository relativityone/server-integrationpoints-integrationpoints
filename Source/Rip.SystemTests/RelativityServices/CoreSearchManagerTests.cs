using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories.Implementations;
using kCura.Relativity.Client;
using kCura.WinEDDS;
using Moq;
using NUnit.Framework;
using Relativity.API.Foundation;
using Relativity.Core;
using Relativity.Services.FileField;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.ViewField;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.User;
using Rip.SystemTests.RelativityServices.Arrangers;
using Rip.SystemTests.RelativityServices.TestCases;
using FieldCategory = Relativity.Services.Objects.DataContracts.FieldCategory;
using FieldRef = Relativity.Services.Field.FieldRef;
using IViewManager = Relativity.Services.View.IViewManager;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;
using View = Relativity.Services.View.View;

namespace Rip.SystemTests.RelativityServices
{
	[TestFixture]
	public class CoreSearchManagerTests
	{
		private int _workspaceID;
		private int _productionID;
		private Lazy<ITestHelper> _testHelperLazy;
		private IObjectManager _objectManager;
		private IViewManager _viewManager;
		private DocumentsTestData _documentsTestData;
		private DocumentTestCase[] _documentTestCases;
		private WorkspaceService _workspaceService;

		private CoreSearchManager _sut;

		private const int _DOCUMENT_ARTIFACT_TYPE_ID = 10;
		private const string _DOCUMENT_ARTIFACT_ID_COLUMN_NAME = "DocumentArtifactID";
		private const string _FILENAME_COLUMN_NAME = "Filename";
		private const string _IMAGE_FILENAME_COLUMN_NAME = "ImageFileName";
		private const string _PRODUCTION_ARTIFACT_ID_COLUMN_NAME = "ProductionArtifactID";
		private const string _OBJECT_ARTIFACT_ID_COLUMN_NAME = "ObjectArtifactID";

		[OneTimeSetUp]
		public async Task OneTimeSetup()
		{
			_workspaceID = SystemTestsFixture.WorkspaceID;
			_workspaceService = new WorkspaceService(new ImportHelper(withNatives: true));
			_testHelperLazy = new Lazy<ITestHelper>(() => new TestHelper());
			_objectManager = _testHelperLazy.Value.CreateUserProxy<IObjectManager>();
			_viewManager = _testHelperLazy.Value.CreateUserProxy<IViewManager>();

			_documentsTestData = DocumentTestDataBuilder.BuildTestData(
				withNatives: true, 
				testDataType: DocumentTestDataBuilder.TestDataType.SmallWithoutFolderStructure
			);
			_documentTestCases = DocumentTestCaseArranger.CreateTestCases(_documentsTestData);
			_workspaceService.ImportData(_workspaceID, _documentsTestData);
			await DocumentTestCaseArranger.FillTestCasesWithDocumentArtifactIDsAsync(
				_workspaceID,
				_documentTestCases,
				_objectManager
			).ConfigureAwait(false);

			_sut = CreateCoreSearchManager();
			_productionID = CreateAndRunProduction(_workspaceService);
		}

		[Test]
		public void RetrieveNativesForSearch_ShouldRetrieveAllDocumentsByIDs()
		{
			//arrange
			int[] documentIDs = _documentTestCases.Select(x => x.ArtifactID).ToArray();
			string documentIDsAsString = ConvertIntArrayToCommaSeparatedString(documentIDs);

			//act
			DataSet result = _sut.RetrieveNativesForSearch(
				_workspaceID,
				documentIDsAsString
			);

			//assert
			AssertNativesAreSameAsExpected(
				result,
				_documentTestCases,
				expectedLength: documentIDs.Length,
				fileNameColumnName: _FILENAME_COLUMN_NAME
			);
		}

		[Test]
		public void RetrieveNativesForProduction_ShouldRetrieveDocumentsByIDsAndProductionID()
		{
			//arrange
			int[] documentIDs = _documentTestCases.Select(x => x.ArtifactID).ToArray();
			string documentIDsAsString = ConvertIntArrayToCommaSeparatedString(documentIDs);

			//act
			DataSet result = _sut.RetrieveNativesForProduction(
				_workspaceID, 
				_productionID,
				documentIDsAsString
			);

			//assert
			AssertNativesAreSameAsExpected(
				result,
				_documentTestCases,
				expectedLength: documentIDs.Length,
				fileNameColumnName: _FILENAME_COLUMN_NAME
			);
		}

		[Test]
		public async Task RetrieveFilesForDynamicObjects_ShouldRetrieveFileForProductionPlacehoder()
		{
			//arrange
			int placeholderFieldArtifactID = await GetPlaceholderFieldArtifactIDAsync()
				.ConfigureAwait(false);
			int productionPlaceholderArtifactID =
				await _workspaceService.GetDefaultProductionPlaceholderArtifactIDAsync(
					_workspaceID,
					_objectManager
				).ConfigureAwait(false);

			//act
			DataSet result = _sut.RetrieveFilesForDynamicObjects(
				_workspaceID,
				placeholderFieldArtifactID,
				new[] { productionPlaceholderArtifactID }
			);

			//assert
			DataRow[] rows = GetRowsFromFirstTable(result);
			rows.Length.Should().Be(1);
			rows.Single()[_OBJECT_ARTIFACT_ID_COLUMN_NAME]
				.Should()
				.Be(productionPlaceholderArtifactID);
		}

		[Test]
		public void RetrieveImagesForProductionDocuments_ShouldRetrieveImagesForProducedDocuments()
		{
			//arrange
			int[] documentIDs = _documentTestCases.Select(x => x.ArtifactID).ToArray();
			int numberOfPlaceholders = _documentTestCases.Count(x => !x.Images.Any());
			int numberOfImages = _documentTestCases.Sum(x => x.Images.Count);

			//act
			DataSet result = _sut.RetrieveImagesForProductionDocuments(
				_workspaceID, 
				documentIDs, 
				_productionID
			);

			//assert
			AssertImagesAreSameAsExpected(
				result,
				_documentTestCases.SelectMany(x => x.Images).ToArray(),
				expectedLength: numberOfImages + numberOfPlaceholders,
				fileNameColumnName: _IMAGE_FILENAME_COLUMN_NAME
			);
		}

		[Test]
		public void RetrieveImagesForDocuments_ShouldRetrieveImagesByDocumentIDs()
		{
			//arrange
			DocumentTestCase[] documentsWithImages = _documentTestCases
				.Where(x => x.Images.Any())
				.ToArray();
			int[] documentIDs = documentsWithImages
				.Select(x => x.ArtifactID)
				.ToArray();
			int numberOfImages = documentsWithImages.Sum(x => x.Images.Count);

			//act
			DataSet result = _sut.RetrieveImagesForDocuments(
				_workspaceID, 
				documentIDs
			);

			//assert
			AssertImagesAreSameAsExpected(
				result,
				documentsWithImages.SelectMany(x => x.Images).ToArray(),
				expectedLength: numberOfImages,
				fileNameColumnName: _FILENAME_COLUMN_NAME
			);
		}

		[Test]
		public  void RetrieveProducedImagesForDocument_ShouldRetrieveImagesByDocumentId()
		{
			//arrange
			DocumentTestCase documentWithMoreThanOneImage = _documentTestCases.Single(x => x.Images.Count > 1);
			int documentID = documentWithMoreThanOneImage.ArtifactID;
			
			//act
			DataSet result = _sut.RetrieveProducedImagesForDocument(
				_workspaceID, 
				documentID
			);

			//assert
			AssertImagesAreSameAsExpected(
				result, 
				documentWithMoreThanOneImage.Images.ToArray(), 
				expectedLength: documentWithMoreThanOneImage.Images.Count,
				fileNameColumnName: _FILENAME_COLUMN_NAME
			);
		}

		[Test]
		public void RetrieveImagesByProductionIDsAndDocumentIDsForExport_ShouldRetrieveProducedImagesForExport()
		{
			//arrange
			int[] documentIDs = _documentTestCases.Select(x => x.ArtifactID).ToArray();
			int numberOfPlaceholders = _documentTestCases.Count(x => !x.Images.Any());
			int numberOfImages = _documentTestCases.Sum(x => x.Images.Count);

			//act
			DataSet result = _sut.RetrieveImagesByProductionIDsAndDocumentIDsForExport(
				_workspaceID, 
				new []{ _productionID }, 
				documentIDs.ToArray()
			);

			//assert
			AssertImagesAreSameAsExpected(
				result,
				_documentTestCases.SelectMany(x => x.Images).ToArray(), 
				expectedLength: numberOfImages + numberOfPlaceholders,
				fileNameColumnName: _IMAGE_FILENAME_COLUMN_NAME
			);
			AssertProductionIDIsSameAsExpected(
				result,
				_productionID
			);
		}

		[Test]
		public async Task RetrieveAllExportableViewFields_ShouldRetrieveAllViewFieldsForDocument()
		{
			// arrange
			IList<int> exportableFieldIDs = await RetrieveExportableFieldIDsAsync();

			// act
			ViewFieldInfo[] result = _sut.RetrieveAllExportableViewFields(
				_workspaceID, 
				_DOCUMENT_ARTIFACT_TYPE_ID
			);

			// assert
			result.Length.Should().Be(exportableFieldIDs.Count);
			int[] fieldArtifactIDs = result
				.Select(ViewFieldInfo => ViewFieldInfo.FieldArtifactId)
				.ToArray();
			exportableFieldIDs.Should().Contain(fieldArtifactIDs);
		}

		[Test]
		public void RetrieveDefaultViewFieldIDs_ShouldRetrieveDefaultViewFieldIDsForSavedSearchView()
		{
			// arrange
			View view = CreateTestView();

			// act
			int[] result =
				_sut.RetrieveDefaultViewFieldIds(_workspaceID, view.ArtifactID, _DOCUMENT_ARTIFACT_TYPE_ID, false);

			// assert
			view.Fields.Count.Should().Be(result.Length);
			int[] expectedViewFieldIDs = view.Fields
				.Select(fieldRef => fieldRef.ViewFieldID)
				.ToArray();
			result.Should().Contain(expectedViewFieldIDs);
		}

		private async Task<IList<int>> RetrieveExportableFieldIDsAsync()
		{
			var fieldQuery = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int) ArtifactType.Field },
				Fields = new[]
				{
					new Relativity.Services.Objects.DataContracts.FieldRef {Name = TestConstants.FieldNames.ARTIFACT_ID},
					new Relativity.Services.Objects.DataContracts.FieldRef {Name = TestConstants.FieldNames.FIELD_TYPE},
					new Relativity.Services.Objects.DataContracts.FieldRef {Name = TestConstants.FieldNames.FIELD_CATEGORY_ID}
				},
				Condition = $"'{TestConstants.FieldNames.OBJECT_TYPE_ARTIFACT_TYPE_ID}' == OBJECT {_DOCUMENT_ARTIFACT_TYPE_ID}"
			};

			QueryResult queryResult = await _objectManager
				.QueryAsync(_workspaceID, fieldQuery, 0, 1000)
				.ConfigureAwait(false);

			IList<int> fields = queryResult
				.Objects
				.Select(fieldObject => new
				{
					ArtifactID = (int) fieldObject[TestConstants.FieldNames.ARTIFACT_ID].Value,
					FieldType = fieldObject[TestConstants.FieldNames.FIELD_TYPE].Value.ToString(),
					CategoryID = (int) fieldObject[TestConstants.FieldNames.FIELD_CATEGORY_ID].Value
				})
				.Where(item => IsFieldExportable(item.FieldType, item.CategoryID))
				.Select(item => item.ArtifactID)
				.ToList();

			return fields;
		}

		private static bool IsFieldExportable(string fieldType, int fieldCategoryID)
		{
			if (fieldCategoryID == (int)FieldCategory.FileInfo)
			{
				return false;
			}

			if (fieldCategoryID == (int)FieldCategory.MultiReflected)
			{
				if (fieldType == TestConstants.FieldTypeNames.LONG_TEXT ||
				    fieldType == TestConstants.FieldTypeNames.MULTIPLE_CHOICE)
				{
					return false;
				}
			}

			return true;
		}

		private View CreateTestView()
		{
			View view = new View
			{
				Name = "CoreSearchManagerTestsView",
				ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID,
				Owner = new UserRef(0),
				Order = 9999,
				VisibleInDropdown = false,
				Fields =
				{
					new FieldRef(TestConstants.FieldNames.CONTROL_NUMBER),
					new FieldRef(TestConstants.FieldNames.EXTRACTED_TEXT),
					new FieldRef(TestConstants.FieldNames.GROUP_IDENTIFIER)
				}
			};

			int viewID = _viewManager.CreateSingleAsync(_workspaceID, view).GetAwaiter().GetResult();
			return _viewManager.ReadSingleAsync(_workspaceID, viewID).GetAwaiter().GetResult();
		}

		private CoreSearchManager CreateCoreSearchManager()
		{
			var baseServiceContextMock = new Mock<BaseServiceContext>();
			IViewFieldManager viewFieldManager = _testHelperLazy.Value.CreateUserProxy<IViewFieldManager>();
			IFileManager fileManager = _testHelperLazy.Value.CreateUserProxy<IFileManager>();
			IFileFieldManager fileFieldManager = _testHelperLazy.Value.CreateUserProxy<IFileFieldManager>();
			IExternalServiceInstrumentationProvider instrumentationProvider =
				new ExternalServiceInstrumentationProviderWithoutJobContext(_testHelperLazy.Value.GetLoggerFactory().GetLogger());
			IViewFieldRepository viewFieldRepository = new ViewFieldRepository(viewFieldManager, instrumentationProvider);
			IFileRepository fileRepository = new FileRepository(fileManager, instrumentationProvider);
			IFileFieldRepository fileFieldRepository = new FileFieldRepository(fileFieldManager, instrumentationProvider);
			var coreSearchManager = new CoreSearchManager(
				baseServiceContextMock.Object,
				fileRepository,
				fileFieldRepository,
				viewFieldRepository
			);
			return coreSearchManager;
		}

		private int CreateAndRunProduction(WorkspaceService workspaceService)
		{
			int savedSearch = workspaceService.CreateSavedSearch(
				new List<string> { "Control Number" }, 
				_workspaceID, 
				"SavedSearchName"
			);

			return workspaceService.CreateAndRunProduction(
				_workspaceID, 
				savedSearch, 
				"ProdName"
			);
		}

		private DataRow[] GetRowsFromFirstTable(DataSet dataSet) => dataSet.Tables[0].Select();

		private string ConvertIntArrayToCommaSeparatedString(IEnumerable<int> docsIds)
		{
			return string.Join(",", docsIds);
		}

		private void AssertNativesAreSameAsExpected(
			DataSet actualDataSet, 
			DocumentTestCase[] expectedTestCases, 
			int expectedLength,
			string fileNameColumnName)
		{
			DataRow[] rows = GetRowsFromFirstTable(actualDataSet);
			rows.Length.Should().Be(expectedLength);

			var asserts = rows.Zip(expectedTestCases, (a, e) => new
			{
				Expected = e,
				Actual = a
			});

			foreach (var assert in asserts)
			{
				DataRow actual = assert.Actual;
				DocumentTestCase expected = assert.Expected;

				((int)actual[_DOCUMENT_ARTIFACT_ID_COLUMN_NAME]).Should().Be(expected.ArtifactID);
				actual[fileNameColumnName].ToString().Should().Be(expected.FileName);
			}
		}

		private void AssertImagesAreSameAsExpected(
			DataSet actualDataSet,
			ImageTestCase[] expectedTestCases,
			int expectedLength,
			string fileNameColumnName)
		{
			DataRow[] rows = GetRowsFromFirstTable(actualDataSet);
			rows.Length.Should().Be(expectedLength);

			var asserts = rows.Zip(expectedTestCases, (a, e) => new
			{
				Expected = e,
				Actual = a
			});

			foreach (var assert in asserts)
			{
				DataRow actual = assert.Actual;
				ImageTestCase expected = assert.Expected;

				((int)actual[_DOCUMENT_ARTIFACT_ID_COLUMN_NAME]).Should().Be(expected.DocumentArtifactID);
				actual[fileNameColumnName].ToString().Should().Be(expected.FileName);
			}
		}

		private void AssertProductionIDIsSameAsExpected(DataSet actualDataSet, int expectedProductionID)
		{
			DataRow[] rows = GetRowsFromFirstTable(actualDataSet);

			int actualProductionID = rows
				.Select(row => row[_PRODUCTION_ARTIFACT_ID_COLUMN_NAME])
				.OfType<int>()
				.Distinct()
				.Single();
			actualProductionID.Should().Be(expectedProductionID);
		}

		private async Task<int> GetPlaceholderFieldArtifactIDAsync()
		{
			var request = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = (int) ArtifactType.Field
				},
				Condition = "('Field Type' == 'File' AND 'Name' == 'Placeholder')"
			};

			QueryResult result = await _objectManager.QueryAsync(
					_workspaceID, request, start: 1, length: 1
				).ConfigureAwait(false);

			return result.Objects.Single().ArtifactID;
		}
	}

	internal class ArtifactRef : IArtifactRef
	{
		public int ArtifactID { get; set; }
		public IList<Guid> Guids { get; set; }

		public ArtifactRef(int artifactID)
		{
			ArtifactID = artifactID;
		}
	}
}
