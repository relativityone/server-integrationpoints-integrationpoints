﻿using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using NSubstitute;
using Relativity.Services.Folder;
using Relativity.Toggles;

namespace kCura.IntegrationPoint.Tests.Core.Templates
{
	[TestFixture]
	public abstract class RelativityProviderTemplate : SourceProviderTemplate
	{
		private string _destinationConfig;

		private readonly string _targetWorkspaceName;
		private readonly string _targetWorkspaceTemplate;

		protected SourceProvider RelativityProvider { get; private set; }
		protected SourceProvider LdapProvider { get; private set; }
		protected IRepositoryFactory RepositoryFactory { get; private set; }

		public int SourceWorkspaceArtifactID { get; private set; }
		public int TargetWorkspaceArtifactID { get; private set; }
		public int SavedSearchArtifactID { get; set; }
		public int TypeOfExport { get; set; }
		public int FolderArtifactID { get; set; }

		public RelativityProviderTemplate(
			string sourceWorkspaceName,
			string targetWorkspaceName,
			string sourceWorkspaceTemplate = WorkspaceTemplates.NEW_CASE_TEMPLATE,
			string targetWorkspaceTemplate = WorkspaceTemplates.NEW_CASE_TEMPLATE)
			: base(sourceWorkspaceName, sourceWorkspaceTemplate)
		{
			_targetWorkspaceName = targetWorkspaceName;
			_targetWorkspaceTemplate = targetWorkspaceTemplate;
		}

		protected RelativityProviderTemplate(
			int sourceWorkspaceArtifactID,
			string targetWorkspaceName,
			string targetWorkspaceTemplate = WorkspaceTemplates.NEW_CASE_TEMPLATE)
			: base(sourceWorkspaceArtifactID)
		{
			_targetWorkspaceName = targetWorkspaceName;
			_targetWorkspaceTemplate = targetWorkspaceTemplate;
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			SourceWorkspaceArtifactID = WorkspaceArtifactId;

			Task.Run(async () => await SetupAsync().ConfigureAwait(false)).Wait();

			RelativityProvider = SourceProviders.First(provider => provider.Name == "Relativity");
			LdapProvider = SourceProviders.First(provider => provider.Name == "LDAP");

			RepositoryFactory = Container.Resolve<IRepositoryFactory>();

			IToggleProvider toggleProviderMock = Substitute.For<IToggleProvider>();
			ToggleProvider.Current = toggleProviderMock;
		}

		public override void SuiteTeardown()
		{
			Workspace.DeleteWorkspace(TargetWorkspaceArtifactID);
			base.SuiteTeardown();
		}

		#region Helper Methods

		protected string CreateDefaultSourceConfig()
		{
			if (_destinationConfig == null)
			{
				_destinationConfig = CreateSourceConfigWithTargetWorkspace(TargetWorkspaceArtifactID);
			}
			return _destinationConfig;
		}

		protected string CreateSourceConfigWithTargetWorkspace(int targetWorkspaceId)
		{
			return CreateSourceConfigWithCustomParameters(
				targetWorkspaceId,
				SavedSearchArtifactID,
				SourceWorkspaceArtifactID,
				SourceConfiguration.ExportType.SavedSearch);
		}

		protected string CreateSourceConfigWithCustomParameters(
			int targetWorkspaceId,
			int savedSearchArtifactId,
			int sourceWorkspaceArtifactId,
			SourceConfiguration.ExportType exportType)
		{
			var sourceConfiguration = new SourceConfiguration
			{
				SavedSearchArtifactId = savedSearchArtifactId,
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				TargetWorkspaceArtifactId = targetWorkspaceId,
				TypeOfExport = exportType
			};
			return Container.Resolve<ISerializer>().Serialize(sourceConfiguration);
		}

		protected string CreateDestinationConfig(ImportOverwriteModeEnum overwriteMode, int? federatedInstanceArtifactId = null)
		{
			return CreateSerializedDestinationConfigWithTargetWorkspace(overwriteMode, SourceWorkspaceArtifactID, federatedInstanceArtifactId);
		}

		protected string CreateSerializedDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum overwriteMode, int targetWorkspaceId, int? federatedInstanceArtifactId = null)
		{
			ImportSettings destinationConfig = CreateDestinationConfigWithTargetWorkspace(overwriteMode, targetWorkspaceId, federatedInstanceArtifactId);
			return Container.Resolve<ISerializer>().Serialize(destinationConfig);
		}

