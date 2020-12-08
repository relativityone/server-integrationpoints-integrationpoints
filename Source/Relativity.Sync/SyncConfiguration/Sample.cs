using Relativity.Sync.Configuration;
using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.SyncConfiguration
{
	public class Sample
	{
		//Add Contructor to options

		//public ISyncServiceManager ServicesMgr;

		//public Sample()
		//{
		//	ISyncContext context = new SyncContext(1, 2, TODO);

		//	ISyncConfigurationBuilder builder = new SyncConfigurationBuilder(context, ServicesMgr);

		//	builder.ConfigureDocumentSync(new DocumentSyncOptions(1, 1))
		//		.DestinationFolderStructure(DestinationFolderStructureOptions.None())
		//		.OverwriteMode(OverwriteOptions.AppendOverlay(FieldOverlayBehavior.MergeValues))
		//		.Build();
		//}


		//public Sample()
		//{
		//	ISyncConfigurationBuilder builder = new SyncConfigurationBuilder(executingUserId: 1, destinationWorkspace);

		//	builder.ConfigureDocumentSync(
		//			new DocumentSyncOptions
		//			{
		//				SavedSearchId = 100,
		//				DestinationFolderId = 100,
		//				CopyNativesMode = ImportNativeFileCopyMode.DoNotImportNativeFiles,
		//				FieldsMapping = null
		//			})
		//		.DestinationFolderStructure(
		//			new DestinationFolderStructureOptions
		//			{
		//				DestinationFolderStructure = DestinationFolderStructureBehavior.None,
		//				FolderPathSourceFieldId = 0,
		//				MoveExistingDocuments = false
		//			})
		//		.CreateSavedSearch()
		//		.EmailNotifications(new EmailNotificationsOptions())
		//		.ExecutingInfo(new ExecutingInfoOptions
		//		{
		//			TriggeredBy = null
		//		})
		//		.IsRetry()
		//		.OverwriteMode(new OverwriteOptions
		//		{
		//			OverwriteMode = ImportOverwriteMode.AppendOnly,
		//			FieldsOverlayBehavior = FieldOverlayBehavior.UseFieldSettings
		//		})
		//		.Build();


		//	int documentConfig = builder.ConfigureDocumentSync(
		//			new DocumentSyncOptions
		//			{
		//				SavedSearchId = 100,
		//				CopyNativesMode = ImportNativeFileCopyMode.DoNotImportNativeFiles,
		//				DestinationFolderId = 10,
		//				FieldsMapping = new List<FieldMap>()
		//			})
		//		.CreateSavedSearch()
		//		.DestinationFolderStructure(
		//			new DestinationFolderStructureOptions
		//			{
		//				DestinationFolderStructure = DestinationFolderStructureBehavior.ReadFromField,
		//				FolderPathSourceFieldId = 0,
		//				MoveExistingDocuments = false
		//			})
		//		.OverwriteMode(
		//			new OverwriteOptions
		//			{
		//				OverwriteMode = ImportOverwriteMode.AppendOverlay,
		//				FieldsOverlayBehavior = FieldOverlayBehavior.UseFieldSettings
		//			})
		//		.ExecutingInfo(
		//			new ExecutingInfoOptions
		//			{
		//				TriggeredBy = null
		//			})
		//		.EmailNotifications(
		//			new EmailNotificationsOptions
		//			{
		//				Emails = new List<string>()
		//			})
		//		.IsRetry() //Retry PlaceHolder
		//		.Build();

		//	int imageConfig = builder.ConfigureImageSync(
		//			new ImageSyncOptions
		//			{
		//				IdentifierFieldId = 100, //Control Number
		//				CopyImagesMode = ImportImageFileCopyMode.DoNotImportImageFiles,
		//				DestinationLocationType = DestinationLocationType.Folder,
		//				DestinationLocationId = 10,
		//				DataSourceType = DataSourceType.SavedSearch,
		//				DataSourceId = 10000
		//			})
		//		.CreateSavedSearch()
		//		.OverwriteMode(options =>
		//			options.OverwriteMode
		//			new OverwriteOptions
		//			{
		//				OverwriteMode = ImportOverwriteMode.AppendOverlay,
		//				FieldsOverlayBehavior = FieldOverlayBehavior.UseFieldSettings
		//			})
		//		.ExecutingInfo(
		//			new ExecutingInfoOptions
		//			{
		//				TriggeredBy = null
		//			})
		//		.EmailNotifications(
		//			new EmailNotificationsOptions
		//			{
		//				Emails = new List<string>()
		//			})
		//		.ProductionImagePrecedence(
		//			new ProductionImagePrecedenceOptions
		//			{
		//				ProductionImagePrecedenceIds = new int[] { 1, 2, 3 },
		//				IncludeOriginalImagesIfNotFoundInProductions = true
		//			})
		//		.IsRetry() //Retry PlaceHolder
		//		.Build();
		//}

		//[...(Guid())]
		//public class RDO
		//{
		//	[...(Guid)]
		//	public int Id { get; set; }
		//}

		//DestinationWorkspaceSavedSearchCreationConfiguration`)
	}
}
