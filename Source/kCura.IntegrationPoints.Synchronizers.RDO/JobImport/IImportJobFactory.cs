using kCura.IntegrationPoints.Domain.Readers;
using kCura.Relativity.ImportAPI;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
    public interface IImportJobFactory
    {
        IJobImport Create(IImportAPI importApi, ImportSettings settings, IDataTransferContext context, IHelper helper);
    }
}