		protected ImportSettings CreateDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum overwriteMode, int targetWorkspaceId, int? federatedInstanceArtifactId = null)
		{
			return new ImportSettings
			{
				ArtifactTypeId = 10,
				CaseArtifactId = targetWorkspaceId,
				Provider = "Relativity",
				ImportOverwriteMode = overwriteMode,
				ImportNativeFile = false,
				ExtractedTextFieldContainsFilePath = false,
				FieldOverlayBehavior = "Use Field Settings",
				RelativityUsername = SharedVariables.RelativityUserName,
				RelativityPassword = SharedVariables.RelativityPassword,
				DestinationProviderType = "74A863B9-00EC-4BB7-9B3E-1E22323010C6",
				DestinationFolderArtifactId = GetRootFolder(Helper, targetWorkspaceId),
				FederatedInstanceArtifactId = federatedInstanceArtifactId
			};
		}

		protected string CreateDefaultFieldMap()
		{
			FieldMap[] map = GetDefaultFieldMap();
			return Container.Resolve<ISerializer>().Serialize(map);
		}

		protected FieldMap[] GetDefaultFieldMap()
		{
			IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
			IFieldQueryRepository sourceFieldQueryRepository = repositoryFactory.GetFieldQueryRepository(SourceWorkspaceArtifactID);
			IFieldQueryRepository destinationFieldQueryRepository = repositoryFactory.GetFieldQueryRepository(TargetWorkspaceArtifactID);

			ArtifactDTO sourceDto = sourceFieldQueryRepository.RetrieveTheIdentifierField((int)global::kCura.Relativity.Client.ArtifactType.Document);
			ArtifactDTO targetDto = destinationFieldQueryRepository.RetrieveTheIdentifierField((int)global::kCura.Relativity.Client.ArtifactType.Document);

			FieldMap[] map = new[]
			{
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						FieldIdentifier = sourceDto.ArtifactId.ToString(),
						DisplayName = sourceDto.Fields.First(field => field.Name == "Name").Value as string + " [Object Identifier]",
						IsIdentifier = true,
					},
					FieldMapType = FieldMapTypeEnum.Identifier,
					DestinationField = new FieldEntry()
					{
						FieldIdentifier = targetDto.ArtifactId.ToString(),
						DisplayName = targetDto.Fields.First(field => field.Name == "Name").Value as string + " [Object Identifier]",
						IsIdentifier = true,
					},
				}
			};
			return map;
		}

		private async Task SetupAsync()
		{
			TargetWorkspaceArtifactID = string.IsNullOrEmpty(_targetWorkspaceName)
				? SourceWorkspaceArtifactID
				: await Task.Run(() => Workspace.CreateWorkspace(_targetWorkspaceName, _targetWorkspaceTemplate)).ConfigureAwait(false);

			SavedSearchArtifactID = await Task.Run(() => SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactID, "All documents")).ConfigureAwait(false);
			TypeOfExport = (int)SourceConfiguration.ExportType.SavedSearch;
		}

		protected IntegrationPointModel CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum overwriteMode, string name, string overwrite, bool promoteEligible = true)
		{
			IntegrationPointModel integrationModel = new IntegrationPointModel();
			SetIntegrationPointBaseModelProperties(integrationModel, overwriteMode, name, overwrite, promoteEligible);
			return integrationModel;
		}

		protected IntegrationPointProfileModel CreateDefaultIntegrationPointProfileModel(ImportOverwriteModeEnum overwriteMode, string name, string overwrite, bool promoteEligible)
		{
			IntegrationPointProfileModel integrationModel = new IntegrationPointProfileModel();
			SetIntegrationPointBaseModelProperties(integrationModel, overwriteMode, name, overwrite, promoteEligible);
			return integrationModel;
		}

		private void SetIntegrationPointBaseModelProperties(IntegrationPointModelBase modelBase, ImportOverwriteModeEnum overwriteMode, string name, string overwrite, bool promoteEligible)
		{
			modelBase.Destination = CreateDestinationConfig(overwriteMode);
			modelBase.DestinationProvider = RelativityDestinationProviderArtifactId;
			modelBase.SourceProvider = RelativityProvider.ArtifactId;
			modelBase.SourceConfiguration = CreateDefaultSourceConfig();
			modelBase.LogErrors = true;
			modelBase.Name = $"{name}{DateTime.Now:yy-MM-dd HH-mm-ss}";
			modelBase.SelectedOverwrite = overwrite;
			modelBase.Scheduler = new Scheduler() { EnableScheduler = false };
			modelBase.Map = CreateDefaultFieldMap();
			modelBase.Type =
				Container.Resolve<IIntegrationPointTypeService>()
					.GetIntegrationPointType(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid)
					.ArtifactId;
			modelBase.PromoteEligible = promoteEligible;
		}

		private static int GetRootFolder(ITestHelper helper, int workspaceArtifactId)
		{
			using (var folderManager = helper.CreateAdminProxy<IFolderManager>())
			{
				return folderManager.GetWorkspaceRootAsync(workspaceArtifactId).Result.ArtifactID;
			}
		}

		protected IntegrationPointModel CreateDefaultIntegrationPointModelScheduled(ImportOverwriteModeEnum overwriteMode, string name, string overwrite, string startDate, string endDate, ScheduleInterval interval)
		{
			const int offsetInSeconds = 30;
			DateTime newScheduledTime = DateTime.UtcNow.AddSeconds(offsetInSeconds);

			var integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(overwriteMode),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"{name}{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = overwrite,
				Scheduler = new Scheduler()
				{
					EnableScheduler = true,
					//Date format "MM/dd/yyyy". For testing purpose. No sanity check here
					StartDate = startDate,
					EndDate = endDate,
					ScheduledTime = newScheduledTime.ToString("HH:mm:ss"),
					Reoccur = 0,
					SelectedFrequency = interval.ToString(),
					TimeZoneId = TimeZoneInfo.Utc.Id
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			return integrationModel;
		}

		#endregion Helper Methods
	}
}