using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Windsor;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.Services.Folder;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects.DataContracts;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace Rip.SystemTests.IntegrationPointServices
{
	[TestFixture]
	public class IntegrationPointServiceTests
	{
		private const string _VERY_LONG_FIELD_NAME_PREFIX = "Very_Long_Field_Name_0000000000000000000000000";
		private const int _VERY_LONG_FIELD_NAME_COUNT = 500;
		private const string _RELATIVITY_PROVIDER_NAME = "Relativity";
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = 10;
		private const string _ALL_DOCUMENTS_SAVED_SEARCH_NAME = "All documents";

		private int _sourceWorkspaceID => SystemTestsFixture.WorkspaceID;
		private int _destinationWorkspaceID => SystemTestsFixture.DestinationWorkspaceID;
		private int _savedSearchArtifactID;
		private int _integrationPointExportType;
		private IEnumerable<SourceProvider> _sourceProviders;
		private IEnumerable<DestinationProvider> _destinationProviders;

		private IWindsorContainer _container => SystemTestsFixture.Container;
		private ITestHelper _testHelper => SystemTestsFixture.TestHelper;
		private IIntegrationPointService _integrationPointService;
		private IRepositoryFactory _repositoryFactory;
		private IFieldManager _fieldManager;
		private ICaseServiceContext _caseServiceContext;
		private IRelativityObjectManager _objectManager;
		private IFolderManager _folderManager;
		private ISerializer _serializer;
		private IIntegrationPointTypeService _typeService;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			_integrationPointService = _container.Resolve<IIntegrationPointService>();
			_repositoryFactory = _container.Resolve<IRepositoryFactory>();
			_fieldManager = _testHelper.CreateUserProxy<IFieldManager>();
			_caseServiceContext = _container.Resolve<ICaseServiceContext>();
			_objectManager = _caseServiceContext.RsapiService.RelativityObjectManager;
			_folderManager = _testHelper.CreateAdminProxy<IFolderManager>();
			_serializer = _container.Resolve<ISerializer>();
			_typeService = _container.Resolve<IIntegrationPointTypeService>();

			_sourceProviders = GetSourceProviders();
			_destinationProviders = GetDestinationProviders();
			_savedSearchArtifactID = Task.Run(() => SavedSearch.CreateSavedSearch(_sourceWorkspaceID, _ALL_DOCUMENTS_SAVED_SEARCH_NAME)).Result;
			_integrationPointExportType = _typeService.GetIntegrationPointType(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId;
		}

		[Test]
		public void IntegrationPointShouldBeSavedAndRetrievedProperly_WhenFieldMappingJsonIsLongerThan10000()
		{
			// arrange
			Guid guid = Guid.Parse("DAFFB7F6-363A-470A-B0E8-213CADA59963");
			string integrationPointName = $"{guid}-{DateTime.UtcNow.Ticks}";
			IList<FieldEntry> sourceFields = CreateVeryLongNameFields(_sourceWorkspaceID);
			IList<FieldEntry> destinationFields = CreateVeryLongNameFields(_destinationWorkspaceID);
			string fieldMapping = CreateVeryLongFieldMapping(sourceFields, destinationFields);

			// act
			int integrationPointArtifactID = CreateRelativityProviderIntegrationPoint(integrationPointName, fieldMapping);
			IntegrationPointModel retrievedIntegrationPoint = RetrieveIntegrationPoint(integrationPointArtifactID);

			// assert
			retrievedIntegrationPoint.Map.Should().Be(fieldMapping);
		}

		private IEnumerable<SourceProvider> GetSourceProviders()
		{
			var queryRequest = new QueryRequest();
			return _objectManager.Query<SourceProvider>(queryRequest);
		}

		private IEnumerable<DestinationProvider> GetDestinationProviders()
		{
			var queryRequest = new QueryRequest();
			return _objectManager.Query<DestinationProvider>(queryRequest);
		}

		private IList<FieldEntry> CreateVeryLongNameFields(int workspaceID)
		{
			var fieldEntries = new List<FieldEntry>(_VERY_LONG_FIELD_NAME_COUNT);

			for (int i = 0; i < _VERY_LONG_FIELD_NAME_COUNT; i++)
			{
				string fieldName = CreateVeryLongFieldName(i);
				var fixedLengthFieldRequest = new FixedLengthFieldRequest
				{
					ObjectType = new ObjectTypeIdentifier {ArtifactTypeID = (int) ArtifactType.Document},
					Name = fieldName,
					Length = 255,
					IsRequired = false,
					IncludeInTextIndex = false,
					HasUnicode = true,
					AllowHtml = false,
					OpenToAssociations = false
				};

				int fieldArtifactID = _fieldManager.CreateFixedLengthFieldAsync(workspaceID, fixedLengthFieldRequest)
						.GetAwaiter().GetResult();

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

		private static string CreateVeryLongFieldName(int id)
		{
			return $"{_VERY_LONG_FIELD_NAME_PREFIX}{id}";
		}

		private string CreateVeryLongFieldMapping(IList<FieldEntry> sourceFieldEntries, IList<FieldEntry> destinationFieldEntries)
		{
			var fieldMapping = new List<FieldMap>(_VERY_LONG_FIELD_NAME_COUNT + 1);
			var identifierFieldMap = CreateIdentifierFieldMap();
			fieldMapping.Add(identifierFieldMap);

			for (int i = 0; i < _VERY_LONG_FIELD_NAME_COUNT; i++)
			{
				var fieldMap = new FieldMap
				{
					SourceField = sourceFieldEntries[i],
					DestinationField = destinationFieldEntries[i],
					FieldMapType = FieldMapTypeEnum.None
				};
				fieldMapping.Add(fieldMap);
			}

			FieldMap[] fieldMappingArray = fieldMapping.ToArray();
			return _serializer.Serialize(fieldMappingArray);
		}

		private FieldMap CreateIdentifierFieldMap()
		{
			var sourceDto = RetrieveIdentifierField(_sourceWorkspaceID);
			var destinationDto = RetrieveIdentifierField(_destinationWorkspaceID);

			var fieldMap = new FieldMap()
			{
				SourceField = CreateIdentifierFieldEntry(sourceDto),
				DestinationField = CreateIdentifierFieldEntry(destinationDto),
				FieldMapType = FieldMapTypeEnum.Identifier
			};

			return fieldMap;
		}

		private static FieldEntry CreateIdentifierFieldEntry(ArtifactDTO fieldDto)
		{
			return new FieldEntry
			{
				FieldIdentifier = fieldDto.ArtifactId.ToString(),
				DisplayName = fieldDto.Fields.First(field => field.Name == "Name").Value as string + " [Object Identifier]",
				IsIdentifier = true
			};
		}

		private ArtifactDTO RetrieveIdentifierField(int workspaceID)
		{
			IFieldQueryRepository fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(workspaceID);
			return fieldQueryRepository.RetrieveTheIdentifierField((int) ArtifactType.Document);
		}

		private int CreateRelativityProviderIntegrationPoint(string name, string fieldMapping)
		{
			SourceProvider relativityProvider = _sourceProviders.First(provider => provider.Name == _RELATIVITY_PROVIDER_NAME);
			DestinationProvider destinationProvider = _destinationProviders.First(provider => provider.Name == _RELATIVITY_PROVIDER_NAME);

			var integrationPointModel = new IntegrationPointModel
			{
				Name = name,
				SourceProvider = relativityProvider.ArtifactId,
				SourceConfiguration = CreateRelativityProviderSavedSearchSourceConfiguration(),
				DestinationProvider = destinationProvider.ArtifactId,
				Destination = CreateRelativityProviderDestinationConfiguration(),
				Map = fieldMapping,
				Scheduler = new Scheduler
				{
					EnableScheduler = false
				},
				SelectedOverwrite = "Append Only",
				Type = _integrationPointExportType
			};

			return SaveIntegrationPoint(integrationPointModel);
		}

		private string CreateRelativityProviderSavedSearchSourceConfiguration()
		{
			var configuration = new SourceConfiguration
			{
				SavedSearchArtifactId = _savedSearchArtifactID,
				SourceWorkspaceArtifactId = _sourceWorkspaceID,
				TargetWorkspaceArtifactId = _destinationWorkspaceID,
				TypeOfExport = SourceConfiguration.ExportType.SavedSearch
			};

			return _serializer.Serialize(configuration);
		}

		private string CreateRelativityProviderDestinationConfiguration()
		{
			var configuration = new ImportSettings
			{
				ArtifactTypeId = _DOCUMENT_ARTIFACT_TYPE_ID,
				CaseArtifactId = _destinationWorkspaceID,
				Provider = _RELATIVITY_PROVIDER_NAME,
				ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
				ImportNativeFile = false,
				ExtractedTextFieldContainsFilePath = false,
				FieldOverlayBehavior = ImportSettings.FIELDOVERLAYBEHAVIOR_DEFAULT,
				RelativityUsername = SharedVariables.RelativityUserName,
				RelativityPassword = SharedVariables.RelativityPassword,
				DestinationProviderType = Constants.IntegrationPoints.DestinationProviders.RELATIVITY,
				DestinationFolderArtifactId = GetDestinationWorkspaceRootFolderID(),
				FederatedInstanceArtifactId = null
			};

			return _serializer.Serialize(configuration);
		}

		private int GetDestinationWorkspaceRootFolderID()
		{
			return _folderManager.GetWorkspaceRootAsync(_destinationWorkspaceID).Result.ArtifactID;
		}

		private int SaveIntegrationPoint(IntegrationPointModel integrationPointModel)
		{
			return _integrationPointService.SaveIntegration(integrationPointModel);
		}

		private IntegrationPointModel RetrieveIntegrationPoint(int artifactID)
		{
			return _integrationPointService.ReadIntegrationPointModel(artifactID);
		}
	}
}
