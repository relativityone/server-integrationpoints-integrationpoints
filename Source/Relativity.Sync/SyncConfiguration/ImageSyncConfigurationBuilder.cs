using Relativity.Sync.Storage;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;

namespace Relativity.Sync.SyncConfiguration
{
	public class ImageSyncConfigurationBuilder : SyncConfigurationRootBuilderBase, IImageSyncConfigurationBuilder
	{
		public ImageSyncConfigurationBuilder(ISyncContext syncContext, ISyncServiceManager servicesMgr,
				IFieldsMappingBuilder fieldsMappingBuilder, ISerializer serializer, ImageSyncOptions options) 
			: base(syncContext, servicesMgr, serializer)
		{
			SyncConfiguration.ImageImport = true;
			SyncConfiguration.RdoArtifactTypeId = (int)ArtifactType.Document;
			SyncConfiguration.DataSourceType = options.DataSourceType.ToString();
			SyncConfiguration.DataSourceArtifactId = options.DataSourceId;
			SyncConfiguration.DataDestinationType = options.DestinationLocationType.ToString();
			SyncConfiguration.DataDestinationArtifactId = options.DestinationLocationId;
			SyncConfiguration.ImageFileCopyMode = options.CopyImagesMode.GetDescription();
			SyncConfiguration.IncludeOriginalImages = true;

			var fieldsMapping = fieldsMappingBuilder.WithIdentifier().FieldsMapping;
			SyncConfiguration.FieldsMapping = Serializer.Serialize(fieldsMapping);
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
	}
}
