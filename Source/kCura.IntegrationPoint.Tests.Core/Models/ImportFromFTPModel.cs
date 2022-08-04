namespace kCura.IntegrationPoint.Tests.Core.Models
{
    public class ImportFromFTPModel : IntegrationPointGeneralModel
    {
        public ImportFromFTPModel(string name) : base(name)
        {
            SourceProvider = INTEGRATION_POINT_SOURCE_PROVIDER_FTP;
        }
    }
}
