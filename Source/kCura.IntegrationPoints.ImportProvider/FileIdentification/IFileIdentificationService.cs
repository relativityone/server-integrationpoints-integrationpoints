using System.IO;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.ImportProvider.FileIdentification
{
    public interface IFileIdentificationService
    {
        Task<FileProperties> IdentifyFileAsync(Stream stream);
    }
}
