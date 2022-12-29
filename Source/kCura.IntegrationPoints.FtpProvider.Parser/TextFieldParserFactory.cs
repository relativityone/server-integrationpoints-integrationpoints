using System.IO;
using System.Text;
using kCura.IntegrationPoints.FtpProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.FtpProvider.Parser
{
    public class TextFieldParserFactory : IFieldParserFactory
    {
        public IFieldParser Create(string fileLocation)
        {
            return new TextFieldParserWrapper(fileLocation);
        }

        public IFieldParser Create(Stream stream, Encoding defaultEncoding, bool detectEncoding)
        {
            return new TextFieldParserWrapper(stream, defaultEncoding, detectEncoding);
        }

        public IFieldParser Create(TextReader reader)
        {
            return new TextFieldParserWrapper(reader);
        }
    }
}
