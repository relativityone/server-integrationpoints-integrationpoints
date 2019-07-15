﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.UtilityDTO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories.Implementations;
using kCura.Relativity.Client;
using kCura.WinEDDS;
using NUnit.Framework;
using Relativity.API.Foundation;
using Relativity.Services.FileField;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.ViewField;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;
using Relativity.Services.User;
using Relativity.Testing.Identification;
using Rip.SystemTests.RelativityServices.Arrangers;
using Rip.SystemTests.RelativityServices.TestCases;
using Rip.TestUtilities;
using FieldCategory = Relativity.Services.Objects.DataContracts.FieldCategory;
using FieldRef = Relativity.Services.Field.FieldRef;
using IViewManager = Relativity.Services.View.IViewManager;
using View = Relativity.Services.View.View;

namespace Rip.SystemTests.RelativityServices
{
	[TestFixture]
	public class CoreSearchManagerTests
	{
		private int _workspaceID;
		private int _searchID;
		private ProductionCreateResultDto _productionCreateResult;
		private IWindsorContainer _container => SystemTestsFixture.Container;
		private Lazy<ITestHelper> _testHelperLazy;
		private IRelativityObjectManager _objectManager;
		private IViewManager _viewManager;
		private IKeywordSearchManager _keywordSearchManager;
		private DocumentTestCase[] _documentTestCases;
		private WorkspaceService _workspaceService;
		private ProductionHelper _productionHelper;
		private SavedSearchHelper _savedSearchHelper;

		private CoreSearchManager _sut;

		private const int _DOCUMENT_ARTIFACT_TYPE_ID = 10;
		private const string _DOCUMENT_ARTIFACT_ID_COLUMN_NAME = "DocumentArtifactID";
		private const string _FILENAME_COLUMN_NAME = "Filename";
		private const string _IMAGE_FILENAME_COLUMN_NAME = "ImageFileName";
		private const string _PRODUCTION_ARTIFACT_ID_COLUMN_NAME = "ProductionArtifactID";
		private const string _OBJECT_ARTIFACT_ID_COLUMN_NAME = "ObjectArtifactID";
		private const string _ARTIFACT_ID_COLUMN_NAME = "ArtifactID";
		private const string _NAME_COLUMN_NAME = "Name";
		private const string _ARTIFACT_TYPE_ID_COLUMN_NAME = "ArtifactTypeID";
		private const string _ARTIFACT_TYPE_COLUMN_NAME = "ArtifactType";

		[OneTimeSetUp]
		public async Task OneTimeSetup()
		{
			_workspaceID = SystemTestsFixture.WorkspaceID;
			_workspaceService = new WorkspaceService(new ImportHelper(withNatives: true));
			_testHelperLazy = new Lazy<ITestHelper>(() => new TestHelper());
			_objectManager = _container.Resolve<IRelativityObjectManager>();
			_viewManager = _testHelperLazy.Value.CreateProxy<IViewManager>();
			_keywordSearchManager = _testHelperLazy.Value.CreateProxy<IKeywordSearchManager>();
			_productionHelper = new ProductionHelper(_workspaceID, _objectManager, _workspaceService);
			_savedSearchHelper = new SavedSearchHelper(_workspaceID, _keywordSearchManager);

			DocumentsTestData documentsTestData = DocumentTestDataBuilder.BuildTestData(
				prefix: "CSM_",
				withNatives: true, 
				testDataType: DocumentTestDataBuilder.TestDataType.SmallWithoutFolderStructure
			);
			_documentTestCases = DocumentTestCaseArranger.CreateTestCases(documentsTestData);
			_workspaceService.ImportData(_workspaceID, documentsTestData);
			await DocumentTestCaseArranger.FillTestCasesWithDocumentArtifactIDsAsync(
				_workspaceID,
				_documentTestCases,
				_objectManager
			).ConfigureAwait(false);

			_sut = CreateCoreSearchManager();

			CreateAndRunProduction(documentsTestData);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			_productionHelper.DeleteProduction(_productionCreateResult);
			_savedSearchHelper.DeleteSavedSearch(_searchID);
		}

