using System;
using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.FtpProvider.Parser.Interfaces
{
    public interface IParser : IDisposable
    {
        IEnumerable<String> ParseColumns();
        IDataReader ParseData();
    }
}
