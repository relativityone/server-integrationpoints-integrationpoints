using Relativity.Sync.Storage;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;

namespace Relativity.Sync.SyncConfiguration
{
	public class SyncConfigurationBuilder : ISyncConfigurationBuilder
	{
		private readonly ISyncContext _syncContext;
		private readonly ISyncServiceManager _servicesMgr;
		private readonly ISerializer _serializer;

		public SyncConfigurationBuilder(ISyncContext syncContext, ISyncServiceManager servicesMgr)
		{
			_syncContext = syncContext;
			_servicesMgr = servicesMgr;

			_serializer = new JSONSerializer();
		}

		public IDocumentSyncConfigurationBuilder ConfigureDocumentSync(DocumentSyncOptions options)
		{
			IFieldsMappingBuilder fieldsMappingBuilder = new FieldsMappingBuilder(
				_syncContext.SourceWorkspaceId, _syncContext.DestinationWorkspaceId, _servicesMgr);

			return new DocumentSyncConfigurationBuilder(_syncContext, _servicesMgr, fieldsMappingBuilder, _serializer, options);
		}

		public IImageSyncConfigurationBuilder ConfigureImageSync(ImageSyncOptions options)
		{
			IFieldsMappingBuilder fieldsMappingBuilder = new FieldsMappingBuilder(
				_syncContext.SourceWorkspaceId, _syncContext.DestinationWorkspaceId, _servicesMgr);

			return new ImageSyncConfigurationBuilder(_syncContext, _servicesMgr, fieldsMappingBuilder, _serializer, options);
		}
	}
}
