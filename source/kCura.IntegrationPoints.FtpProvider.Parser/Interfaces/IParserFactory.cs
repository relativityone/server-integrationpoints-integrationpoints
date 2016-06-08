using System.Collections.Generic;
using System.IO;

namespace kCura.IntegrationPoints.FtpProvider.Parser.Interfaces
{
    public interface IParserFactory
    {
        IParser GetDelimitedFileParser(Stream stream, ParserOptions parserOptions);
        IParser GetDelimitedFileParser(TextReader reader, ParserOptions parserOptions, List<string> columnList);
    }
}