		[IdentifiedTest("970522ef-de92-404d-b120-c198ebb154e5")]
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

		[IdentifiedTest("254977f7-1f71-4315-a257-5c3a0d19923a")]
		public void RetrieveNativesForProduction_ShouldRetrieveDocumentsByIDsAndProductionID()
		{
			//arrange
			int[] documentIDs = _documentTestCases.Select(x => x.ArtifactID).ToArray();
			string documentIDsAsString = ConvertIntArrayToCommaSeparatedString(documentIDs);

			//act
			DataSet result = _sut.RetrieveNativesForProduction(
				_workspaceID,
				_productionCreateResult.ProductionArtifactID,
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

		[IdentifiedTest("635f343b-afa1-4012-b67d-55288baef551")]
		public async Task RetrieveFilesForDynamicObjects_ShouldRetrieveFileForProductionPlacehoder()
		{
			//arrange
			int placeholderFieldArtifactID = await GetPlaceholderFieldArtifactIDAsync()
				.ConfigureAwait(false);
			int productionPlaceholderArtifactID = await _workspaceService
				.GetDefaultProductionPlaceholderArtifactIDAsync(_objectManager)
				.ConfigureAwait(false);

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

		[IdentifiedTest("67ccf933-70c7-4d57-ae01-a17801e0fead")]
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
				_productionCreateResult.ProductionArtifactID
			);

			//assert
			AssertImagesAreSameAsExpected(
				result,
				_documentTestCases.SelectMany(x => x.Images).ToArray(),
				expectedLength: numberOfImages + numberOfPlaceholders,
				fileNameColumnName: _IMAGE_FILENAME_COLUMN_NAME
			);
		}

		[IdentifiedTest("b30e2994-b38a-4544-b2c3-b8b5f10c1969")]
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

		[IdentifiedTest("74dd9275-1c0d-4fba-b894-aeef33d54d4f")]
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

		[IdentifiedTest("394b7203-50b3-4433-a08c-6e1c230ff771")]
		public void RetrieveImagesByProductionIDsAndDocumentIDsForExport_ShouldRetrieveProducedImagesForExport()
		{
			//arrange
			int[] documentIDs = _documentTestCases.Select(x => x.ArtifactID).ToArray();
			int numberOfPlaceholders = _documentTestCases.Count(x => !x.Images.Any());
			int numberOfImages = _documentTestCases.Sum(x => x.Images.Count);

			//act
			DataSet result = _sut.RetrieveImagesByProductionIDsAndDocumentIDsForExport(
				_workspaceID, 
				new []{ _productionCreateResult.ProductionArtifactID }, 
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
				_productionCreateResult.ProductionArtifactID
			);
		}

		[IdentifiedTest("d381c2fd-30f1-472c-a050-317dcd103cf8")]
		public async Task RetrieveAllExportableViewFields_ShouldRetrieveAllViewFieldsForDocument()
		{
			// arrange
			IList<int> exportableFieldIDs = await RetrieveExportableFieldIDsAsync().ConfigureAwait(false);

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

		[IdentifiedTest("b317ec6d-ebb3-4481-973c-a61d8321da25")]
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

		[IdentifiedTest("7b0fa46f-2831-4c6e-99c5-2eb0af8dddd3")]
		public void RetrieveViewsByContextArtifactID_ShouldRetrieveViews()
		{
			//arrange
			View view = CreateTestView();

			//act
			DataSet result =
				_sut.RetrieveViewsByContextArtifactID(_workspaceID, _DOCUMENT_ARTIFACT_TYPE_ID, isSearch: false);

			//assert
			AssertViewResponseContainsExpectedView(view, result);
		}

		[IdentifiedTest("da483778-1f5e-44f5-a125-dc1e5082c71e")]
		public async Task RetrieveViewsByContextArtifactIDForSearch_ShouldRetrieveViews()
		{
			//arrange
			int searchArtifactID = SavedSearch.CreateSavedSearch(_workspaceID, name: "CoreSearchManagerTestSavedSearch");
			KeywordSearch savedSearch = await SavedSearch.ReadAsync(_workspaceID, searchArtifactID).ConfigureAwait(false);

			//act
			DataSet result =
				_sut.RetrieveViewsByContextArtifactID(_workspaceID, _DOCUMENT_ARTIFACT_TYPE_ID, isSearch: true);

			//assert
			AssertSearchViewResponseContainsExpectedSearch(savedSearch, result);
		}

		private async Task<IList<int>> RetrieveExportableFieldIDsAsync()
		{
			QueryRequest fieldQuery = new QueryRequest
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

			ResultSet<RelativityObject> queryResult = await _objectManager
				.QueryAsync(fieldQuery, 0, 1000)
				.ConfigureAwait(false);

			IList<int> fields = queryResult
				.Items
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
			IViewFieldManager viewFieldManager = _testHelperLazy.Value.CreateProxy<IViewFieldManager>();
			IFileManager fileManager = _testHelperLazy.Value.CreateProxy<IFileManager>();
			IFileFieldManager fileFieldManager = _testHelperLazy.Value.CreateProxy<IFileFieldManager>();
			IViewManager viewManager = _testHelperLazy.Value.CreateProxy<IViewManager>();
			IExternalServiceInstrumentationProvider instrumentationProvider = 
				new ExternalServiceInstrumentationProviderWithoutJobContext(_testHelperLazy.Value.GetLoggerFactory().GetLogger());
			IViewFieldRepository viewFieldRepository = new ViewFieldRepository(viewFieldManager, instrumentationProvider);
			IFileRepository fileRepository = new FileRepository(fileManager, instrumentationProvider);
			IFileFieldRepository fileFieldRepository = new FileFieldRepository(fileFieldManager, instrumentationProvider);
			IViewRepository viewRepository = new ViewRepository(viewManager, instrumentationProvider);
			var coreSearchManager = new CoreSearchManager(
				fileRepository,
				fileFieldRepository,
				viewFieldRepository,
				viewRepository
			);
			return coreSearchManager;
		}

		private void CreateAndRunProduction(DocumentsTestData documentsTestData)
		{
			_searchID = _savedSearchHelper.CreateSavedSearch(documentsTestData);
			_productionCreateResult = _productionHelper.CreateAndRunProduction(_searchID);
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

			ResultSet<RelativityObject> result = await _objectManager.QueryAsync(
					request, 
					start: 0, 
					length: 1
				).ConfigureAwait(false);

			return result.Items.Single().ArtifactID;
		}

		private void AssertViewResponseContainsExpectedView(View view, DataSet dataSet)
		{
			DataRow[] rows = GetRowsFromFirstTable(dataSet);
			DataRow row = rows.Single(x => (int)x[_ARTIFACT_ID_COLUMN_NAME] == view.ArtifactID);
			row[_NAME_COLUMN_NAME].Should().Be(view.Name);
			row[_ARTIFACT_TYPE_ID_COLUMN_NAME].Should().Be(view.ArtifactTypeID);
			row.Table.Columns.Contains(_ARTIFACT_TYPE_COLUMN_NAME).Should().BeFalse();
		}

		private void AssertSearchViewResponseContainsExpectedSearch(KeywordSearch savedSearch, DataSet dataSet)
		{
			DataRow[] rows = GetRowsFromFirstTable(dataSet);
			DataRow row = rows.Single(x => (int)x[_ARTIFACT_ID_COLUMN_NAME] == savedSearch.ArtifactID);
			row[_NAME_COLUMN_NAME].Should().Be(savedSearch.Name);
			row[_ARTIFACT_TYPE_ID_COLUMN_NAME].Should().Be(savedSearch.ArtifactTypeID);
			row.Table.Columns.Contains(_ARTIFACT_TYPE_COLUMN_NAME).Should().BeTrue();
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
