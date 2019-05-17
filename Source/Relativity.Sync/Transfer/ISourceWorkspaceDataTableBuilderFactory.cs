namespace Relativity.Sync.Transfer
{
	internal interface ISourceWorkspaceDataTableBuilderFactory
	{
		ISourceWorkspaceDataTableBuilder Create(SourceDataReaderConfiguration configuration);
	}
}
