using Relativity.API;
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
        private readonly IServicesMgr _servicesMgr;
        private readonly RdoOptions _rdoOptions;
        private readonly ISerializer _serializer;
        private readonly IAPILog _logger;

        internal SyncJobConfigurationBuilder(ISyncContext syncContext, IServicesMgr servicesMgr, RdoOptions rdoOptions, ISerializer serializer, IAPILog logger)
        {
            _syncContext = syncContext;
            _servicesMgr = servicesMgr;
            _rdoOptions = rdoOptions;
            _serializer = serializer;
            _logger = logger;
        }

        public IDocumentSyncConfigurationBuilder ConfigureDocumentSync(DocumentSyncOptions options)
        {
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin = CreateServicesManagerForAdmin();
            IFieldsMappingBuilder fieldsMappingBuilder = new FieldsMappingBuilder(
                _syncContext.SourceWorkspaceId, _syncContext.DestinationWorkspaceId, (int)ArtifactType.Document, (int)ArtifactType.Document, serviceFactoryForAdmin);

            return new DocumentSyncConfigurationBuilder(_syncContext, serviceFactoryForAdmin, fieldsMappingBuilder, _serializer,
                options, _rdoOptions, new RdoManager(new EmptyLogger(), serviceFactoryForAdmin, new RdoGuidProvider()));
        }

        public IImageSyncConfigurationBuilder ConfigureImageSync(ImageSyncOptions options)
        {
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin = CreateServicesManagerForAdmin();
            IFieldsMappingBuilder fieldsMappingBuilder = new FieldsMappingBuilder(
                _syncContext.SourceWorkspaceId, _syncContext.DestinationWorkspaceId, (int)ArtifactType.Document, (int)ArtifactType.Document, serviceFactoryForAdmin);

            return new ImageSyncConfigurationBuilder(_syncContext, serviceFactoryForAdmin, fieldsMappingBuilder, _serializer,
                options, _rdoOptions, new RdoManager(new EmptyLogger(), serviceFactoryForAdmin, new RdoGuidProvider()));
        }

        public INonDocumentSyncConfigurationBuilder ConfigureNonDocumentSync(NonDocumentSyncOptions options)
        {
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin = CreateServicesManagerForAdmin();
            IFieldsMappingBuilder fieldsMappingBuilder = new FieldsMappingBuilder(
                _syncContext.SourceWorkspaceId, _syncContext.DestinationWorkspaceId, options.RdoArtifactTypeId, options.DestinationRdoArtifactTypeId, serviceFactoryForAdmin);

            return new NonDocumentSyncConfigurationBuilder(_syncContext, serviceFactoryForAdmin,
                fieldsMappingBuilder, _serializer, options, _rdoOptions,
                new RdoManager(new EmptyLogger(), serviceFactoryForAdmin, new RdoGuidProvider()));
        }

        private ISourceServiceFactoryForAdmin CreateServicesManagerForAdmin()
        {
            ServiceFactoryForAdminFactory serviceFactoryForAdminFactory = new ServiceFactoryForAdminFactory(_servicesMgr, _logger);
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin = serviceFactoryForAdminFactory.Create();

            return serviceFactoryForAdmin;
        }
    }
}
