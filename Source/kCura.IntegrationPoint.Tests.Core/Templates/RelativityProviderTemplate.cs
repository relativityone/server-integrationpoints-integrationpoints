using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
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

        protected RelativityProviderTemplate(
            string sourceWorkspaceName,
            string targetWorkspaceName,
            string sourceWorkspaceTemplate = WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME,
            string targetWorkspaceTemplate = WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME)
            : base(sourceWorkspaceName, sourceWorkspaceTemplate)
        {
            _targetWorkspaceName = targetWorkspaceName;
            _targetWorkspaceTemplate = targetWorkspaceTemplate;
        }

        protected RelativityProviderTemplate(
            int sourceWorkspaceArtifactID,
            string targetWorkspaceName,
            string targetWorkspaceTemplate = WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME)
            : base(sourceWorkspaceArtifactID)
        {
            _targetWorkspaceName = targetWorkspaceName;
            _targetWorkspaceTemplate = targetWorkspaceTemplate;
        }

        public override void SuiteSetup()
        {
            base.SuiteSetup();

            SourceWorkspaceArtifactID = WorkspaceArtifactId;

            SetupAsync().GetAwaiter().GetResult();

            RelativityProvider = SourceProviders.First(provider => provider.Name == "Relativity");
            LdapProvider = SourceProviders.First(provider => provider.Name == "LDAP");

            RepositoryFactory = Container.Resolve<IRepositoryFactory>();

            IToggleProvider toggleProviderMock = Substitute.For<IToggleProvider>();
            ToggleProvider.Current = toggleProviderMock;
        }

        public override void SuiteTeardown()
        {
            if (!HasTestFailed())
            {
                Workspace.DeleteWorkspaceAsync(TargetWorkspaceArtifactID).GetAwaiter().GetResult();
            }

            base.SuiteTeardown();
        }

        #region Helper Methods

        protected string CreateDefaultSourceConfig()
        {
            if (_destinationConfig == null)
            {
                _destinationConfig = CreateSerializedSourceConfigWithTargetWorkspace(TargetWorkspaceArtifactID);
            }
            return _destinationConfig;
        }

        protected SourceConfiguration CreateSourceConfigWithTargetWorkspace(int targetWorkspaceId)
        {
            return CreateSourceConfigWithCustomParameters(
                targetWorkspaceId,
                SavedSearchArtifactID,
                SourceWorkspaceArtifactID,
                SourceConfiguration.ExportType.SavedSearch);
        }

        protected string CreateSerializedSourceConfigWithTargetWorkspace(int targetWorkspaceId)
        {
            return CreateSerializedSourceConfigWithCustomParameters(
                targetWorkspaceId,
                SavedSearchArtifactID,
                SourceWorkspaceArtifactID,
                SourceConfiguration.ExportType.SavedSearch);
        }

        protected SourceConfiguration CreateSourceConfigWithCustomParameters(
            int targetWorkspaceId,
            int savedSearchArtifactId,
            int sourceWorkspaceArtifactId,
            SourceConfiguration.ExportType exportType)
        {
            return new SourceConfiguration
            {
                SavedSearchArtifactId = savedSearchArtifactId,
                SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
                TargetWorkspaceArtifactId = targetWorkspaceId,
                TypeOfExport = exportType
            };
        }

        protected string CreateSerializedSourceConfigWithCustomParameters(
            int targetWorkspaceId,
            int savedSearchArtifactId,
            int sourceWorkspaceArtifactId,
            SourceConfiguration.ExportType exportType)
        {
            SourceConfiguration sourceConfiguration = CreateSourceConfigWithCustomParameters(targetWorkspaceId,
                savedSearchArtifactId, sourceWorkspaceArtifactId, exportType);
            return Serializer.Serialize(sourceConfiguration);
        }

        protected string CreateDestinationConfig(ImportOverwriteModeEnum overwriteMode)
        {
            return CreateSerializedDestinationConfigWithTargetWorkspace(overwriteMode, SourceWorkspaceArtifactID);
        }

        protected string CreateSerializedDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum overwriteMode, int targetWorkspaceId)
        {
            ImportSettings destinationConfig = CreateDestinationConfigWithTargetWorkspace(overwriteMode, targetWorkspaceId);
            return Serializer.Serialize(destinationConfig);
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
                FederatedInstanceArtifactId = federatedInstanceArtifactId,
                ExtractedTextFileEncoding = "utf-8"
            };
        }

        protected List<FieldMap> GetDefaultFieldMap(bool withObjectIdentifierBracket = true)
        {
            IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
            IFieldQueryRepository sourceFieldQueryRepository = repositoryFactory.GetFieldQueryRepository(SourceWorkspaceArtifactID);
            IFieldQueryRepository destinationFieldQueryRepository = repositoryFactory.GetFieldQueryRepository(TargetWorkspaceArtifactID);

            ArtifactDTO sourceDto = sourceFieldQueryRepository.RetrieveIdentifierField((int) ArtifactType.Document);
            ArtifactDTO targetDto = destinationFieldQueryRepository.RetrieveIdentifierField((int) ArtifactType.Document);

            string objectIdentifierBracket = withObjectIdentifierBracket
                ? " [Object Identifier]"
                : string.Empty;

            FieldMap[] map =
            {
                new FieldMap
                {
                    SourceField = new FieldEntry
                    {
                        FieldIdentifier = sourceDto.ArtifactId.ToString(),
                        DisplayName = sourceDto.Fields.First(field => field.Name == "Name").Value as string + objectIdentifierBracket,
                        IsIdentifier = true,
                        IsRequired = true
                    },
                    FieldMapType = FieldMapTypeEnum.Identifier,
                    DestinationField = new FieldEntry
                    {
                        FieldIdentifier = targetDto.ArtifactId.ToString(),
                        DisplayName = targetDto.Fields.First(field => field.Name == "Name").Value as string + objectIdentifierBracket,
                        IsIdentifier = true,
                        IsRequired = true
                    },
                }
            };
            return map.ToList();
        }

        private async Task SetupAsync()
        {
            TargetWorkspaceArtifactID = string.IsNullOrEmpty(_targetWorkspaceName)
                ? SourceWorkspaceArtifactID
                : (await Workspace.CreateWorkspaceAsync(_targetWorkspaceName, _targetWorkspaceTemplate).ConfigureAwait(false)).ArtifactID;

            SavedSearchArtifactID = await Task.Run(() => SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactID, "All documents")).ConfigureAwait(false);
            TypeOfExport = (int)SourceConfiguration.ExportType.SavedSearch;
        }

        protected IntegrationPointDto CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum overwriteMode, string name, string overwrite)
        {
            IntegrationPointDto integrationDto = new IntegrationPointDto();
            SetIntegrationPointBaseModelProperties(integrationDto, overwriteMode, name, overwrite);
            return integrationDto;
        }

        protected IntegrationPointProfileDto CreateDefaultIntegrationPointProfileModel(ImportOverwriteModeEnum overwriteMode, string name, string overwrite)
        {
            IntegrationPointProfileDto integrationDto = new IntegrationPointProfileDto();
            SetIntegrationPointBaseModelProperties(integrationDto, overwriteMode, name, overwrite);
            return integrationDto;
        }

        private void SetIntegrationPointBaseModelProperties(IntegrationPointDtoBase dto, ImportOverwriteModeEnum overwriteMode, string name, string overwrite)
        {
            dto.DestinationConfiguration = CreateDestinationConfig(overwriteMode);
            dto.DestinationProvider = RelativityDestinationProviderArtifactId;
            dto.SourceProvider = RelativityProvider.ArtifactId;
            dto.SourceConfiguration = CreateDefaultSourceConfig();
            dto.LogErrors = true;
            dto.EmailNotificationRecipients = "test@relativity.com";
            dto.Name = $"{name}{DateTime.Now:yy-MM-dd HH-mm-ss}";
            dto.SelectedOverwrite = overwrite;
            dto.Scheduler = new Scheduler() { EnableScheduler = false };
            dto.FieldMappings = GetDefaultFieldMap();
            dto.Type =
                Container.Resolve<IIntegrationPointTypeService>()
                    .GetIntegrationPointType(IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid)
                    .ArtifactId;
        }

        protected static int GetRootFolder(ITestHelper helper, int workspaceArtifactId)
        {
            using (var folderManager = helper.CreateProxy<IFolderManager>())
            {
                return folderManager.GetWorkspaceRootAsync(workspaceArtifactId).GetAwaiter().GetResult().ArtifactID;
            }
        }

        #endregion Helper Methods
    }
}
