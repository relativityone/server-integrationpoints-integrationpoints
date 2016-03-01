using System;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Data
{
	public class BaseFields
	{
		public const string SystemCreatedOn = "System Created On";
		public const string SystemCreatedBy = "System Created By";
		public const string LastModifiedOn = "Last Modified On";
		public const string LastModifiedBy = "Last Modified By";
	}

	public class FieldTypes
	{
		public const string SingleObject = "Single Object";
		public const string MultipleObject = "Multiple Object";
		public const string FixedLengthText = "Fixed Length Text";
		public const string SingleChoice = "Single Choice";
		public const string MultipleChoice = "MultipleChoice";
		public const string LongText = "Long Text";
		public const string User = "User";
		public const string YesNo = "Yes/No";
		public const string WholeNumber = "Whole Number";
		public const string Currency = "Currency";
		public const string Decimal = "Decimal";
		public const string @Date = "Date";
		public const string File = "File";
	}

	public partial class ObjectTypes
	{
		public const string Workspace = "Workspace";
		public const string Folder = "Folder";
		public const string IntegrationPoint = @"Integration Point";
		public const string SourceProvider = @"Source Provider";
		public const string DestinationProvider = @"Destination Provider";
		public const string JobHistory = @"Job History";
		public const string JobHistoryError = @"Job History Error";
	}

	public partial class ObjectTypeGuids
	{
		public const string IntegrationPoint = @"03d4f67e-22c9-488c-bee6-411f05c52e01";
		public const string SourceProvider = @"5be4a1f7-87a8-4cbe-a53f-5027d4f70b80";
		public const string DestinationProvider = @"d014f00d-f2c0-4e7a-b335-84fcb6eae980";
		public const string JobHistory = @"08f4b1f7-9692-4a08-94ab-b5f3a88b6cc9";
		public const string JobHistoryError = @"17e7912d-4f57-4890-9a37-abc2b8a37bdb";
		}

	#region "Field Constants"
	
	public partial class IntegrationPointFields : BaseFields
	{
		public const string NextScheduledRuntimeUTC = @"Next Scheduled Runtime (UTC)";
		public const string LastRuntimeUTC = @"Last Runtime (UTC)";
		public const string FieldMappings = @"Field Mappings";
		public const string EnableScheduler = @"Enable Scheduler";
		public const string SourceConfiguration = @"Source Configuration";
		public const string DestinationConfiguration = @"Destination Configuration";
		public const string SourceProvider = @"Source Provider";
		public const string ScheduleRule = @"Schedule Rule";
		public const string OverwriteFields = @"Overwrite Fields";
		public const string DestinationProvider = @"Destination Provider";
		public const string JobHistory = @"Job History";
		public const string LogErrors = @"LogErrors";
		public const string EmailNotificationRecipients = @"EmailNotificationRecipients";
		public const string Name = @"Name";
	}

	public partial class IntegrationPointFieldGuids 
	{
		public const string NextScheduledRuntimeUTC = @"5b1c9986-f166-40e4-a0dd-a56f185ff30b";
		public const string LastRuntimeUTC = @"90d58af1-f79f-40ae-85fc-7e42f84dbcc1";
		public const string FieldMappings = @"1b065787-a6e4-4d70-a7ed-f49d770f0bc7";
		public const string EnableScheduler = @"bcdafc41-311e-4b66-8084-4a8e0f56ca00";
		public const string SourceConfiguration = @"b5000e91-82bd-475a-86e9-32fefc04f4b8";
		public const string DestinationConfiguration = @"b1323ca7-34e5-4e6b-8ff1-e8d3b1a5fd0a";
		public const string SourceProvider = @"dc902551-2c9c-4f41-a917-41f4a3ef7409";
		public const string ScheduleRule = @"000f25ef-d714-4671-8075-d2a71cac396b";
		public const string OverwriteFields = @"0cae01d8-0dc3-4852-9359-fb954215c36f";
		public const string DestinationProvider = @"d6f4384a-0d2c-4eee-aab8-033cc77155ee";
		public const string JobHistory = @"14b230cf-a505-4dd3-b05c-c54d05e62966";
		public const string LogErrors = @"0319869e-37aa-499c-a95b-6d8d0e96a711";
		public const string EmailNotificationRecipients = @"1bac59db-f7bf-48e0-91d4-18cf09ff0e39";
		public const string Name = @"d534f433-dd92-4a53-b12d-bf85472e6d7a";
	}

	public partial class SourceProviderFields : BaseFields
	{
		public const string Identifier = @"Identifier";
		public const string SourceConfigurationUrl = @"Source Configuration Url";
		public const string ApplicationIdentifier = @"Application Identifier";
		public const string ViewConfigurationUrl = @"View Configuration Url";
		public const string Name = @"Name";
	}

	public partial class SourceProviderFieldGuids 
	{
		public const string Identifier = @"d0ecc6c9-472c-4296-83e1-0906f0c0fbb9";
		public const string SourceConfigurationUrl = @"b1b34def-3e77-48c3-97d4-eae7b5ee2213";
		public const string ApplicationIdentifier = @"0e696f9e-0e14-40f9-8cd7-34195defe5de";
		public const string ViewConfigurationUrl = @"bb036af8-1309-4f66-98f3-3495285b4a4b";
		public const string Name = @"9073997b-319e-482f-92fe-67e0b5860c1b";
	}

	public partial class DestinationProviderFields : BaseFields
	{
		public const string Identifier = @"Identifier";
		public const string ApplicationIdentifier = @"Application Identifier";
		public const string Name = @"Name";
	}

	public partial class DestinationProviderFieldGuids 
	{
		public const string Identifier = @"9fa104ac-13ea-4868-b716-17d6d786c77a";
		public const string ApplicationIdentifier = @"92892e25-0927-4073-b03d-e6a94ff80450";
		public const string Name = @"3ed18f54-c75a-4879-92a8-5ae23142bbeb";
	}

	public partial class JobHistoryFields : BaseFields
	{
		public const string JobStatus = @"Job Status";
		public const string DestinationWorkspace = "Destination Workspace";
		public const string RecordsImported = @"Records Imported";
		public const string TotalItems = @"Total Items";
		public const string RecordsWithErrors = @"Records with Errors";
		public const string StartTimeUTC = @"Start Time (UTC)";
		public const string EndTimeUTC = @"End Time (UTC)";
		public const string IntegrationPoint = @"Integration Point";
		public const string BatchInstance = @"Batch Instance";
		public const string Name = @"Name";
	}

	public partial class JobHistoryFieldGuids 
	{
		public const string JobStatus = @"5c28ce93-c62f-4d25-98c9-9a330a6feb52";
		public const string RecordsImported = @"70680399-c8ea-4b12-b711-e9ecbc53cb1c";
		public const string TotalItems = @"576189A9-0347-4B20-9369-B16D1AC89B4B";
		public const string DestinationWorkspace = @"FF01A766-B494-4F2C-9CBB-10A5AB163B8D";
		public const string RecordsWithErrors = @"c224104f-c1ca-4caa-9189-657e01d5504e";
		public const string StartTimeUTC = @"25b7c8ef-66d9-41d1-a8de-29a93e47fb11";
		public const string EndTimeUTC = @"4736cf49-ad0f-4f02-aaaa-898e07400f22";
		public const string IntegrationPoint = @"d3e791d3-2e21-45f4-b403-e7196bd25eea";
		public const string BatchInstance = @"08ba2c77-a9cd-4faf-a77a-be35e1ef1517";
		public const string Name = @"07061466-5fab-4581-979c-c801e8207370";
	}

	public partial class JobHistoryErrorFields : BaseFields
	{
		public const string JobHistory = @"JobHistory";
		public const string SourceUniqueID = @"Source Unique ID";
		public const string Error = @"Error";
		public const string StackTrace = @"StackTrace";
		public const string TimestampUTC = @"Timestamp (UTC)";
		public const string ErrorType = @"Error Type";
		public const string Name = @"Name";
	}

	public partial class JobHistoryErrorFieldGuids 
	{
		public const string JobHistory = @"8b747b91-0627-4130-8e53-2931ffc4135f";
		public const string SourceUniqueID = @"5519435e-ee82-4820-9546-f1af46121901";
		public const string Error = @"4112b894-35b0-4e53-ab99-c9036d08269d";
		public const string StackTrace = "0353DBDE-9E00-4227-8A8F-4380A8891CFF";
		public const string TimestampUTC = @"b9cba772-e7c9-493e-b7f8-8d605a6bfe1f";
		public const string ErrorType = @"eeffa5d3-82e3-46f8-9762-b4053d73f973";
		public const string Name = @"84e757cc-9da2-435d-b288-0c21ec589e66";
	}

	#endregion

	#region "Choice Constants"

	public partial class OverwriteFieldsChoices
	{
		public static Choice IntegrationPointAppend = new Choice(Guid.Parse("998c2b04-d42e-435b-9fba-11fec836aad8"), @"Append");
		public static Choice IntegrationPointAppendOverlay = new Choice(Guid.Parse("5450ebc3-ac57-4e6a-9d28-d607bbdcf6fd"), @"Append/Overlay");
		public static Choice IntegrationPointOverlayOnly = new Choice(Guid.Parse("70a1052d-93a3-4b72-9235-ac65f0d5a515"), @"Overlay Only");
	}

	public partial class JobStatusChoices
	{
		public static Choice JobHistoryPending = new Choice(Guid.Parse("24512aba-b8aa-4858-9324-5799033d7e96"), @"Pending");
		public static Choice JobHistoryProcessing = new Choice(Guid.Parse("bb170e53-2264-4708-9b00-86156187ed54"), @"Processing");
		public static Choice JobHistoryCompleted = new Choice(Guid.Parse("c7d1eb34-166e-48d0-bce7-0be0df43511c"), @"Completed");
		public static Choice JobHistoryCompletedWithErrors = new Choice(Guid.Parse("c0f4a2b2-499e-45bc-96d7-f8bc25e18b37"), @"Completed with errors");
		public static Choice JobHistoryErrorJobFailed = new Choice(Guid.Parse("3152ece9-40e6-44dd-afc8-1004f55dfb63"), @"Error - job failed");
	}

	public partial class ErrorTypeChoices
	{
		public static Choice JobHistoryErrorItem = new Choice(Guid.Parse("9ddc4914-fef3-401f-89b7-2967cd76714b"), @"Item");
		public static Choice JobHistoryErrorJob = new Choice(Guid.Parse("fa8bb625-05e6-4bf7-8573-012146baf19b"), @"Job");
	}

	#endregion								

	#region "Layouts"

	public partial class IntegrationPointLayoutGuids
	{
		public const string IntegrationPointDetails = @"f4a9ed1f-d874-4b07-b127-043e8ad0d506";
	}

	public partial class IntegrationPointLayouts
	{
		public const string IntegrationPointDetails = @"Integration Point Details";
	}

	public partial class SourceProviderLayoutGuids
	{
		public const string SourceProviderLayout = @"6d2ecb5d-ec2d-4b4b-b631-47fada8af8d4";
	}

	public partial class SourceProviderLayouts
	{
		public const string SourceProviderLayout = @"Source Provider Layout";
	}

	public partial class DestinationProviderLayoutGuids
	{
		public const string DestinationProviderLayout = @"806a3f21-3171-4093-afe6-b7a53cd2c4b5";
	}

	public partial class DestinationProviderLayouts
	{
		public const string DestinationProviderLayout = @"DestinationProvider Layout";
	}

	public partial class JobHistoryLayoutGuids
	{
		public const string JobDetails = @"04081530-f66e-4f56-bf07-7b325b53ffa9";
	}

	public partial class JobHistoryLayouts
	{
		public const string JobDetails = @"Job Details";
	}

	public partial class JobHistoryErrorLayoutGuids
	{
		public const string JobHistoryErrorLayout = @"52f405af-9903-47a8-a00b-f1359b548526";
	}

	public partial class JobHistoryErrorLayouts
	{
		public const string JobHistoryErrorLayout = @"Job History Error Layout";
	}

	#endregion
	
	#region "Tabs"

	public partial class IntegrationPointTabGuids
	{
		public const string IntegrationPoints = @"136c14c4-daa7-4542-abed-8d4e9b36a5dd";
	}

	public partial class IntegrationPointTabs
	{
		public const string IntegrationPoints = @"Integration Points";
	}

	public partial class SourceProviderTabGuids
	{
		public const string SourceProvider = @"72b6d3c7-1827-4b52-b16d-fdfa1b8fc898";
	}

	public partial class SourceProviderTabs
	{
		public const string SourceProvider = @"Source Provider";
	}

	public partial class DestinationProviderTabGuids
	{
		public const string DestinationProvider = @"8ee118d4-f0b4-4db5-ab1b-ad1d86ac8564";
	}

	public partial class DestinationProviderTabs
	{
		public const string DestinationProvider = @"DestinationProvider";
	}

	public partial class JobHistoryTabGuids
	{
		public const string JobHistory = @"7fdc0f38-dc5a-492a-93ff-77650b2581a9";
	}

	public partial class JobHistoryTabs
	{
		public const string JobHistory = @"Job History";
	}

	#endregion
	
	#region "Views"

	public partial class IntegrationPointViewGuids
	{
		public const string AllIntegrationPoints = @"181bf82a-e0dc-4a95-955a-0630bccb6afa";
	}

	public partial class IntegrationPointViews
	{
		public const string AllIntegrationPoints = @"All Integration Points";
	}

	public partial class SourceProviderViewGuids
	{
		public const string AllSourceProviders = @"f4e2c372-da19-4bb2-9c46-a1d6fa037136";
	}

	public partial class SourceProviderViews
	{
		public const string AllSourceProviders = @"All Source Providers";
	}

	public partial class DestinationProviderViewGuids
	{
		public const string AllDestinationProviders = @"602c03fd-3694-4547-ab39-598a95a957d2";
	}

	public partial class DestinationProviderViews
	{
		public const string AllDestinationProviders = @"All DestinationProviders";
	}

	public partial class JobHistoryViewGuids
	{
		public const string AllJobs = @"067aad47-7092-4782-aedf-157f5f1c7f4c";
		public const string AllJobsWithErrors = @"2d438c4d-afeb-4662-bfcd-935f4094751c";
	}

	public partial class JobHistoryViews
	{
		public const string AllJobs = @"All Jobs";
		public const string AllJobsWithErrors = @"All Jobs with Errors";
	}

	public partial class JobHistoryErrorViewGuids
	{
		public const string AllJobHistoryErrors = @"c0f2e9de-74f5-44b1-b3bf-335ac4ed50e0";
	}

	public partial class JobHistoryErrorViews
	{
		public const string AllJobHistoryErrors = @"All Job History Errors";
	}

	#endregion									
}
