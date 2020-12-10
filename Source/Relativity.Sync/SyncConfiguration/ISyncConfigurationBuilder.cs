using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.SyncConfiguration
{
	/// <summary>
	/// 
	/// </summary>
	public interface ISyncConfigurationBuilder
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		IDocumentSyncConfigurationBuilder ConfigureDocumentSync(DocumentSyncOptions options);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		IImageSyncConfigurationBuilder ConfigureImageSync(ImageSyncOptions options);
	}
}
