using System.Data;

namespace Relativity.Sync.Transfer
{
    internal interface IBatchDataReader : IDataReader
    {
        bool CanCancel { get; }
    }
}