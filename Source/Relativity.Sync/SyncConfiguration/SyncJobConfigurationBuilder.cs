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
        private readonly ISyncServiceManager _servicesMgr;
        private readonly RdoOptions _rdoOptions;
        private readonly ISerializer _serializer;
        private readonly ISyncLog _logger;

        internal SyncJobConfigurationBuilder(ISyncContext syncContext, ISyncServiceManager servicesMgr, RdoOptions rdoOptions, ISerializer serializer, ISyncLog logger)
        {
            _syncContext = syncContext;
            _servicesMgr = servicesMgr;
            _rdoOptions = rdoOptions;
            _serializer = serializer;
            _logger = logger;
        }

        public IDocumentSyncConfigurationBuilder ConfigureDocumentSync(DocumentSyncOptions options)
        {
            ISourceServiceFactoryForAdmin servicesMgrForAdmin = CreateServicesManagerForAdmin();
            IFieldsMappingBuilder fieldsMappingBuilder = new FieldsMappingBuilder(
                _syncContext.SourceWorkspaceId, _syncContext.DestinationWorkspaceId, (int)ArtifactType.Document, (int)ArtifactType.Document, servicesMgrForAdmin);

            return new DocumentSyncConfigurationBuilder(_syncContext, servicesMgrForAdmin, fieldsMappingBuilder, _serializer,
                options, _rdoOptions, new RdoManager(new EmptyLogger(), servicesMgrForAdmin, new RdoGuidProvider()));
        }

        public IImageSyncConfigurationBuilder ConfigureImageSync(ImageSyncOptions options)
        {
            ISourceServiceFactoryForAdmin servicesMgrForAdmin = CreateServicesManagerForAdmin();
            IFieldsMappingBuilder fieldsMappingBuilder = new FieldsMappingBuilder(
                _syncContext.SourceWorkspaceId, _syncContext.DestinationWorkspaceId, (int)ArtifactType.Document, (int)ArtifactType.Document, servicesMgrForAdmin);

            return new ImageSyncConfigurationBuilder(_syncContext, servicesMgrForAdmin, fieldsMappingBuilder, _serializer,
                options, _rdoOptions, new RdoManager(new EmptyLogger(), servicesMgrForAdmin, new RdoGuidProvider()));
        }

        public INonDocumentSyncConfigurationBuilder ConfigureNonDocumentSync(NonDocumentSyncOptions options)
        {
            ISourceServiceFactoryForAdmin servicesMgrForAdmin = CreateServicesManagerForAdmin();
            IFieldsMappingBuilder fieldsMappingBuilder = new FieldsMappingBuilder(
                _syncContext.SourceWorkspaceId, _syncContext.DestinationWorkspaceId, options.RdoArtifactTypeId, options.DestinationRdoArtifactTypeId, servicesMgrForAdmin);

            return new NonDocumentSyncConfigurationBuilder(_syncContext, servicesMgrForAdmin,
                fieldsMappingBuilder, _serializer, options, _rdoOptions,
                new RdoManager(new EmptyLogger(), servicesMgrForAdmin, new RdoGuidProvider()));
        }

        private ISourceServiceFactoryForAdmin CreateServicesManagerForAdmin()
        {
            ServiceFactoryForAdminFactory servicesManagerForAdminFactory = new ServiceFactoryForAdminFactory(_servicesMgr, _logger);
            ISourceServiceFactoryForAdmin servicesMgrForAdmin = servicesManagerForAdminFactory.Create();

            return servicesMgrForAdmin;
        }
    }
}