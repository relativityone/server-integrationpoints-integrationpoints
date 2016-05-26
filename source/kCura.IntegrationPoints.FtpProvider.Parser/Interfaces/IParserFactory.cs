using System;
using System.IO;

namespace kCura.IntegrationPoints.FtpProvider.Parser.Interfaces
{
    public interface IParserFactory
    {
        IParser GetDelimitedFileParser(Stream stream, String fieldDelimiter);
    }
}
