using Relativity.Sync.SyncConfiguration.FieldsMapping;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;

namespace Relativity.Sync.SyncConfiguration
{
    internal class SyncJobConfigurationBuilder : ISyncJobConfigurationBuilder
    {
        private readonly ISyncContext _syncContext;
        private readonly ISyncServiceManager _servicesMgr;
        private readonly RdoOptions _rdoOptions;
        private readonly ISerializer _serializer;

        public SyncJobConfigurationBuilder(ISyncContext syncContext, ISyncServiceManager servicesMgr, RdoOptions rdoOptions, ISerializer serializer)
        {
            _syncContext = syncContext;
            _servicesMgr = servicesMgr;
            _rdoOptions = rdoOptions;
            _serializer = serializer;
        }
        
        public IDocumentSyncConfigurationBuilder ConfigureDocumentSync(DocumentSyncOptions options)
        {
            IFieldsMappingBuilder fieldsMappingBuilder = new FieldsMappingBuilder(
                _syncContext.SourceWorkspaceId, _syncContext.DestinationWorkspaceId, _servicesMgr);

            return new DocumentSyncConfigurationBuilder(_syncContext, _servicesMgr, fieldsMappingBuilder, _serializer,
                options, _rdoOptions);
        }

        public IImageSyncConfigurationBuilder ConfigureImageSync(ImageSyncOptions options)
        {
            IFieldsMappingBuilder fieldsMappingBuilder = new FieldsMappingBuilder(
                _syncContext.SourceWorkspaceId, _syncContext.DestinationWorkspaceId, _servicesMgr);

            return new ImageSyncConfigurationBuilder(_syncContext, _servicesMgr, fieldsMappingBuilder, _serializer,
                options, _rdoOptions);
        }
    }
}