﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Repositories;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;
using Relativity.Testing.Identification;
using Rip.TestUtilities;

namespace Relativity.IntegrationPoints.FunctionalTests.SystemTests.IntegrationPointServices
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class KeplerDocumentRepositoryTests
	{
		private const string SMALL_SET_DOC_PREFIX = "SMALL_";
		private const string LARGE_PRODUCTION_SET_DOC_PREFIX = "LARGE_PROD_";
		private const string LONG_DATA_SET_EMBEDDED_DATA_INFO_VALUE = "KEPLER_DOCUMENT_REPOSITORY_LONG";

		private int _workspaceID => SystemTestsSetupFixture.SourceWorkspace.ArtifactID;
		private IWindsorContainer _container => SystemTestsSetupFixture.Container;
		private ITestHelper _testHelper;
		private IDocumentRepository _documentRepository;
		private IRelativityObjectManager _relativityObjectManager;
		private IKeywordSearchManager _keywordSearchManager;
		private ImportHelper _importHelperWithoutNatives;
		private WorkspaceService _workspaceService;
		private SavedSearchHelper _savedSearchHelper;
		private LoadFileHelper _loadFileHelper;

		private DocumentsTestData _smallDocumentsTestData;
		private DocumentsTestData _largeDocumentsTestData;

		private DocumentsTestData _smallProductionSetTestData;
		private DocumentsTestData _largeProductionSetTestData;

		private int _smallDataSetSearchArtifactID;
		private int _largeDataSetSearchArtifactID;

		private int _smallDataSetProductionID;
		private int _largeDataSetProductionID;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			_testHelper = new TestHelper();
			_documentRepository = _container.Resolve<IDocumentRepository>();
			_relativityObjectManager = _container.Resolve<IRelativityObjectManager>();
			_keywordSearchManager = _testHelper.CreateProxy<IKeywordSearchManager>();
			_importHelperWithoutNatives = new ImportHelper(withNatives: false);
			_workspaceService = new WorkspaceService(new ImportHelper(withNatives: true));
			_savedSearchHelper = new SavedSearchHelper(_workspaceID, _keywordSearchManager);
			_loadFileHelper = new LoadFileHelper();

			// Saved searches

			ImportSmallDataSet();
			ImportLargeDataSet();

			_smallDataSetSearchArtifactID = _savedSearchHelper.CreateSavedSearch(_smallDocumentsTestData);
			_largeDataSetSearchArtifactID = _savedSearchHelper.CreateSavedSearch(
				TestConstants.FieldNames.EMBEDDED_DATA_INFO,
				CriteriaConditionEnum.Is, 
				LONG_DATA_SET_EMBEDDED_DATA_INFO_VALUE);

			// Productions

			_smallDataSetProductionID = _workspaceService.CreateProductionAsync(_workspaceID, "Small").GetAwaiter().GetResult();
			_largeDataSetProductionID = _workspaceService.CreateProductionAsync(_workspaceID, "Large").GetAwaiter().GetResult();

			ImportSmallProductionDataSet(_smallDataSetProductionID);
			ImportLargeProductionDataSet(_largeDataSetProductionID);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			_savedSearchHelper.DeleteSavedSearch(_smallDataSetSearchArtifactID);
			_savedSearchHelper.DeleteSavedSearch(_largeDataSetSearchArtifactID);
			_workspaceService.DeleteProductionAsync(_workspaceID, _smallDataSetProductionID).GetAwaiter().GetResult();
			_workspaceService.DeleteProductionAsync(_workspaceID, _largeDataSetProductionID).GetAwaiter().GetResult();
		}

		[IdentifiedTest("46D66A54-4604-491A-ABBB-A84F1905A1A0")]
		public Task DocumentExportFromSavedSearch_ShouldReturnCorrectDocumentFieldValues_ForSmallDataSet()
		{
			string[] fieldNames =
			{
				TestConstants.FieldNames.CONTROL_NUMBER,
				TestConstants.FieldNames.DOCUMENT_EXTENSION
			};

			return RunDocumentExportFromSavedSearchTestAsync(_smallDataSetSearchArtifactID, fieldNames, _smallDocumentsTestData);
		}

		[IdentifiedTest("0FE282D3-61B1-4DF7-837A-8647E25BC0C0")]
		public Task DocumentExportFromSavedSearch_ShouldReturnCorrectDocumentFieldValues_ForLargeDataSet()
		{
			string[] fieldNames =
			{
				TestConstants.FieldNames.CONTROL_NUMBER,
				TestConstants.FieldNames.EMAIL_FROM,
				TestConstants.FieldNames.SAMPLE_INDICATOR,
				TestConstants.FieldNames.DATE_SENT,
				TestConstants.FieldNames.DATE_RECEIVED,
				TestConstants.FieldNames.EMBEDDED_DATA_INFO
			};

			return RunDocumentExportFromSavedSearchTestAsync(_largeDataSetSearchArtifactID, fieldNames, _largeDocumentsTestData);
		}

		[IdentifiedTest("175CEB53-3172-4B71-A6DA-686AA0942F2C")]
		public Task DocumentExportFromProduction_ShouldReturnCorrentDocumentFieldValues_ForSmallDataSet()
		{
			return RunDocumentExportFromProductionTestAsync(_smallDataSetProductionID, SMALL_SET_DOC_PREFIX);
		}

		[IdentifiedTest("E61E48E9-DD0E-46E5-9FC8-C3A2DFF1A0D1")]
		public Task DocumentExportFromProduction_ShouldReturnCorrentDocumentFieldValues_ForLargeDataSet()
		{
			return RunDocumentExportFromProductionTestAsync(_largeDataSetProductionID, LARGE_PRODUCTION_SET_DOC_PREFIX);
		}

		private async Task RunDocumentExportFromSavedSearchTestAsync(
			int searchArtifactID,
			string[] fieldNames,
			DocumentsTestData documentsTestData)
		{
			// arrange
			int[] fieldArtifactIds = fieldNames.Select(RetrieveFieldArtifactID).ToArray();
			int expectedDocumentCount = documentsTestData.AllDocumentsDataTable.AsEnumerable().Count();

			// act
			ExportInitializationResultsDto initializationResults = await _documentRepository
				.InitializeSearchExportAsync(searchArtifactID, fieldArtifactIds, 0)
				.ConfigureAwait(false);

			IList<RelativityObjectSlimDto> objects = await _documentRepository
				.RetrieveResultsBlockFromExportAsync(
					initializationResults,
					(int)initializationResults.RecordCount,
					exportIndexID: 0)
				.ConfigureAwait(false);

			// assert
			initializationResults.RecordCount.Should().Be((long) expectedDocumentCount);
			initializationResults.FieldNames.ShouldBeEquivalentTo(fieldNames, options => options.WithStrictOrdering());

			objects.Count.Should().Be(expectedDocumentCount);
			AssertDocumentsAreIdenticalForSavedSearch(
				documentsTestData.AllDocumentsDataTable.AsEnumerable(),
				objects,
				fieldNames);
		}

		private async Task RunDocumentExportFromProductionTestAsync(int productionID, string productionBegBatesPrefix)
		{
			// arrange
			string[] fieldNames =
			{
				TestConstants.FieldNames.ARTIFACT_ID,
				TestConstants.FieldNames.PRODUCTION_BEGIN_BATES,
				TestConstants.FieldNames.PRODUCTION_END_BATES
			};
			int[] fieldArtifactIds = fieldNames.Select(RetrieveFieldArtifactID).ToArray();
			IList<RelativityObject> expectedDocuments = await RetrieveExpectedDocumentsForProductionAsync(
					fieldNames,
					productionBegBatesPrefix)
				.ConfigureAwait(false);

			// act
			ExportInitializationResultsDto initializationResults = await _documentRepository
				.InitializeProductionExportAsync(productionID, fieldArtifactIds, 0)
				.ConfigureAwait(false);

			IList<RelativityObjectSlimDto> objects = await _documentRepository
				.RetrieveResultsBlockFromExportAsync(
					initializationResults,
					(int) initializationResults.RecordCount,
					exportIndexID: 0)
				.ConfigureAwait(false);

			// assert
			initializationResults.RecordCount
				.Should().Be((long)expectedDocuments.Count);
			initializationResults.FieldNames.ShouldBeEquivalentTo(fieldNames, options => options.WithStrictOrdering());

			objects.Count.Should().Be(expectedDocuments.Count);
			AssertDocumentsAreIdenticalForProduction(
				expectedDocuments,
				objects,
				fieldNames);
		}

		private void ImportSmallDataSet()
		{
			_smallDocumentsTestData = DocumentTestDataBuilder.BuildTestData(
				prefix: SMALL_SET_DOC_PREFIX,
				withNatives: true,
				testDataType: DocumentTestDataBuilder.TestDataType.SmallWithoutFolderStructure
			);
			_workspaceService.ImportData(_workspaceID, _smallDocumentsTestData);
		}

		private void ImportSmallProductionDataSet(int productionID)
		{
			_smallProductionSetTestData = DocumentTestDataBuilder.BuildTestData(
				prefix: SMALL_SET_DOC_PREFIX,
				withNatives: false,
				testDataType: DocumentTestDataBuilder.TestDataType.SmallWithoutFolderStructure
			);
			_workspaceService.ImportDataToProduction(_workspaceID, productionID, _smallProductionSetTestData.Images);
		}

		private void ImportLargeDataSet()
		{
			const char separator = '|';
			const char quote = '^';

			string testDirectory = TestContext.CurrentContext.TestDirectory;
			string loadFilePath = Path.Combine(testDirectory, SharedVariables.SystemTestDataLocation, "KeplerDocumentRepository_Long.dat");
			Type[] columnTypes = {typeof(string), typeof(string), typeof(bool), typeof(DateTime), typeof(DateTime), typeof(string)};
			_largeDocumentsTestData = _loadFileHelper.BuildDocumentTestDataFromLoadFile(
				loadFilePath,
				columnTypes,
				separator,
				quote,
				"KeplerDocumentRepository_LargeDataSet");

			_importHelperWithoutNatives.ImportMetadataFromFileWithExtractedTextInFile(
				_workspaceID,
				_largeDocumentsTestData.AllDocumentsDataTable);
		}

		private void ImportLargeProductionDataSet(int productionID)
		{
			DocumentsTestData testDataForDocuments = DocumentTestDataBuilder.BuildTestData(
				prefix: LARGE_PRODUCTION_SET_DOC_PREFIX,
				withNatives: false, 
				testDataType: DocumentTestDataBuilder.TestDataType.SaltPepperWithFolderStructure);

			_workspaceService.ImportData(_workspaceID, testDataForDocuments);
			
			_largeProductionSetTestData = DocumentTestDataBuilder.BuildTestData(
				prefix: LARGE_PRODUCTION_SET_DOC_PREFIX,
				withNatives: false,
				testDataType: DocumentTestDataBuilder.TestDataType.SaltPepperWithFolderStructure);

			_workspaceService.ImportDataToProduction(_workspaceID, productionID, _largeProductionSetTestData.Images);
		}

		private int RetrieveFieldArtifactID(string fieldName)
		{
			var queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef {ArtifactTypeID = (int) ArtifactType.Field},
				Fields = new List<FieldRef>(),
				Condition = $"((('Object Type' IN ['Document']))) AND ((('Name' LIKE ['{fieldName}'])))"
			};
			RelativityObject result = _relativityObjectManager.Query(queryRequest).First();
			return result.ArtifactID;
		}

		private async Task<IList<RelativityObject>> RetrieveExpectedDocumentsForProductionAsync(IEnumerable<string> fieldNames, string productionBegBatesPrefix)
		{
			IList<FieldRef> fields = fieldNames
				.Select(x => new FieldRef { Name = x })
				.ToList();
			var request = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Document },
				Fields = fields,
				Condition = $"'Production::Begin Bates' STARTSWITH '{productionBegBatesPrefix}'"
			};
			IList<RelativityObject> result = await _relativityObjectManager.QueryAsync(request).ConfigureAwait(false);
			return result;
		}

		private void AssertDocumentsAreIdenticalForSavedSearch(
			EnumerableRowCollection<DataRow> expected, 
			IList<RelativityObjectSlimDto> actual,
			IEnumerable<string> fieldNames)
		{
			var fieldValues = fieldNames
				.Select(fieldName => expected
					.Zip(
						actual,
						(dataRow, objectSlim) => new
						{
							ExpectedValue = dataRow[fieldName],
							ActualValue = objectSlim.FieldValues[fieldName]
						}))
				.SelectMany(x => x);

			foreach (var fieldValue in fieldValues)
			{
				if (fieldValue.ExpectedValue is DateTime expectedValueAsDateTime)
				{
					var stringRepresentation = expectedValueAsDateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
					fieldValue.ActualValue.Should().Be(stringRepresentation);
				}
				else
				{
					fieldValue.ActualValue.Should().Be(fieldValue.ExpectedValue);
				}
			}
		}

		private void AssertDocumentsAreIdenticalForProduction(
			IList<RelativityObject> expected,
			IList<RelativityObjectSlimDto> actual,
			IEnumerable<string> fieldNames)
		{
			var fieldValues = fieldNames
				.Select(fieldName => expected
					.Zip(
						actual,
						(relObject, relObjectSlim) => new
						{
							ExpectedValue = relObject[fieldName].Value,
							ActualValue = relObjectSlim.FieldValues[fieldName]
						}))
				.SelectMany(x => x);

			foreach (var fieldValue in fieldValues)
			{
				if (fieldValue.ActualValue is IEnumerable actualValueAsEnumerable &&
				    fieldValue.ExpectedValue is IEnumerable expectedValueAsEnumerable)
				{
					AssertMultipleObjectFieldsAreIdentical(actualValueAsEnumerable, expectedValueAsEnumerable);
				}
				else
				{
					fieldValue.ActualValue.ToString().Should().Be(fieldValue.ExpectedValue.ToString());
				}
			}
		}

		private void AssertMultipleObjectFieldsAreIdentical(IEnumerable actualValue, IEnumerable expectedValue)
		{
			var innerValues = actualValue.Cast<object>()
				.Zip(expectedValue.Cast<object>(), (actualInner, expectedInner) => new
				{
					Actual = actualInner?.ToString() ?? string.Empty,
					Expected = expectedInner?.ToString() ?? string.Empty
				});
			foreach (var innerValue in innerValues)
			{
				innerValue.Actual.Should().Be(innerValue.Expected);
			}
		}
	}
}
