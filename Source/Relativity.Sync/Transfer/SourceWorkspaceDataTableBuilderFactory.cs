namespace Relativity.Sync.Transfer
{
	internal sealed class SourceWorkspaceDataTableBuilderFactory : ISourceWorkspaceDataTableBuilderFactory
	{
		private readonly IFolderPathRetriever _folderPathRetriever;
		private readonly INativeFileRepository _nativeFileRepository;

		public SourceWorkspaceDataTableBuilderFactory(IFolderPathRetriever folderPathRetriever,
			INativeFileRepository nativeFileRepository)
		{
			_folderPathRetriever = folderPathRetriever;
			_nativeFileRepository = nativeFileRepository;
		}

		public ISourceWorkspaceDataTableBuilder Create(SourceDataReaderConfiguration configuration)
		{
			return new SourceWorkspaceDataTableBuilder(configuration, _folderPathRetriever, _nativeFileRepository);
		}
	}
}
