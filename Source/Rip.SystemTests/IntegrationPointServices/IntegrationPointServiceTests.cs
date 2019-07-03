﻿using System;
using System.Collections.Generic;
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
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.Services.Folder;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Rip.TestUtilities;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace Rip.SystemTests.IntegrationPointServices
{
	[TestFixture]
	public class IntegrationPointServiceTests
	{
		private const string _VERY_LONG_FIELD_NAME_PREFIX = "Very_Long_Field_Name_0000000000000000000000000";
		private const int _VERY_LONG_FIELD_NAME_COUNT = 500;
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

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			_integrationPointService = _container.Resolve<IIntegrationPointService>();
			_repositoryFactory = _container.Resolve<IRepositoryFactory>();
			_fieldManager = _testHelper.CreateProxy<IFieldManager>();
			_objectManager = _container.Resolve<IRelativityObjectManager>();
			_folderManager = _testHelper.CreateProxy<IFolderManager>();
			_serializer = _container.Resolve<ISerializer>();

			_savedSearchArtifactID = SavedSearch.CreateSavedSearch(_sourceWorkspaceID, _ALL_DOCUMENTS_SAVED_SEARCH_NAME);

			IIntegrationPointTypeService typeService = _container.Resolve<IIntegrationPointTypeService>();
			_integrationPointExportType = typeService
				.GetIntegrationPointType(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid)
				.ArtifactId;
		}

		[Test]
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
					ObjectType = new ObjectTypeIdentifier {ArtifactTypeID = kCura.IntegrationPoint.Tests.Core.Constants.DOCUMENT_ARTIFACT_TYPE_ID },
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
				.WithSourceProvider(Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME)
				.WithSourceConfiguration(CreateRelativityProviderSavedSearchSourceConfiguration())
				.WithDestinationProvider(Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME)
				.WithDestinationConfiguration(CreateRelativityProviderDestinationConfiguration())
				.WithFieldMapping(fieldMapping)
				.WithOverwriteMode(ImportOverwriteModeEnum.AppendOnly)
				.Build();

			return _integrationPointService.SaveIntegration(integrationPointModel);
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
