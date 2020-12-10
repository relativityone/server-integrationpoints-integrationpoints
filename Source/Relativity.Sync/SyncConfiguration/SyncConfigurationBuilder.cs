using Relativity.Sync.Storage;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;

namespace Relativity.Sync.SyncConfiguration
{
	/// <inheritdoc />
	public class SyncConfigurationBuilder : ISyncConfigurationBuilder
	{
		private readonly ISyncContext _syncContext;
		private readonly ISyncServiceManager _servicesMgr;
		private readonly ISerializer _serializer;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="syncContext"></param>
		/// <param name="servicesMgr"></param>
		public SyncConfigurationBuilder(ISyncContext syncContext, ISyncServiceManager servicesMgr)
		{
			_syncContext = syncContext;
			_servicesMgr = servicesMgr;

			_serializer = new JSONSerializer();
		}

		/// <inheritdoc />
		public IDocumentSyncConfigurationBuilder ConfigureDocumentSync(DocumentSyncOptions options)
		{
			IFieldsMappingBuilder fieldsMappingBuilder = new FieldsMappingBuilder(
				_syncContext.SourceWorkspaceId, _syncContext.DestinationWorkspaceId, _servicesMgr);

			return new DocumentSyncConfigurationBuilder(_syncContext, _servicesMgr, fieldsMappingBuilder, _serializer, options);
		}

		/// <inheritdoc />
		public IImageSyncConfigurationBuilder ConfigureImageSync(ImageSyncOptions options)
		{
			IFieldsMappingBuilder fieldsMappingBuilder = new FieldsMappingBuilder(
				_syncContext.SourceWorkspaceId, _syncContext.DestinationWorkspaceId, _servicesMgr);

			return new ImageSyncConfigurationBuilder(_syncContext, _servicesMgr, fieldsMappingBuilder, _serializer, options);
		}
	}
}
