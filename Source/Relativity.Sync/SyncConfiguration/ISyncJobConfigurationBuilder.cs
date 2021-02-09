using Relativity.Sync.SyncConfiguration.Options;
#pragma warning disable 1591

namespace Relativity.Sync.SyncConfiguration
{
	public interface ISyncJobConfigurationBuilder
	{
		IDocumentSyncConfigurationBuilder ConfigureDocumentSync(DocumentSyncOptions options);

		IImageSyncConfigurationBuilder ConfigureImageSync(ImageSyncOptions options);
	}
}
