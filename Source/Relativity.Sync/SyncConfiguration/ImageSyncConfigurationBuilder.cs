using System.Threading.Tasks;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.SyncConfiguration.FieldsMapping;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;

namespace Relativity.Sync.SyncConfiguration
{
    internal class ImageSyncConfigurationBuilder : SyncConfigurationRootBuilderBase, IImageSyncConfigurationBuilder
    {
        private readonly IFieldsMappingBuilder _fieldsMappingBuilder;

        internal ImageSyncConfigurationBuilder(
                ISyncContext syncContext,
                ISourceServiceFactoryForAdmin serviceFactoryForAdmin,
                IFieldsMappingBuilder fieldsMappingBuilder,
                ISerializer serializer,
                ImageSyncOptions options,
                RdoOptions rdoOptions,
                IRdoManager rdoManager)
            : base(syncContext, serviceFactoryForAdmin, rdoOptions, rdoManager, serializer)
        {
            _fieldsMappingBuilder = fieldsMappingBuilder;

            SyncConfiguration.ImageImport = true;
            SyncConfiguration.RdoArtifactTypeId = (int)ArtifactType.Document;
            SyncConfiguration.DataSourceType = options.DataSourceType;
            SyncConfiguration.DataSourceArtifactId = options.DataSourceId;
            SyncConfiguration.DataDestinationType = options.DestinationLocationType;
            SyncConfiguration.DataDestinationArtifactId = options.DestinationLocationId;
            SyncConfiguration.ImageFileCopyMode = options.CopyImagesMode;
            SyncConfiguration.IncludeOriginalImages = true;

            SyncConfiguration.EnableTagging = options.EnableTagging;
        }

        public new IImageSyncConfigurationBuilder CorrelationId(string correlationId)
        {
            base.CorrelationId(correlationId);

            return this;
        }

        public new IImageSyncConfigurationBuilder OverwriteMode(OverwriteOptions options)
        {
            base.OverwriteMode(options);

            return this;
        }

        public new IImageSyncConfigurationBuilder EmailNotifications(EmailNotificationsOptions options)
        {
            base.EmailNotifications(options);

            return this;
        }

        public new IImageSyncConfigurationBuilder CreateSavedSearch(CreateSavedSearchOptions options)
        {
            base.CreateSavedSearch(options);

            return this;
        }

        public new IImageSyncConfigurationBuilder IsRetry(RetryOptions options)
        {
            base.IsRetry(options);

            return this;
        }

        public IImageSyncConfigurationBuilder ProductionImagePrecedence(ProductionImagePrecedenceOptions options)
        {
            SyncConfiguration.ProductionImagePrecedence = Serializer.Serialize(options.ProductionImagePrecedenceIds);
            SyncConfiguration.IncludeOriginalImages = options.IncludeOriginalImagesIfNotFoundInProductions;

            return this;
        }

        public new IImageSyncConfigurationBuilder DisableItemLevelErrorLogging()
        {
            base.DisableItemLevelErrorLogging();
            return this;
        }

        protected override Task ValidateAsync()
        {
            SetFieldsMapping();

            return Task.CompletedTask;
        }

        private void SetFieldsMapping()
        {
            var fieldsMapping = _fieldsMappingBuilder.WithIdentifier().FieldsMapping;

            SyncConfiguration.FieldsMapping = Serializer.Serialize(fieldsMapping);
        }
    }
}
