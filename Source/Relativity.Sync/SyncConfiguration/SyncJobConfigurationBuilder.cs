using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.SyncConfiguration.FieldsMapping;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;

namespace Relativity.Sync.SyncConfiguration
{
    internal class SyncJobConfigurationBuilder : ISyncJobConfigurationBuilder
    {
        private readonly ISyncContext _syncContext;
        private readonly ISourceServiceFactoryForAdmin _servicesMgrForAdmin;
        private readonly ISourceServiceFactoryForUser _servicesMgrForUser;
        private readonly RdoOptions _rdoOptions;
        private readonly ISerializer _serializer;

        internal SyncJobConfigurationBuilder(ISyncContext syncContext, ISourceServiceFactoryForAdmin servicesMgrForAdmin, ISourceServiceFactoryForUser servicesMgrForUser, RdoOptions rdoOptions, ISerializer serializer)
        {
            _syncContext = syncContext;
            _servicesMgrForAdmin = servicesMgrForAdmin;
            _servicesMgrForUser = servicesMgrForUser;
            _rdoOptions = rdoOptions;
            _serializer = serializer;
        }

        public IDocumentSyncConfigurationBuilder ConfigureDocumentSync(DocumentSyncOptions options)
        {
            IFieldsMappingBuilder fieldsMappingBuilder = new FieldsMappingBuilder(
                _syncContext.SourceWorkspaceId, _syncContext.DestinationWorkspaceId, (int)ArtifactType.Document, (int)ArtifactType.Document, _servicesMgrForAdmin);

            return new DocumentSyncConfigurationBuilder(_syncContext, _servicesMgrForAdmin, fieldsMappingBuilder, _serializer,
                options, _rdoOptions, new RdoManager(new EmptyLogger(), _servicesMgrForAdmin, new RdoGuidProvider()));
        }

        public IImageSyncConfigurationBuilder ConfigureImageSync(ImageSyncOptions options)
        {
            IFieldsMappingBuilder fieldsMappingBuilder = new FieldsMappingBuilder(
                _syncContext.SourceWorkspaceId, _syncContext.DestinationWorkspaceId, (int)ArtifactType.Document, (int)ArtifactType.Document, _servicesMgrForAdmin);

            return new ImageSyncConfigurationBuilder(_syncContext, _servicesMgrForAdmin, fieldsMappingBuilder, _serializer,
                options, _rdoOptions, new RdoManager(new EmptyLogger(), _servicesMgrForAdmin, new RdoGuidProvider()));
        }

        public INonDocumentSyncConfigurationBuilder ConfigureNonDocumentSync(NonDocumentSyncOptions options)
        {
            IFieldsMappingBuilder fieldsMappingBuilder = new FieldsMappingBuilder(
                _syncContext.SourceWorkspaceId, _syncContext.DestinationWorkspaceId, options.RdoArtifactTypeId, options.DestinationRdoArtifactTypeId, _servicesMgrForAdmin);
            
            return new NonDocumentSyncConfigurationBuilder(_syncContext, _servicesMgrForUser,
                fieldsMappingBuilder, _serializer, options, _rdoOptions,
                new RdoManager(new EmptyLogger(), _servicesMgrForAdmin, new RdoGuidProvider()));
        }
    }
}