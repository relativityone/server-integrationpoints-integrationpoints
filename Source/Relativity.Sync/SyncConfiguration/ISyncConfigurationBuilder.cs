using System;
using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.SyncConfiguration
{
	public interface ISyncConfigurationBuilder
	{
		IDocumentSyncConfigurationBuilder ConfigureDocumentSync(DocumentSyncOptions options);

		IImageSyncConfigurationBuilder ConfigureImageSync(ImageSyncOptions options);
	}
}
