namespace Relativity.Sync.Configuration
{
    internal interface IImageSynchronizationConfiguration : ISynchronizationConfiguration
    {
        string ImageFilePathSourceFieldName { get; set; }

        string IdentifierColumn { get; set; }

        ImportImageFileCopyMode ImportImageFileCopyMode { get; }
    }
}
