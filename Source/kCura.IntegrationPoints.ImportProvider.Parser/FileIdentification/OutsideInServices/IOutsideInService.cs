using System.IO;
using OutsideIn;

namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification.OutsideInServices
{
    public interface IOutsideInService
    {
        FileFormat IdentifyFile(Stream stream, Exporter exporter);
    }
}
