using System.Collections.Generic;
using System.Data;
using System.IO;
using kCura.IntegrationPoints.FtpProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.FtpProvider.Parser
{
    public class DataReaderFactory : IDataReaderFactory
    {
        public IDataReader GetFileDataReader(string filePath)
        {
            return new FileDataReader(filePath);
        }

        public TextReader GetEnumerableReader(IEnumerable<string> lines)
        {
            return new EnumerableReader(lines);
        }
    }
}
