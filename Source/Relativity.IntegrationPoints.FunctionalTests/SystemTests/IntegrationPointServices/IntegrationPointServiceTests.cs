using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Windsor;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Folder;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Search;
using Relativity.Testing.Identification;
using Rip.TestUtilities;
using CoreConstants = kCura.IntegrationPoints.Core.Constants;
using FieldEntry = Relativity.IntegrationPoints.Contracts.Models.FieldEntry;
using IntegrationPointModel = kCura.IntegrationPoints.Core.Models.IntegrationPointModel;

namespace Relativity.IntegrationPoints.FunctionalTests.SystemTests.IntegrationPointServices
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class IntegrationPointServiceTests
	{
		private const string _VERY_LONG_FIELD_NAME_PREFIX = "Very_Long_Field_Name_0000000000000000000000000";
		private const int _VERY_LONG_FIELD_NAME_COUNT = 500;
		private const string _ALL_DOCUMENTS_SAVED_SEARCH_NAME = "All documents";

		private int _sourceWorkspaceID => SystemTestsSetupFixture.SourceWorkspace.ArtifactID;
		private int _destinationWorkspaceID => SystemTestsSetupFixture.DestinationWorkspace.ArtifactID;
		private int _savedSearchArtifactID;
		private int _integrationPointExportType;

		private IWindsorContainer _container => SystemTestsSetupFixture.Container;
		private ITestHelper _testHelper => SystemTestsSetupFixture.TestHelper;
		private IIntegrationPointService _integrationPointService;
		private IIntegrationPointRepository _integrationPointRepository;
		private IRepositoryFactory _repositoryFactory;
		private IFieldManager _fieldManager;
		private IRelativityObjectManager _objectManager;
		private IFolderManager _folderManager;
		private ISerializer _serializer;
		private IKeywordSearchManager _keywordSearchManager;
		private SavedSearchHelper _savedSearchHelper;
		private IList<int> _integrationPointArtifactIds;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			_integrationPointService = _container.Resolve<IIntegrationPointService>();
			_integrationPointRepository = _container.Resolve<IIntegrationPointRepository>();
			_repositoryFactory = _container.Resolve<IRepositoryFactory>();
			_fieldManager = _testHelper.CreateProxy<IFieldManager>();
			_objectManager = _container.Resolve<IRelativityObjectManager>();
			_folderManager = _testHelper.CreateProxy<IFolderManager>();
			_serializer = _container.Resolve<ISerializer>();
			_keywordSearchManager = _testHelper.CreateProxy<IKeywordSearchManager>();
			_savedSearchHelper = new SavedSearchHelper(_sourceWorkspaceID, _keywordSearchManager);

			_savedSearchArtifactID = SavedSearch.CreateSavedSearch(_sourceWorkspaceID, _ALL_DOCUMENTS_SAVED_SEARCH_NAME);

			IIntegrationPointTypeService typeService = _container.Resolve<IIntegrationPointTypeService>();
			_integrationPointExportType = typeService
				.GetIntegrationPointType(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid)
				.ArtifactId;

			_integrationPointArtifactIds = new List<int>();
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			_savedSearchHelper.DeleteSavedSearch(_savedSearchArtifactID);
			foreach (int integrationPointArtifactID in _integrationPointArtifactIds)
			{
				_integrationPointRepository.Delete(integrationPointArtifactID);
			}
		}

		[IdentifiedTest("b30513bf-e6b8-4680-a74b-d77b17976d20")]
		public async Task IntegrationPointShouldBeSavedAndRetrievedProperly_WhenFieldMappingJsonIsLongerThan10000()
		{
			// arrange
			string integrationPointName = $"DAFFB7F6-363A-470A-B0E8-213CADA59963-{DateTime.UtcNow.Ticks}";
			IList<FieldEntry> sourceFields = await CreateNameFields(_sourceWorkspaceID, _VERY_LONG_FIELD_NAME_COUNT).ConfigureAwait(false);
			IList<FieldEntry> destinationFields = await CreateNameFields(_destinationWorkspaceID, _VERY_LONG_FIELD_NAME_COUNT).ConfigureAwait(false);

			var fieldMappingBuilder = new FieldMappingBuilder(_repositoryFactory);
			FieldMap[] fieldMapping = fieldMappingBuilder
				.WithSourceWorkspaceID(_sourceWorkspaceID)
				.WithDestinationWorkspaceID(_destinationWorkspaceID)
				.WithSourceFields(sourceFields)
				.WithDestinationFields(destinationFields)
				.Build();

			// act
			int integrationPointArtifactID = CreateRelativityProviderIntegrationPoint(integrationPointName, fieldMapping);
			IntegrationPointModel retrievedIntegrationPoint = _integrationPointService.ReadIntegrationPointModel(integrationPointArtifactID);

			// assert
			string expectedFieldMapping = _serializer.Serialize(fieldMapping);
			retrievedIntegrationPoint.Map.Should().Be(expectedFieldMapping);
		}

		private async Task<IList<FieldEntry>> CreateNameFields(int workspaceID, int numberOfFields)
		{
			var fieldEntries = new List<FieldEntry>(numberOfFields);

			for (int i = 0; i < numberOfFields; i++)
			{
				string fieldName = CreateFieldNameLongerThan45Characters(i);
				var fixedLengthFieldRequest = new FixedLengthFieldRequest
				{
					ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = (int)ArtifactType.Document },
					Name = fieldName,
					Length = 255,
					IsRequired = false,
					IncludeInTextIndex = false,
					HasUnicode = true,
					AllowHtml = false,
					OpenToAssociations = false
				};

				int fieldArtifactID = await _fieldManager.CreateFixedLengthFieldAsync(workspaceID, fixedLengthFieldRequest).ConfigureAwait(false);

				var fieldEntry = new FieldEntry
				{
					FieldIdentifier = fieldArtifactID.ToString(),
					DisplayName = fieldName,
					IsIdentifier = false
				};

				fieldEntries.Add(fieldEntry);
			}

			return fieldEntries;
		}

		private static string CreateFieldNameLongerThan45Characters(int id)
		{
			return $"{_VERY_LONG_FIELD_NAME_PREFIX}{id}";
		}

		private int CreateRelativityProviderIntegrationPoint(string name, FieldMap[] fieldMapping)
		{
			IntegrationPointModelBuilder builder = new IntegrationPointModelBuilder(_serializer, _objectManager);
			IntegrationPointModel integrationPointModel = builder
				.WithType(_integrationPointExportType)
				.WithName(name)
				.WithSourceProvider(CoreConstants.IntegrationPoints.RELATIVITY_PROVIDER_NAME)
				.WithSourceConfiguration(CreateRelativityProviderSavedSearchSourceConfiguration())
				.WithDestinationProvider(CoreConstants.IntegrationPoints.RELATIVITY_PROVIDER_NAME)
				.WithDestinationConfiguration(CreateRelativityProviderDestinationConfiguration())
				.WithFieldMapping(fieldMapping)
				.WithOverwriteMode(ImportOverwriteModeEnum.AppendOnly)
				.Build();

			int integrationPointArtifactID = _integrationPointService.SaveIntegration(integrationPointModel);
			_integrationPointArtifactIds.Add(integrationPointArtifactID);
			return integrationPointArtifactID;
		}

		private SourceConfiguration CreateRelativityProviderSavedSearchSourceConfiguration()
		{
			var configuration = new SourceConfiguration
			{
				SavedSearchArtifactId = _savedSearchArtifactID,
				SourceWorkspaceArtifactId = _sourceWorkspaceID,
				TargetWorkspaceArtifactId = _destinationWorkspaceID,
				TypeOfExport = SourceConfiguration.ExportType.SavedSearch
			};

			return configuration;
		}

		private ImportSettings CreateRelativityProviderDestinationConfiguration()
		{
			var configuration = new ImportSettings
			{
				ArtifactTypeId = (int)ArtifactType.Document,
				CaseArtifactId = _destinationWorkspaceID,
				Provider = CoreConstants.IntegrationPoints.RELATIVITY_PROVIDER_NAME,
				ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
				ImportNativeFile = false,
				ExtractedTextFieldContainsFilePath = false,
				FieldOverlayBehavior = ImportSettings.FIELDOVERLAYBEHAVIOR_DEFAULT,
				RelativityUsername = SharedVariables.RelativityUserName,
				RelativityPassword = SharedVariables.RelativityPassword,
				DestinationProviderType = CoreConstants.IntegrationPoints.DestinationProviders.RELATIVITY,
				DestinationFolderArtifactId = GetDestinationWorkspaceRootFolderID().GetAwaiter().GetResult(),
				FederatedInstanceArtifactId = null
			};

			return configuration;
		}

		private async Task<int> GetDestinationWorkspaceRootFolderID()
		{
			Folder workspaceRoot = await _folderManager.GetWorkspaceRootAsync(_destinationWorkspaceID).ConfigureAwait(false);
			return workspaceRoot.ArtifactID;
		}
	}
}
