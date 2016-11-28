using System;
using kCura.Relativity.Client.DTOs;
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
		public const string Document = @"Document";
		public const string IntegrationPoint = @"Integration Point";
		public const string SourceProvider = @"Source Provider";
		public const string DestinationProvider = @"Destination Provider";
		public const string JobHistory = @"Job History";
		public const string JobHistoryError = @"Job History Error";
		public const string DestinationWorkspace = @"Destination Workspace";
		public const string IntegrationPointType = @"Integration Point Type";
		public const string IntegrationPointProfile = @"Integration Point Profile";
		}

	public partial class ObjectTypeGuids
	{
		public const string Document = @"15c36703-74ea-4ff8-9dfb-ad30ece7530d";
		public const string IntegrationPoint = @"03d4f67e-22c9-488c-bee6-411f05c52e01";
		public const string SourceProvider = @"5be4a1f7-87a8-4cbe-a53f-5027d4f70b80";
		public const string DestinationProvider = @"d014f00d-f2c0-4e7a-b335-84fcb6eae980";
		public const string JobHistory = @"08f4b1f7-9692-4a08-94ab-b5f3a88b6cc9";
		public const string JobHistoryError = @"17e7912d-4f57-4890-9a37-abc2b8a37bdb";
		public const string DestinationWorkspace = @"3f45e490-b4cf-4c7d-8bb6-9ca891c0c198";
		public const string IntegrationPointType = @"3c4acdf0-41c8-4888-a374-383c25a734cb";
		public const string IntegrationPointProfile = @"6dc915a9-25d7-4500-97f7-07cb98a06f64";
		}

	#region "Field Constants"
	
	public partial class DocumentFields : BaseFields
	{
		public const string MarkupSetPrimary = @"Markup Set - Primary";
		public const string Batch = @"Batch";
		public const string BatchBatchSet = @"Batch::Batch Set";
		public const string BatchAssignedTo = @"Batch::Assigned To";
		public const string BatchStatus = @"Batch::Status";
		public const string RelativityDestinationCase = @"Relativity Destination Case";
		public const string JobHistory = @"Job History";
		public const string ControlNumber = @"Control Number";
	}

	public partial class DocumentFieldGuids 
	{
		public const string MarkupSetPrimary = @"b9b8964b-92f9-4d34-bd8a-69e303821db5";
		public const string Batch = @"d7a9d9fd-68fc-4c85-ad44-ba524a0ca872";
		public const string BatchBatchSet = @"de165970-de80-4c1e-90e3-8dd330b8138a";
		public const string BatchAssignedTo = @"81c45cf6-71e8-443c-ab82-877651aa5be4";
		public const string BatchStatus = @"478d3913-f243-4e43-93ad-c95935e71657";
		public const string RelativityDestinationCase = @"8980c2fa-0d33-4686-9a97-ea9d6f0b4196";
		public const string JobHistory = @"97bc12fa-509b-4c75-8413-6889387d8ef6";
		public const string ControlNumber = @"2a3f1212-c8ca-4fa9-ad6b-f76c97f05438";
	}



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
		public const string HasErrors = @"Has Errors";
		public const string Type = @"Type";
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
		public const string HasErrors = @"a9853e55-0ba0-43d8-a766-747a61471981";
		public const string Type = @"e646016e-5df6-4440-b218-18a00926d002";
		public const string Name = @"d534f433-dd92-4a53-b12d-bf85472e6d7a";
	}



	public partial class SourceProviderFields : BaseFields
	{
		public const string Identifier = @"Identifier";
		public const string SourceConfigurationUrl = @"Source Configuration Url";
		public const string ApplicationIdentifier = @"Application Identifier";
		public const string ViewConfigurationUrl = @"View Configuration Url";
		public const string Configuration = @"Configuration";
		public const string Name = @"Name";
	}

	public partial class SourceProviderFieldGuids 
	{
		public const string Identifier = @"d0ecc6c9-472c-4296-83e1-0906f0c0fbb9";
		public const string SourceConfigurationUrl = @"b1b34def-3e77-48c3-97d4-eae7b5ee2213";
		public const string ApplicationIdentifier = @"0e696f9e-0e14-40f9-8cd7-34195defe5de";
		public const string ViewConfigurationUrl = @"bb036af8-1309-4f66-98f3-3495285b4a4b";
		public const string Configuration = @"a85e3e30-e56a-4ddb-9282-fc37dc5e70d3";
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
		public const string Documents = @"Documents";
		public const string IntegrationPoint = @"Integration Point";
		public const string JobStatus = @"Job Status";
		public const string ItemsTransferred = @"Items Transferred";
		public const string ItemsWithErrors = @"Items with Errors";
		public const string StartTimeUTC = @"Start Time (UTC)";
		public const string EndTimeUTC = @"End Time (UTC)";
		public const string BatchInstance = @"Batch Instance";
		public const string DestinationWorkspace = @"Destination Workspace";
		public const string TotalItems = @"Total Items";
		public const string DestinationWorkspaceInformation = @"Destination Workspace Information";
		public const string JobType = @"Job Type";
		public const string Name = @"Name";
	}

	public partial class JobHistoryFieldGuids 
	{
		public const string Documents = @"5d99f717-3b5e-4773-9f51-9ca5d4c1a0fc";
		public const string IntegrationPoint = @"d3e791d3-2e21-45f4-b403-e7196bd25eea";
		public const string JobStatus = @"5c28ce93-c62f-4d25-98c9-9a330a6feb52";
		public const string ItemsTransferred = @"70680399-c8ea-4b12-b711-e9ecbc53cb1c";
		public const string ItemsWithErrors = @"c224104f-c1ca-4caa-9189-657e01d5504e";
		public const string StartTimeUTC = @"25b7c8ef-66d9-41d1-a8de-29a93e47fb11";
		public const string EndTimeUTC = @"4736cf49-ad0f-4f02-aaaa-898e07400f22";
		public const string BatchInstance = @"08ba2c77-a9cd-4faf-a77a-be35e1ef1517";
		public const string DestinationWorkspace = @"ff01a766-b494-4f2c-9cbb-10a5ab163b8d";
		public const string TotalItems = @"576189a9-0347-4b20-9369-b16d1ac89b4b";
		public const string DestinationWorkspaceInformation = @"20a24c4e-55e8-4fc2-abbe-f75c07fad91b";
		public const string JobType = @"e809db5e-5e99-4a75-98a1-26129313a3f5";
		public const string Name = @"07061466-5fab-4581-979c-c801e8207370";
	}



	public partial class JobHistoryErrorFields : BaseFields
	{
		public const string JobHistory = @"Job History";
		public const string SourceUniqueID = @"Source Unique ID";
		public const string Error = @"Error";
		public const string TimestampUTC = @"Timestamp (UTC)";
		public const string ErrorType = @"Error Type";
		public const string StackTrace = @"Stack Trace";
		public const string ErrorStatus = @"Error Status";
		public const string Name = @"Name";
	}

	public partial class JobHistoryErrorFieldGuids 
	{
		public const string JobHistory = @"8b747b91-0627-4130-8e53-2931ffc4135f";
		public const string SourceUniqueID = @"5519435e-ee82-4820-9546-f1af46121901";
		public const string Error = @"4112b894-35b0-4e53-ab99-c9036d08269d";
		public const string TimestampUTC = @"b9cba772-e7c9-493e-b7f8-8d605a6bfe1f";
		public const string ErrorType = @"eeffa5d3-82e3-46f8-9762-b4053d73f973";
		public const string StackTrace = @"0353dbde-9e00-4227-8a8f-4380a8891cff";
		public const string ErrorStatus = @"de1a46d2-d615-427a-b9f2-c10769bc2678";
		public const string Name = @"84e757cc-9da2-435d-b288-0c21ec589e66";
	}



	public partial class DestinationWorkspaceFields : BaseFields
	{
		public const string Documents = @"Documents";
		public const string JobHistory = @"Job History";
		public const string DestinationWorkspaceArtifactID = @"Destination Workspace Artifact ID";
		public const string DestinationWorkspaceName = @"Destination Workspace Name";
		public const string Name = @"Name";
	}

	public partial class DestinationWorkspaceFieldGuids 
	{
		public const string Documents = @"94ee2bd7-76d5-4d17-99e2-04768cce05e6";
		public const string JobHistory = @"07b8a468-dec8-45bd-b50a-989a35150be2";
		public const string DestinationWorkspaceArtifactID = @"207e6836-2961-466b-a0d2-29974a4fad36";
		public const string DestinationWorkspaceName = @"348d7394-2658-4da4-87d0-8183824adf98";
		public const string Name = @"155649c0-db15-4ee7-b449-bfdf2a54b7b5";
	}



	public partial class IntegrationPointTypeFields : BaseFields
	{
		public const string ApplicationIdentifier = @"Application Identifier";
		public const string Identifier = @"Identifier";
		public const string Name = @"Name";
	}

	public partial class IntegrationPointTypeFieldGuids 
	{
		public const string ApplicationIdentifier = @"9720e543-cce0-445c-8af7-042355671a71";
		public const string Identifier = @"3bd675a0-555d-49bc-b108-e2d04afcc1e3";
		public const string Name = @"ae4fe868-5428-49fd-b2e4-bb17abd597ef";
	}



	public partial class IntegrationPointProfileFields : BaseFields
	{
		public const string DestinationConfiguration = @"Destination Configuration";
		public const string DestinationProvider = @"Destination Provider";
		public const string EmailNotificationRecipients = @"EmailNotificationRecipients";
		public const string EnableScheduler = @"Enable Scheduler";
		public const string FieldMappings = @"Field Mappings";
		public const string LogErrors = @"LogErrors";
		public const string NextScheduledRuntimeUTC = @"Next Scheduled Runtime (UTC)";
		public const string OverwriteFields = @"Overwrite Fields";
		public const string ScheduleRule = @"Schedule Rule";
		public const string SourceConfiguration = @"Source Configuration";
		public const string SourceProvider = @"Source Provider";
		public const string Type = @"Type";
		public const string Name = @"Name";
	}

	public partial class IntegrationPointProfileFieldGuids 
	{
		public const string DestinationConfiguration = @"5d9e425a-b59c-4119-9ceb-73665a5e7049";
		public const string DestinationProvider = @"7d9e7944-bf13-4c4f-a9eb-5f60e683ec0c";
		public const string EmailNotificationRecipients = @"b72f5bb9-2e07-45f0-903a-b20d3a17958c";
		public const string EnableScheduler = @"bc2e19fd-c95c-4f1c-b4a9-1692590cef8e";
		public const string FieldMappings = @"8ae37734-29d1-4441-b5d8-483134f98818";
		public const string LogErrors = @"b582f002-00fe-4c44-b721-859dd011d4fd";
		public const string NextScheduledRuntimeUTC = @"a3c48572-4ec7-4e06-b57f-1c1681cd07d1";
		public const string OverwriteFields = @"e0a14a2c-0bb6-47ad-a34f-26400258a761";
		public const string ScheduleRule = @"35b6c8b8-5b0c-4660-bfdd-226e424edeb5";
		public const string SourceConfiguration = @"9ed96d44-7767-46f5-a67f-28b48b155ff2";
		public const string SourceProvider = @"60d3de54-f0d5-4744-a23f-a17609edc537";
		public const string Type = @"8999dd19-c67c-43e3-88c0-edc989e224cc";
		public const string Name = @"ad0552e0-4511-4507-a60d-23c0c6d05972";
	}



	#endregion

	#region "Choice Constants"

	public partial class OverwriteFieldsChoices
	{
		public static Choice IntegrationPointAppendOnly = new Choice(Guid.Parse("998c2b04-d42e-435b-9fba-11fec836aad8")) {Name=@"Append Only"};
		public static Choice IntegrationPointAppendOverlay = new Choice(Guid.Parse("5450ebc3-ac57-4e6a-9d28-d607bbdcf6fd")) {Name=@"Append/Overlay"};
		public static Choice IntegrationPointOverlayOnly = new Choice(Guid.Parse("70a1052d-93a3-4b72-9235-ac65f0d5a515")) {Name=@"Overlay Only"};
	}

	public partial class JobStatusChoices
	{
		public static Choice JobHistoryPending = new Choice(Guid.Parse("24512aba-b8aa-4858-9324-5799033d7e96")) {Name=@"Pending"};
		public static Choice JobHistoryProcessing = new Choice(Guid.Parse("bb170e53-2264-4708-9b00-86156187ed54")) {Name=@"Processing"};
		public static Choice JobHistoryCompleted = new Choice(Guid.Parse("c7d1eb34-166e-48d0-bce7-0be0df43511c")) {Name=@"Completed"};
		public static Choice JobHistoryCompletedWithErrors = new Choice(Guid.Parse("c0f4a2b2-499e-45bc-96d7-f8bc25e18b37")) {Name=@"Completed with errors"};
		public static Choice JobHistoryErrorJobFailed = new Choice(Guid.Parse("3152ece9-40e6-44dd-afc8-1004f55dfb63")) {Name=@"Error - job failed"};
		public static Choice JobHistoryStopping = new Choice(Guid.Parse("97c1410d-864d-4811-857b-952464872baa")) {Name=@"Stopping"};
		public static Choice JobHistoryStopped = new Choice(Guid.Parse("a29c5bcb-d3a6-4f81-877a-2a6556c996c3")) {Name=@"Stopped"};
	}

	public partial class JobTypeChoices
	{
		public static Choice JobHistoryRun = new Choice(Guid.Parse("86c8c17d-74ec-4187-bdb1-9380252f4c20")) {Name=@"Run"};
		public static Choice JobHistoryScheduledRun = new Choice(Guid.Parse("79510ad3-49cb-4b4f-840c-c64247404a4d")) {Name=@"Scheduled Run"};
		public static Choice JobHistoryRetryErrors = new Choice(Guid.Parse("b0171a20-2042-44eb-a957-5dbc9c377c2f")) {Name=@"Retry Errors"};
	}

	public partial class ErrorTypeChoices
	{
		public static Choice JobHistoryErrorItem = new Choice(Guid.Parse("9ddc4914-fef3-401f-89b7-2967cd76714b")) {Name=@"Item"};
		public static Choice JobHistoryErrorJob = new Choice(Guid.Parse("fa8bb625-05e6-4bf7-8573-012146baf19b")) {Name=@"Job"};
	}

	public partial class ErrorStatusChoices
	{
		public static Choice JobHistoryErrorNew = new Choice(Guid.Parse("f881b199-8a67-4d49-b1c1-f9e68658fb5a")) {Name=@"New"};
		public static Choice JobHistoryErrorExpired = new Choice(Guid.Parse("af01a8fa-b419-49b1-bd71-25296e221e57")) {Name=@"Expired"};
		public static Choice JobHistoryErrorInProgress = new Choice(Guid.Parse("e5ebd98c-c976-4fa2-936f-434e265ea0aa")) {Name=@"In Progress"};
		public static Choice JobHistoryErrorRetried = new Choice(Guid.Parse("7d3d393d-384f-434e-9776-f9966550d29a")) {Name=@"Retried"};
	}

	public partial class OverwriteFieldsChoices
	{
		public static Choice IntegrationPointProfileAppendOnly = new Choice(Guid.Parse("12105945-5fb8-4640-8516-11a96f12279c")) {Name=@"Append Only"};
		public static Choice IntegrationPointProfileAppendOverlay = new Choice(Guid.Parse("e5c80435-d876-4cba-b645-658a545eaea1")) {Name=@"Append/Overlay"};
		public static Choice IntegrationPointProfileOverlayOnly = new Choice(Guid.Parse("e08fc9e5-416c-4656-a9a1-6323013160fb")) {Name=@"Overlay Only"};
	}

	#endregion								

	#region "Layouts"

	public partial class DocumentLayoutGuids
	{
	}

	public partial class DocumentLayouts
	{
	}

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

	public partial class DestinationWorkspaceLayoutGuids
	{
		public const string DestinationWorkspaceLayout = @"ed0da23b-191d-40b3-8171-fcdf57342436";
	}

	public partial class DestinationWorkspaceLayouts
	{
		public const string DestinationWorkspaceLayout = @"Destination Workspace Layout";
	}

	public partial class IntegrationPointTypeLayoutGuids
	{
		public const string IntegrationPointTypeLayout = @"64197e82-591d-4b2c-a971-b760b11e8307";
	}

	public partial class IntegrationPointTypeLayouts
	{
		public const string IntegrationPointTypeLayout = @"Integration Point Type Layout";
	}

	public partial class IntegrationPointProfileLayoutGuids
	{
		public const string IntegrationPointProfileLayout = @"f8505b51-802b-466f-952b-2e0eb7aadb2f";
	}

	public partial class IntegrationPointProfileLayouts
	{
		public const string IntegrationPointProfileLayout = @"Integration Point Profile Layout";
	}

	#endregion
	
	
	#region "Tabs"

	public partial class IntegrationPointTabGuids
	{
		public const string IntegrationPoints = @"4a9d9b4c-cec1-4c68-8a6d-c0f22516f032";
	}

	public partial class IntegrationPointTabs
	{
		public const string IntegrationPoints = @"Integration Points";
	}

	public partial class JobHistoryTabGuids
	{
		public const string JobHistory = @"677d1324-f251-4cb5-80c1-fb079f05962a";
	}

	public partial class JobHistoryTabs
	{
		public const string JobHistory = @"Job History";
	}

	public partial class JobHistoryErrorTabGuids
	{
		public const string JobHistoryErrors = @"fd585dbf-98ea-427b-8ce5-3e09a053dc14";
	}

	public partial class JobHistoryErrorTabs
	{
		public const string JobHistoryErrors = @"Job History Errors";
	}

	public partial class DestinationWorkspaceTabGuids
	{
		public const string DestinationWorkspaces = @"d04fc871-7a46-47a2-bbe5-83318b2d641e";
	}

	public partial class DestinationWorkspaceTabs
	{
		public const string DestinationWorkspaces = @"Destination Workspaces";
	}

	public partial class IntegrationPointProfileTabGuids
	{
		public const string Profile = @"0ac27fca-a6fe-425b-87f0-afb12e40563a";
	}

	public partial class IntegrationPointProfileTabs
	{
		public const string Profile = @"Profile";
	}

	#endregion
	
	#region "Views"

	public partial class DocumentViewGuids
	{
		public const string DocumentsDestinationWorkspace = @"c18d034d-e911-4401-a370-827aa303bdd8";
	}

	public partial class DocumentViews
	{
		public const string DocumentsDestinationWorkspace = @"Documents (Destination Workspace)";
	}

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

	public partial class DestinationWorkspaceViewGuids
	{
		public const string AllDestinationWorkspaces = @"a74341a4-08c7-4671-883f-59bdd12780a7";
	}

	public partial class DestinationWorkspaceViews
	{
		public const string AllDestinationWorkspaces = @"All Destination Workspaces";
	}

	public partial class IntegrationPointTypeViewGuids
	{
		public const string AllIntegrationPointTypes = @"57fd219a-a2e4-4110-8956-af6c026041ee";
	}

	public partial class IntegrationPointTypeViews
	{
		public const string AllIntegrationPointTypes = @"All Integration Point Types";
	}

	public partial class IntegrationPointProfileViewGuids
	{
		public const string AllProfiles = @"2b9d50cb-6d0e-46f3-a4c5-110001526704";
	}

	public partial class IntegrationPointProfileViews
	{
		public const string AllProfiles = @"All Profiles";
	}

	#endregion									

}
