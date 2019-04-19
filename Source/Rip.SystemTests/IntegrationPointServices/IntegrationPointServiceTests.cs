using System;
using System.Collections.Generic;
using Castle.Windsor;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.Services.Folder;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Rip.SystemTests.Utilities;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace Rip.SystemTests.IntegrationPointServices
{
	[TestFixture]
	public class IntegrationPointServiceTests
	{
		private const string _VERY_LONG_FIELD_NAME_PREFIX = "Very_Long_Field_Name_0000000000000000000000000";
		private const int _VERY_LONG_FIELD_NAME_COUNT = 5;
		private const string _ALL_DOCUMENTS_SAVED_SEARCH_NAME = "All documents";

		private int _sourceWorkspaceID => SystemTestsFixture.WorkspaceID;
		private int _destinationWorkspaceID => SystemTestsFixture.DestinationWorkspaceID;
		private int _savedSearchArtifactID;
		private int _integrationPointExportType;

		private IWindsorContainer _container => SystemTestsFixture.Container;
		private ITestHelper _testHelper => SystemTestsFixture.TestHelper;
		private IIntegrationPointService _integrationPointService;
		private IRepositoryFactory _repositoryFactory;
		private IFieldManager _fieldManager;
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
			_objectManager = _container.Resolve<IRelativityObjectManager>();
			_folderManager = _testHelper.CreateAdminProxy<IFolderManager>();
			_serializer = _container.Resolve<ISerializer>();
			_typeService = _container.Resolve<IIntegrationPointTypeService>();

			_savedSearchArtifactID = SavedSearch.CreateSavedSearch(_sourceWorkspaceID, _ALL_DOCUMENTS_SAVED_SEARCH_NAME);
			_integrationPointExportType = _typeService.GetIntegrationPointType(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId;
		}

		[Test]
		public void IntegrationPointShouldBeSavedAndRetrievedProperly_WhenFieldMappingJsonIsLongerThan10000()
		{
			// arrange
			string integrationPointName = $"DAFFB7F6-363A-470A-B0E8-213CADA59963-{DateTime.UtcNow.Ticks}";
			IList<FieldEntry> sourceFields = CreateVeryLongNameFields(_sourceWorkspaceID);
			IList<FieldEntry> destinationFields = CreateVeryLongNameFields(_destinationWorkspaceID);

			var fieldMappingBuilder = new FieldMappingBuilder(_repositoryFactory);
			FieldMap[] fieldMapping = fieldMappingBuilder
				.WithSourceWorkspaceID(_sourceWorkspaceID)
				.WithDestinationWorkspaceID(_destinationWorkspaceID)
				.WithSourceFields(sourceFields)
				.WithDestinationFields(destinationFields)
				.Build();

			// act
			int integrationPointArtifactID = CreateRelativityProviderIntegrationPoint(integrationPointName, fieldMapping);
			IntegrationPointModel retrievedIntegrationPoint = RetrieveIntegrationPoint(integrationPointArtifactID);

			// assert
			string expectedFieldMapping = _serializer.Serialize(fieldMapping);
			retrievedIntegrationPoint.Map.Should().Be(expectedFieldMapping);
		}

		private IList<FieldEntry> CreateVeryLongNameFields(int workspaceID)
		{
			var fieldEntries = new List<FieldEntry>(_VERY_LONG_FIELD_NAME_COUNT);

			for (int i = 0; i < _VERY_LONG_FIELD_NAME_COUNT; i++)
			{
				string fieldName = CreateVeryLongFieldName(i);
				var fixedLengthFieldRequest = new FixedLengthFieldRequest
				{
					ObjectType = new ObjectTypeIdentifier {ArtifactTypeID = kCura.IntegrationPoint.Tests.Core.Constants.DOCUMENT_ARTIFACT_TYPE_ID },
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

		private int CreateRelativityProviderIntegrationPoint(string name, FieldMap[] fieldMapping)
		{
			IntegrationPointModelBuilder builder = new IntegrationPointModelBuilder(_serializer, _objectManager);
			IntegrationPointModel integrationPointModel = builder
				.WithType(_integrationPointExportType)
				.WithName(name)
				.WithSourceProvider(Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME)
				.WithSourceConfiguration(CreateRelativityProviderSavedSearchSourceConfiguration())
				.WithDestinationProvider(Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME)
				.WithDestinationConfiguration(CreateRelativityProviderDestinationConfiguration())
				.WithFieldMapping(fieldMapping)
				.WithOverwriteMode(ImportOverwriteModeEnum.AppendOnly)
				.Build();

			return SaveIntegrationPoint(integrationPointModel);
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
				ArtifactTypeId = kCura.IntegrationPoint.Tests.Core.Constants.DOCUMENT_ARTIFACT_TYPE_ID,
				CaseArtifactId = _destinationWorkspaceID,
				Provider = Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME,
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

			return configuration;
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
