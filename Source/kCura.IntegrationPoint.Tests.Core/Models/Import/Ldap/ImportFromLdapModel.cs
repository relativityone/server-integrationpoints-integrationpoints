using kCura.IntegrationPoint.Tests.Core.Models.Shared;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import.Ldap
{
    public class ImportFromLdapModel
    {
        public IntegrationPointGeneralModel General { get; set; }

        public ImportFromLdapSourceConnectionModel Source { get; set; }

        public ImportSettingsModel SharedImportSettings { get; set; }

        public ImportEntitySettingsModel ImportEntitySettingsModel { get; set; }

        public ImportFromLdapModel(string name, string transferredObject)
        {
            General = new IntegrationPointGeneralModel(name)
            {
                Type = IntegrationPointType.Import,
                SourceProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_SOURCE_PROVIDER_LDAP,
                TransferredObject = transferredObject
            };
            Source = new ImportFromLdapSourceConnectionModel();
            SharedImportSettings = new ImportSettingsModel();
            ImportEntitySettingsModel = new ImportEntitySettingsModel();
        }
    }
}
