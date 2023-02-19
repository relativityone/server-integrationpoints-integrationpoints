using System.IO;
using System.Text;

namespace kCura.IntegrationPoints.FtpProvider.Parser.Interfaces
{
    public interface IFieldParserFactory
    {
        IFieldParser Create(string fileLocation);

        IFieldParser Create(Stream stream, Encoding defaultEncoding, bool detectEncoding);

        IFieldParser Create(TextReader reader);
    }
}
