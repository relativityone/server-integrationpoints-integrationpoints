using System.Data;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
    public interface IDataReaderFactory
    {
        IDataReader GetDataReader(string options);
    }
}
