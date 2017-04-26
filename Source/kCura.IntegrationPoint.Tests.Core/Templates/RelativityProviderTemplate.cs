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
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using NSubstitute;
using Relativity.Data.Toggles;
using Relativity.Services.Folder;
using Relativity.Toggles;

namespace kCura.IntegrationPoint.Tests.Core.Templates
{
	[TestFixture]
	public abstract class RelativityProviderTemplate : SourceProviderTemplate
	{
		private readonly string _targetWorkspaceName;
		private readonly string _targetWorkspaceTemplate;
		private string _destinationConfig;
		protected SourceProvider RelativityProvider;
		protected SourceProvider LdapProvider;
		protected IRepositoryFactory RepositoryFactory;

		public int SourceWorkspaceArtifactId { get; protected set; }
		public int TargetWorkspaceArtifactId { get; protected set; }
		public int SavedSearchArtifactId { get; set; }
		public int FolderArtifactId { get; set; }

		public RelativityProviderTemplate(string sourceWorkspaceName, string targetWorkspaceName,
			string sourceWorkspaceTemplate = WorkspaceTemplates.NEW_CASE_TEMPLATE,
			string targetWorkspaceTemplate = WorkspaceTemplates.NEW_CASE_TEMPLATE)
			: base(sourceWorkspaceName, sourceWorkspaceTemplate)
		{
			_targetWorkspaceName = targetWorkspaceName;
			_targetWorkspaceTemplate = targetWorkspaceTemplate;
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			try
			{
				SourceWorkspaceArtifactId = WorkspaceArtifactId;

				Task.Run(async () => await SetupAsync()).Wait();

				RelativityProvider = SourceProviders.First(provider => provider.Name == "Relativity");
				LdapProvider = SourceProviders.First(provider => provider.Name == "LDAP");
			}
			catch (Exception setupException)
			{
				try
				{
					SuiteTeardown();
				}
				catch (Exception teardownException)
				{
					Exception[] exceptions = new[] { setupException, teardownException };
					throw new AggregateException(exceptions);
				}
				throw;
			}
			RepositoryFactory = Container.Resolve<IRepositoryFactory>();

			IToggleProvider toggleProviderMock = Substitute.For<IToggleProvider>();
			toggleProviderMock.IsEnabled<AOAGToggle>().Returns(true);
			ToggleProvider.Current = toggleProviderMock;
		}

		public override void SuiteTeardown()
		{
			Workspace.DeleteWorkspace(TargetWorkspaceArtifactId);
			base.SuiteTeardown();
		}

		#region Helper Methods

		protected string CreateDefaultSourceConfig()
		{
			if (_destinationConfig == null)
			{
				_destinationConfig = CreateSourceConfigWithTargetWorkspace(TargetWorkspaceArtifactId);
			}
			return _destinationConfig;
		}

		protected string CreateSourceConfigWithTargetWorkspace(int targetWorkspaceId)
		{
			return $"{{\"SavedSearchArtifactId\":{SavedSearchArtifactId},\"SourceWorkspaceArtifactId\":\"{SourceWorkspaceArtifactId}\",\"TargetWorkspaceArtifactId\":{targetWorkspaceId},\"FolderArtifactId\":{GetRootFolder(Helper, targetWorkspaceId)}}}";
		}

		protected string CreateDestinationConfig(ImportOverwriteModeEnum overwriteMode, int? federatedInstanceArtifactId = null)
		{
			return CreateDestinationConfigWithTargetWorkspace(overwriteMode, SourceWorkspaceArtifactId, federatedInstanceArtifactId);
		}

		protected string CreateDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum overwriteMode, int targetWorkspaceId, int? federatedInstanceArtifactId = null)
		{
			ImportSettings destinationConfig = new ImportSettings
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
			return Container.Resolve<ISerializer>().Serialize(destinationConfig);
		}

		protected string CreateDefaultFieldMap()
		{
			FieldMap[] map = GetDefaultFieldMap();
			return Container.Resolve<ISerializer>().Serialize(map);
		}

		protected FieldMap[] GetDefaultFieldMap()
		{
			IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
			IFieldQueryRepository sourceFieldQueryRepository = repositoryFactory.GetFieldQueryRepository(SourceWorkspaceArtifactId);
			IFieldQueryRepository destinationFieldQueryRepository = repositoryFactory.GetFieldQueryRepository(TargetWorkspaceArtifactId);

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

		protected new async Task SetupAsync()
		{
			TargetWorkspaceArtifactId = String.IsNullOrEmpty(_targetWorkspaceName)
				? SourceWorkspaceArtifactId
				: await Task.Run(() => Workspace.CreateWorkspace(_targetWorkspaceName, _targetWorkspaceTemplate));

			SavedSearchArtifactId = await Task.Run(() => SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactId, "All documents"));
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
			modelBase.DestinationProvider = DestinationProvider.ArtifactId;
			modelBase.SourceProvider = RelativityProvider.ArtifactId;
			modelBase.SourceConfiguration = CreateDefaultSourceConfig();
			modelBase.LogErrors = true;
			modelBase.Name = name + DateTime.Now;
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
			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(overwriteMode),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = name + DateTime.Now,
				SelectedOverwrite = overwrite,
				Scheduler = new Scheduler()
				{
					EnableScheduler = true,
					//Date format "MM/dd/yyyy". For testing purpose. No sanity check here
					StartDate = startDate,
					EndDate = endDate,
					ScheduledTime = DateTime.UtcNow.Hour + ":" + DateTime.UtcNow.AddMinutes(1),
					Reoccur = 0,
					SelectedFrequency = interval.ToString()
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			return integrationModel;
		}
		
		#endregion Helper Methods
	}
}