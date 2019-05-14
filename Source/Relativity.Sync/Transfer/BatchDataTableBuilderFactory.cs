namespace Relativity.Sync.Transfer
{
	internal sealed class BatchDataTableBuilderFactory : IBatchDataTableBuilderFactory
	{
		private readonly IFolderPathRetriever _folderPathRetriever;
		private readonly INativeFileRepository _nativeFileRepository;

		public BatchDataTableBuilderFactory(IFolderPathRetriever folderPathRetriever,
			INativeFileRepository nativeFileRepository)
		{
			_folderPathRetriever = folderPathRetriever;
			_nativeFileRepository = nativeFileRepository;
		}

		public IBatchDataTableBuilder Create(SourceDataReaderConfiguration configuration)
		{
			return new BatchDataTableBuilder(configuration, _folderPathRetriever, _nativeFileRepository);
		}
	}
}
