using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public interface IImportApiFactory
	{
		IExtendedImportAPI GetImportAPI(ImportSettings settings);
	}
}
