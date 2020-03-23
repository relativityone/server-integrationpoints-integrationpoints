using Relativity.API;
using Relativity.Testing.Identification;
using System;
using System.Linq;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoint.Tests.Core.Templates;
using NSubstitute;
using NUnit.Framework;
using kCura.IntegrationPoint.Tests.Core;
using Relativity.IntegrationPoints.Services;
using IntegrationPointModel = kCura.IntegrationPoints.Core.Models.IntegrationPointModel;


namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class IntegrationPointRepositoryTests : RelativityProviderTemplate
	{
		private int _longTextLimit;
		private ISerializer _serializer;
		private IntegrationPointRepository _instance;

		private const string _LONG_TEXT_LIMIT_SECTION = "kCura.EDDS.Web";
		private const string _LONG_TEXT_LIMIT_NAME = "MaximumNumberOfCharactersSupportedByLongText";

		public IntegrationPointRepositoryTests() : base(nameof(IntegrationPointRepositoryTests), null)
		{ }

		public override void TestSetup()
		{
			IInstanceSettingRepository instanceSettings = Container.Resolve<IRepositoryFactory>().GetInstanceSettingRepository();
			_longTextLimit = Convert.ToInt32(instanceSettings.GetConfigurationValue(_LONG_TEXT_LIMIT_SECTION, _LONG_TEXT_LIMIT_NAME));

			_serializer = Container.Resolve<ISerializer>();

			IHelper helper = Container.Resolve<IHelper>();
			IAPILog logger = Substitute.For<IAPILog>();

			_instance = new IntegrationPointRepository(
				new RelativityObjectManagerFactory(helper).CreateRelativityObjectManager(SourceWorkspaceArtifactID),
				new IntegrationPointSerializer(logger),
				Substitute.For<ISecretsRepository>(),
				logger
			);
		}

		[IdentifiedTest("c63cbf71-8a87-4b19-90ec-6230d563b084")]
		public void GetAllBySourceAndDestinationProviderIDsAsync_ShouldRetrieveDeserializableSourceConfiguration_WhenSourceConfigurationExceedsLongTextLimit()
		{
			// Arrange
			IntegrationPointModel createdIntegrationPoint = CreateOrUpdateIntegrationPoint(new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = _serializer.Serialize(new
				{
					SavedSearchArtifactId = SavedSearchArtifactID,
					SourceWorkspaceArtifactId = SourceWorkspaceArtifactID,
					TargetWorkspaceArtifactId = TargetWorkspaceArtifactID,
					TypeOfExport = 3,
					Filler = new String(Enumerable.Repeat('-', _longTextLimit).ToArray())
				}),
				LogErrors = true,
				Name = $"{nameof(IntegrationPointRepositoryTests)}{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler { EnableScheduler = false },
				HasErrors = true,
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			});

			// Act
			IntegrationPoint retrievedIntegrationPoint = _instance.GetAllBySourceAndDestinationProviderIDsAsync(RelativityProvider.ArtifactId, RelativityDestinationProviderArtifactId)
				.GetAwaiter()
				.GetResult()
				.First(ip => ip.ArtifactId == createdIntegrationPoint.ArtifactID);

			// Assert
			Assert.DoesNotThrow(() => _serializer.Deserialize<IDictionary<string, string>>(retrievedIntegrationPoint.SourceConfiguration));
		}

		[IdentifiedTest("35e575f2-3eb5-4998-a648-8d2ba7ef5b34")]
		public void GetAllBySourceAndDestinationProviderIDsAsync_ShouldRetrieveDeserializableDestinationConfiguration_WhenDestinationConfigurationExceedsLongTextLimit()
		{
			// Arrange
			IntegrationPointModel createdIntegrationPoint = CreateOrUpdateIntegrationPoint(new IntegrationPointModel
			{
				Destination = _serializer.Serialize(new
				{
					ArtifactTypeId = 10,
					CaseArtifactId = SourceWorkspaceArtifactID,
					Provider = "Relativity",
					ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
					ImportNativeFile = false,
					ExtractedTextFieldContainsFilePath = false,
					FieldOverlayBehavior = "Use Field Settings",
					RelativityUsername = SharedVariables.RelativityUserName,
					RelativityPassword = SharedVariables.RelativityPassword,
					DestinationProviderType = "74A863B9-00EC-4BB7-9B3E-1E22323010C6",
					DestinationFolderArtifactId = GetRootFolder(Helper, SourceWorkspaceArtifactID),
					Filler = new String(Enumerable.Repeat('-', _longTextLimit).ToArray())
				}),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"{nameof(IntegrationPointRepositoryTests)}{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler { EnableScheduler = false },
				HasErrors = true,
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			});

			// Act
			IntegrationPoint retrievedIntegrationPoint = _instance.GetAllBySourceAndDestinationProviderIDsAsync(RelativityProvider.ArtifactId, RelativityDestinationProviderArtifactId)
				.GetAwaiter()
				.GetResult()
				.First(ip => ip.ArtifactId == createdIntegrationPoint.ArtifactID);

			// Assert
			Assert.DoesNotThrow(() => _serializer.Deserialize<IDictionary<string, string>>(retrievedIntegrationPoint.DestinationConfiguration));
		}

		[IdentifiedTest("37be6feb-5e21-491f-a5bd-d0b6f951ac60")]
		public void GetAllBySourceAndDestinationProviderIDsAsync_ShouldRetrieveDeserializableFieldMappings_WhenFieldMappingsExceedsLongTextLimit()
		{
			// Arrange
			FieldMap[] fieldMappings = GetDefaultFieldMap();
			fieldMappings[0].SourceField.DisplayName = new String(Enumerable.Repeat('-', _longTextLimit / 2).ToArray());
			fieldMappings[0].DestinationField.DisplayName = new String(Enumerable.Repeat('-', _longTextLimit / 2).ToArray());

			IntegrationPointModel createdIntegrationPoint = CreateOrUpdateIntegrationPoint(new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"{nameof(IntegrationPointRepositoryTests)}{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler { EnableScheduler = false },
				HasErrors = true,
				Map = _serializer.Serialize(fieldMappings),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			});
			
			// Act
			IntegrationPoint retrievedIntegrationPoint = _instance.GetAllBySourceAndDestinationProviderIDsAsync(RelativityProvider.ArtifactId, RelativityDestinationProviderArtifactId)
				.GetAwaiter()
				.GetResult()
				.First(ip => ip.ArtifactId == createdIntegrationPoint.ArtifactID);

			// Assert
			Assert.DoesNotThrow(() => _serializer.Deserialize<FieldMap[]>(retrievedIntegrationPoint.FieldMappings));
		}
	}
}
