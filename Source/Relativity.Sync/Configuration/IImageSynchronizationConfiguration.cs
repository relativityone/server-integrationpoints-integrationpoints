namespace Relativity.Sync.Configuration
{
	interface IImageSynchronizationConfiguration : ISynchronizationConfiguration
	{
		string ImageFilePathSourceFieldName { get; set; }

		string IdentifierColumn { get; set; }

		ImportImageFileCopyMode ImportImageFileCopyMode { get; }
	}
}
