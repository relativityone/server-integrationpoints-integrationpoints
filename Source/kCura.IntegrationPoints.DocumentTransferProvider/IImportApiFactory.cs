using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.DocumentTransferProvider
{
    public interface IImportApiFactory
    {
        IImportAPI Create();
    }
}
