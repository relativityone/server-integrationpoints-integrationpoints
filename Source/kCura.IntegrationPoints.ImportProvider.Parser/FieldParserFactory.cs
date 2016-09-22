using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class FieldParserFactory : IFieldParserFactory
    {
        public IFieldParser GetFieldParser(string options)
        {
            /*
            using (kCura.Relativity.Client.IRSAPIClient client = 
     Relativity.CustomPages.ConnectionHelper.Helper().GetServicesManager().CreateProxy<kCura.Relativity.Client.IRSAPIClient>(Relativity.API.ExecutionIdentity.System))
            {

            }
            */
            return null;
        }
    }
}
