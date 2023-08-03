using kCura.Relativity.ImportAPI;
using Relativity.IntegrationPoints.FieldsMapping.ImportApi;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public interface IImportApiFactory
    {
        IImportAPI GetImportAPI();

        IImportApiFacade GetImportApiFacade();
    }
}
