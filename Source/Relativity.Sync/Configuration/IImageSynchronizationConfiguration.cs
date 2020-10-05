namespace Relativity.Sync.Configuration
{
	interface IImageSynchronizationConfiguration : ISynchronizationConfiguration
	{
		string ImageFilePathSourceFieldName { get; set; }
		
		bool IncludeOriginalImages { get; }

		ImportImageFileCopyMode ImportImageFileCopyMode { get; }

		int[] ProductionImagePrecedence { get; }
	}
}
