using kCura.IntegrationPoints.Domain.Readers;
using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public interface IImportJobFactory
	{
		IJobImport Create(IExtendedImportAPI importApi, ImportSettings settings, IDataTransferContext context);
	}
}
