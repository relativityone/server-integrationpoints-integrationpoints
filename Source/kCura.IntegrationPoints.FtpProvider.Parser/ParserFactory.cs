using System.Collections.Generic;
using System.IO;
using kCura.IntegrationPoints.FtpProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.FtpProvider.Parser
{
    public class ParserFactory : IParserFactory
    {
        public IParser GetDelimitedFileParser(Stream stream, ParserOptions parserOptions)
        {
            return new DelimitedFileParser(stream, parserOptions);
        }

        public IParser GetDelimitedFileParser(TextReader reader, ParserOptions parserOptions, List<string> columnList)
        {
            return new DelimitedFileParser(reader, parserOptions, columnList);
        }
    }
}