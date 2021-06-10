using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public interface IImportApiFactory
	{
		IImportAPI GetImportAPI(ImportSettings settings);
		IImportApiFacade GetImportApiFacade(ImportSettings settings);
	}
}
