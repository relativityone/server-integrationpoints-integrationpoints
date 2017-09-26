using kCura.IntegrationPoints.Domain.Readers;
using kCura.Relativity.ImportAPI;
using Relativity.API;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public interface IImportJobFactory
	{
		IJobImport Create(IExtendedImportAPI importApi, ImportSettings settings, IDataTransferContext context, IHelper helper);
	}
}
