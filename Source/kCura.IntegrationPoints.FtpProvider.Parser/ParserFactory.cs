using System.Collections.Generic;
using System.IO;
using kCura.IntegrationPoints.FtpProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.FtpProvider.Parser
{
    public class ParserFactory : IParserFactory
    {
        private readonly IFieldParserFactory _fieldParserFactory;

        public ParserFactory(IFieldParserFactory fieldParserFactory)
        {
            _fieldParserFactory = fieldParserFactory;
        }

        public IParser GetDelimitedFileParser(Stream stream, ParserOptions parserOptions)
        {
            return new DelimitedFileParser(_fieldParserFactory, stream, parserOptions);
        }

        public IParser GetDelimitedFileParser(TextReader reader, ParserOptions parserOptions, List<string> columnList)
        {
            return new DelimitedFileParser(_fieldParserFactory, reader, parserOptions, columnList);
        }
    }
}