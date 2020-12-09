using Relativity.Sync.Storage;
using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.SyncConfiguration
{
	public class ImageSyncConfigurationBuilder : SyncConfigurationRootBuilderBase, IImageSyncConfigurationBuilder
	{
		public ImageSyncConfigurationBuilder(ISyncContext syncContext, ISyncServiceManager servicesMgr,
				IFieldsMappingBuilder fieldsMappingBuilder, ImageSyncOptions options) 
			: base(syncContext, servicesMgr)
		{
			SyncConfiguration.ImageImport = true;
			SyncConfiguration.RdoArtifactTypeId = (int)ArtifactType.Document;
			SyncConfiguration.DataSourceType = options.DataSourceType.ToString();
			SyncConfiguration.DataSourceArtifactId = options.DataSourceId;
			SyncConfiguration.ImageFileCopyMode = options.CopyImagesMode.GetDescription();
			SyncConfiguration.DataDestinationType = options.DestinationLocationType.ToString();
			SyncConfiguration.DataDestinationArtifactId = options.DestinationLocationId;

			var fieldsMapping = fieldsMappingBuilder.WithIdentifier().FieldsMapping;
			SyncConfiguration.FieldsMapping = Serializer.Serialize(fieldsMapping);
		}

		public IImageSyncConfigurationBuilder OverwriteMode(OverwriteOptions options)
		{
			base.OverwriteMode(options);

			return this;
		}

		public IImageSyncConfigurationBuilder EmailNotifications(EmailNotificationsOptions options)
		{
			base.EmailNotifications(options);

			return this;
		}

		public IImageSyncConfigurationBuilder CreateSavedSearch(CreateSavedSearchOptions options)
		{
			base.CreateSavedSearch(options);

			return this;
		}

		public IImageSyncConfigurationBuilder IsRetry(RetryOptions options)
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
	}
}
