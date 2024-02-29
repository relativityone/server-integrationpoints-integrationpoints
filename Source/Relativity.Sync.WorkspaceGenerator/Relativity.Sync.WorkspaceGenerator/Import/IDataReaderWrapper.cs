using System.Data;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
    public interface IDataReaderWrapper : IDataReader
    {
        DataTable ReadToSimpleDataTable();
    }
}