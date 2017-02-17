using kCura.Relativity.ImportAPI;
using System.Data;
using kCura.IntegrationPoints.Domain.Readers;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public interface IImportJobFactory
	{
		IJobImport Create(IExtendedImportAPI importApi, ImportSettings settings, IDataTransferContext context);
	}
}
