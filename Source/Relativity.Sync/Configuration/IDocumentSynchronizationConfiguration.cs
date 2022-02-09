namespace Relativity.Sync.Configuration
{
	internal interface IDocumentSynchronizationConfiguration : ISynchronizationConfiguration
	{
		DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; }

		ImportNativeFileCopyMode ImportNativeFileCopyMode { get; }

		string FileSizeColumn { get; set; }

		string NativeFilePathSourceFieldName { get; set; }

		string OiFileTypeColumnName { get; set; }

		string SupportedByViewerColumn { get; set; }

	}
}
