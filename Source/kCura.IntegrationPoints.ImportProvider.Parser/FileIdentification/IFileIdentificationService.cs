using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification
{
    public interface IFileIdentificationService
    {
        Task IdentifyFilesAsync(BlockingCollection<ImportFileInfo> files);
    }
}
