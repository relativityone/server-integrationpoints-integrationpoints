using System.Data;

namespace Relativity.Sync.Transfer
{
	internal interface ISourceWorkspaceDataReader : IDataReader
	{
		IItemStatusMonitor ItemStatusMonitor { get; }
	}
}