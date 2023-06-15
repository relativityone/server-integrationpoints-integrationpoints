using System.Collections.Concurrent;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification
{
    public interface IFileIdentificationService
    {
        Task IdentifyFilesAsync(ImportProviderSettings settings, BlockingCollection<string> files);
    }
}
