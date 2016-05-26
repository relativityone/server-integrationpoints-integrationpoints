using System;
using System.IO;
using kCura.IntegrationPoints.FtpProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.FtpProvider.Parser
{
    public class ParserFactory : IParserFactory
    {
        public IParser GetDelimitedFileParser(Stream stream, String fieldDelimiter)
        {
            return new DelimitedFileParser(stream, fieldDelimiter);
        }
    }
}