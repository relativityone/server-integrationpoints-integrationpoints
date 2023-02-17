using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using Relativity;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoint.Tests.Core.Templates
{
    public abstract class OtherProvidersTemplate : SourceProviderTemplate
    {
        protected SourceProvider LdapProvider;

        protected OtherProvidersTemplate(
            string workspaceName,
            string workspaceTemplate = WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME)
            : base(workspaceName, workspaceTemplate)
        {
        }

        public override void SuiteSetup()
        {
            base.SuiteSetup();
            LdapProvider = SourceProviders.First(provider => provider.Name == "LDAP");
        }

        #region Helper Methods

        private LDAPSettings CreateDefaultLdapSettings()
        {
            LDAPSettings ldapSettings = new LDAPSettings()
            {
                ConnectionPath = SharedVariables.LdapConnectionPath,
                ConnectionAuthenticationType = AuthenticationTypesEnum.FastBind
            };

            return ldapSettings;
        }

        private string CreateDefaultLdapSourceConfig()
        {
            LDAPSettings defaultLdapSettings = CreateDefaultLdapSettings();

            string serializedLdapSettings = JsonConvert.SerializeObject(defaultLdapSettings);

            return serializedLdapSettings;
        }

        private string CreateDefaultLdapDestinationConfig(int artifactTypeId = (int)ArtifactType.Document, ImportOverwriteModeEnum overwriteMode = ImportOverwriteModeEnum.AppendOnly)
        {
            ImportSettings destinationConfig = new ImportSettings
            {
                ArtifactTypeId = artifactTypeId,
                DestinationProviderType = "74A863B9-00EC-4BB7-9B3E-1E22323010C6",
                CaseArtifactId = WorkspaceArtifactId,
                ImportOverwriteMode = overwriteMode,
                ImportNativeFile = false,
                ExtractedTextFieldContainsFilePath = false,
                EntityManagerFieldContainsLink = true,
                FieldOverlayBehavior = "Use Field Settings"
            };
            return Container.Resolve<ISerializer>().Serialize(destinationConfig);
        }

        private string CreateDefaultLdapFieldMap()
        {
            FieldMap[] map = GetDefaultLdapFieldMap();
            return Container.Resolve<ISerializer>().Serialize(map);
        }

        private FieldMap[] GetDefaultLdapFieldMap()
        {
            IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
            IFieldQueryRepository workspaceFieldQueryRepository = repositoryFactory.GetFieldQueryRepository(WorkspaceArtifactId);

            ArtifactDTO documentDto = workspaceFieldQueryRepository.RetrieveIdentifierField((int)ArtifactType.Document);

            FieldMap[] map =
            {
                new FieldMap
                {
                    SourceField = new FieldEntry
                    {
                        FieldIdentifier = "objectguid",
                        DisplayName = "objectguid",
                        IsIdentifier = false
                    },
                    FieldMapType = FieldMapTypeEnum.Identifier,
                    DestinationField = new FieldEntry
                    {
                        FieldIdentifier = documentDto.ArtifactId.ToString(),
                        DisplayName = documentDto.Fields.First(field => field.Name == "Name").Value as string + " [Object Identifier]",
                        IsIdentifier = true
                    },
                }
            };
            return map;
        }

        #endregion Helper Methods
    }
}
