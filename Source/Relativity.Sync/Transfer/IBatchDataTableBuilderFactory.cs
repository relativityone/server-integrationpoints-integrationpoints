namespace Relativity.Sync.Transfer
{
	internal interface IBatchDataTableBuilderFactory
	{
		IBatchDataTableBuilder Create(SourceDataReaderConfiguration configuration);
	}
}
