using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.SyncConfiguration
{
	/// <summary>
	/// Provides methods for creating configuration builder for specific Sync flow.
	/// </summary>
	public interface ISyncJobConfigurationBuilder
	{
		/// <summary>
		/// Creates configuration builder for document synchronization flow.
		/// </summary>
		/// <param name="options">Document synchronization options.</param>
		IDocumentSyncConfigurationBuilder ConfigureDocumentSync(DocumentSyncOptions options);

		/// <summary>
		/// Creates configuration builder for image synchronization flow.
		/// </summary>
		/// <param name="options">Image synchronization options.</param>
		IImageSyncConfigurationBuilder ConfigureImageSync(ImageSyncOptions options);
	}
}
