using kCura.Relativity.ImportAPI;
using System.Data;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public interface IImportJobFactory
	{
		IJobImport Create(IExtendedImportAPI importApi, ImportSettings settings, IDataReader sourceData);
	}
}
