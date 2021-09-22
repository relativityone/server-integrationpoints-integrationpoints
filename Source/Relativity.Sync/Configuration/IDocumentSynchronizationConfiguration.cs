﻿namespace Relativity.Sync.Configuration
{
	internal interface IDocumentSynchronizationConfiguration : ISynchronizationConfiguration
	{
		char MultiValueDelimiter { get; }

		char NestedValueDelimiter { get; }

		DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; }

		ImportNativeFileCopyMode ImportNativeFileCopyMode { get; }

		string FileSizeColumn { get; set; }

		string NativeFilePathSourceFieldName { get; set; }

		string OiFileTypeColumnName { get; set; }

		string SupportedByViewerColumn { get; set; }

	}
}
