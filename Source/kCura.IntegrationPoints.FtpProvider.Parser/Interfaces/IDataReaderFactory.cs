using System.Collections.Generic;
using System.Data;
using System.IO;

namespace kCura.IntegrationPoints.FtpProvider.Parser.Interfaces
{
    public interface IDataReaderFactory
    {
        IDataReader GetFileDataReader(string filePath);
        TextReader GetEnumerableReader(IEnumerable<string> lines);
    }
}
