using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	internal interface ISourceWorkspaceDataReaderFactory
	{
		ISourceWorkspaceDataReader CreateSourceWorkspaceDataReader(IBatch batch);
	}
}