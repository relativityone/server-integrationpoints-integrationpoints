using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.DocumentTransferProvider
{
	public interface IExtendedImportApiFactory
	{
		IExtendedImportAPI Create();
	}
}