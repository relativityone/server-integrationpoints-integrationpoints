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
		public const string SyncConfiguration = @"Sync Configuration";
		public const string SyncBatch = @"Sync Batch";
		public const string SyncProgress = @"Sync Progress";
		}

	public partial class ObjectTypeGuids
	{
		public const string Document = @"15c36703-74ea-4ff8-9dfb-ad30ece7530d";
		public static readonly Guid  DocumentGuid = Guid.Parse(Document);
		public const string IntegrationPoint = @"03d4f67e-22c9-488c-bee6-411f05c52e01";
		public static readonly Guid  IntegrationPointGuid = Guid.Parse(IntegrationPoint);
		public const string SourceProvider = @"5be4a1f7-87a8-4cbe-a53f-5027d4f70b80";
		public static readonly Guid  SourceProviderGuid = Guid.Parse(SourceProvider);
		public const string DestinationProvider = @"d014f00d-f2c0-4e7a-b335-84fcb6eae980";
		public static readonly Guid  DestinationProviderGuid = Guid.Parse(DestinationProvider);
		public const string JobHistory = @"08f4b1f7-9692-4a08-94ab-b5f3a88b6cc9";
		public static readonly Guid  JobHistoryGuid = Guid.Parse(JobHistory);
		public const string JobHistoryError = @"17e7912d-4f57-4890-9a37-abc2b8a37bdb";
		public static readonly Guid  JobHistoryErrorGuid = Guid.Parse(JobHistoryError);
		public const string DestinationWorkspace = @"3f45e490-b4cf-4c7d-8bb6-9ca891c0c198";
		public static readonly Guid  DestinationWorkspaceGuid = Guid.Parse(DestinationWorkspace);
		public const string IntegrationPointType = @"3c4acdf0-41c8-4888-a374-383c25a734cb";
		public static readonly Guid  IntegrationPointTypeGuid = Guid.Parse(IntegrationPointType);
		public const string IntegrationPointProfile = @"6dc915a9-25d7-4500-97f7-07cb98a06f64";
		public static readonly Guid  IntegrationPointProfileGuid = Guid.Parse(IntegrationPointProfile);
		public const string SyncConfiguration = @"3be3de56-839f-4f0e-8446-e1691ed5fd57";
		public static readonly Guid  SyncConfigurationGuid = Guid.Parse(SyncConfiguration);
		public const string SyncBatch = @"18c766eb-eb71-49e4-983e-ffde29b1a44e";
		public static readonly Guid  SyncBatchGuid = Guid.Parse(SyncBatch);
		public const string SyncProgress = @"3d107450-db18-4fe1-8219-73ee1f921ed9";
		public static readonly Guid  SyncProgressGuid = Guid.Parse(SyncProgress);
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
		public const string MarkupSetPrimary = @"14292546-9c9c-4210-998b-4b5ff4d89e58";
		public static readonly Guid  MarkupSetPrimaryGuid = Guid.Parse(MarkupSetPrimary);
		public const string Batch = @"d7a9d9fd-68fc-4c85-ad44-ba524a0ca872";
		public static readonly Guid  BatchGuid = Guid.Parse(Batch);
		public const string BatchBatchSet = @"705dad6c-843d-4131-b470-f65874366fa7";
		public static readonly Guid  BatchBatchSetGuid = Guid.Parse(BatchBatchSet);
		public const string BatchAssignedTo = @"e8de43da-fbdf-4319-8ee6-e865dbf70bdb";
		public static readonly Guid  BatchAssignedToGuid = Guid.Parse(BatchAssignedTo);
		public const string BatchStatus = @"2d8b37df-02ac-4a66-834a-3bfc2de78486";
		public static readonly Guid  BatchStatusGuid = Guid.Parse(BatchStatus);
		public const string RelativityDestinationCase = @"8980c2fa-0d33-4686-9a97-ea9d6f0b4196";
		public static readonly Guid  RelativityDestinationCaseGuid = Guid.Parse(RelativityDestinationCase);
		public const string JobHistory = @"97bc12fa-509b-4c75-8413-6889387d8ef6";
		public static readonly Guid  JobHistoryGuid = Guid.Parse(JobHistory);
		public const string ControlNumber = @"2a3f1212-c8ca-4fa9-ad6b-f76c97f05438";
		public static readonly Guid  ControlNumberGuid = Guid.Parse(ControlNumber);
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
		public const string SecuredConfiguration = @"Secured Configuration";
		public const string PromoteEligible = @"Promote Eligible";
		public const string Name = @"Name";
	}

	public partial class IntegrationPointFieldGuids 
	{
		public const string NextScheduledRuntimeUTC = @"5b1c9986-f166-40e4-a0dd-a56f185ff30b";
		public static readonly Guid  NextScheduledRuntimeUTCGuid = Guid.Parse(NextScheduledRuntimeUTC);
		public const string LastRuntimeUTC = @"90d58af1-f79f-40ae-85fc-7e42f84dbcc1";
		public static readonly Guid  LastRuntimeUTCGuid = Guid.Parse(LastRuntimeUTC);
		public const string FieldMappings = @"1b065787-a6e4-4d70-a7ed-f49d770f0bc7";
		public static readonly Guid  FieldMappingsGuid = Guid.Parse(FieldMappings);
		public const string EnableScheduler = @"bcdafc41-311e-4b66-8084-4a8e0f56ca00";
		public static readonly Guid  EnableSchedulerGuid = Guid.Parse(EnableScheduler);
		public const string SourceConfiguration = @"b5000e91-82bd-475a-86e9-32fefc04f4b8";
		public static readonly Guid  SourceConfigurationGuid = Guid.Parse(SourceConfiguration);
		public const string DestinationConfiguration = @"b1323ca7-34e5-4e6b-8ff1-e8d3b1a5fd0a";
		public static readonly Guid  DestinationConfigurationGuid = Guid.Parse(DestinationConfiguration);
		public const string SourceProvider = @"dc902551-2c9c-4f41-a917-41f4a3ef7409";
		public static readonly Guid  SourceProviderGuid = Guid.Parse(SourceProvider);
		public const string ScheduleRule = @"000f25ef-d714-4671-8075-d2a71cac396b";
		public static readonly Guid  ScheduleRuleGuid = Guid.Parse(ScheduleRule);
		public const string OverwriteFields = @"0cae01d8-0dc3-4852-9359-fb954215c36f";
		public static readonly Guid  OverwriteFieldsGuid = Guid.Parse(OverwriteFields);
		public const string DestinationProvider = @"d6f4384a-0d2c-4eee-aab8-033cc77155ee";
		public static readonly Guid  DestinationProviderGuid = Guid.Parse(DestinationProvider);
		public const string JobHistory = @"14b230cf-a505-4dd3-b05c-c54d05e62966";
		public static readonly Guid  JobHistoryGuid = Guid.Parse(JobHistory);
		public const string LogErrors = @"0319869e-37aa-499c-a95b-6d8d0e96a711";
		public static readonly Guid  LogErrorsGuid = Guid.Parse(LogErrors);
		public const string EmailNotificationRecipients = @"1bac59db-f7bf-48e0-91d4-18cf09ff0e39";
		public static readonly Guid  EmailNotificationRecipientsGuid = Guid.Parse(EmailNotificationRecipients);
		public const string HasErrors = @"a9853e55-0ba0-43d8-a766-747a61471981";
		public static readonly Guid  HasErrorsGuid = Guid.Parse(HasErrors);
		public const string Type = @"e646016e-5df6-4440-b218-18a00926d002";
		public static readonly Guid  TypeGuid = Guid.Parse(Type);
		public const string SecuredConfiguration = @"48b0a4cb-bc21-45b5-b124-76ae27e03c42";
		public static readonly Guid  SecuredConfigurationGuid = Guid.Parse(SecuredConfiguration);
		public const string PromoteEligible = @"bf85f332-8c8f-4c69-86fd-6ce4c567ebf9";
		public static readonly Guid  PromoteEligibleGuid = Guid.Parse(PromoteEligible);
		public const string Name = @"d534f433-dd92-4a53-b12d-bf85472e6d7a";
		public static readonly Guid  NameGuid = Guid.Parse(Name);
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
		public static readonly Guid  IdentifierGuid = Guid.Parse(Identifier);
		public const string SourceConfigurationUrl = @"b1b34def-3e77-48c3-97d4-eae7b5ee2213";
		public static readonly Guid  SourceConfigurationUrlGuid = Guid.Parse(SourceConfigurationUrl);
		public const string ApplicationIdentifier = @"0e696f9e-0e14-40f9-8cd7-34195defe5de";
		public static readonly Guid  ApplicationIdentifierGuid = Guid.Parse(ApplicationIdentifier);
		public const string ViewConfigurationUrl = @"bb036af8-1309-4f66-98f3-3495285b4a4b";
		public static readonly Guid  ViewConfigurationUrlGuid = Guid.Parse(ViewConfigurationUrl);
		public const string Configuration = @"a85e3e30-e56a-4ddb-9282-fc37dc5e70d3";
		public static readonly Guid  ConfigurationGuid = Guid.Parse(Configuration);
		public const string Name = @"9073997b-319e-482f-92fe-67e0b5860c1b";
		public static readonly Guid  NameGuid = Guid.Parse(Name);
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
		public static readonly Guid  IdentifierGuid = Guid.Parse(Identifier);
		public const string ApplicationIdentifier = @"92892e25-0927-4073-b03d-e6a94ff80450";
		public static readonly Guid  ApplicationIdentifierGuid = Guid.Parse(ApplicationIdentifier);
		public const string Name = @"3ed18f54-c75a-4879-92a8-5ae23142bbeb";
		public static readonly Guid  NameGuid = Guid.Parse(Name);
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
		public const string DestinationInstance = @"Destination Instance";
		public const string FilesSize = @"FilesSize";
		public const string Overwrite = @"Overwrite";
		public const string JobID = @"Job ID";
		public const string Name = @"Name";
	}

	public partial class JobHistoryFieldGuids 
	{
		public const string Documents = @"5d99f717-3b5e-4773-9f51-9ca5d4c1a0fc";
		public static readonly Guid  DocumentsGuid = Guid.Parse(Documents);
		public const string IntegrationPoint = @"d3e791d3-2e21-45f4-b403-e7196bd25eea";
		public static readonly Guid  IntegrationPointGuid = Guid.Parse(IntegrationPoint);
		public const string JobStatus = @"5c28ce93-c62f-4d25-98c9-9a330a6feb52";
		public static readonly Guid  JobStatusGuid = Guid.Parse(JobStatus);
		public const string ItemsTransferred = @"70680399-c8ea-4b12-b711-e9ecbc53cb1c";
		public static readonly Guid  ItemsTransferredGuid = Guid.Parse(ItemsTransferred);
		public const string ItemsWithErrors = @"c224104f-c1ca-4caa-9189-657e01d5504e";
		public static readonly Guid  ItemsWithErrorsGuid = Guid.Parse(ItemsWithErrors);
		public const string StartTimeUTC = @"25b7c8ef-66d9-41d1-a8de-29a93e47fb11";
		public static readonly Guid  StartTimeUTCGuid = Guid.Parse(StartTimeUTC);
		public const string EndTimeUTC = @"4736cf49-ad0f-4f02-aaaa-898e07400f22";
		public static readonly Guid  EndTimeUTCGuid = Guid.Parse(EndTimeUTC);
		public const string BatchInstance = @"08ba2c77-a9cd-4faf-a77a-be35e1ef1517";
		public static readonly Guid  BatchInstanceGuid = Guid.Parse(BatchInstance);
		public const string DestinationWorkspace = @"ff01a766-b494-4f2c-9cbb-10a5ab163b8d";
		public static readonly Guid  DestinationWorkspaceGuid = Guid.Parse(DestinationWorkspace);
		public const string TotalItems = @"576189a9-0347-4b20-9369-b16d1ac89b4b";
		public static readonly Guid  TotalItemsGuid = Guid.Parse(TotalItems);
		public const string DestinationWorkspaceInformation = @"20a24c4e-55e8-4fc2-abbe-f75c07fad91b";
		public static readonly Guid  DestinationWorkspaceInformationGuid = Guid.Parse(DestinationWorkspaceInformation);
		public const string JobType = @"e809db5e-5e99-4a75-98a1-26129313a3f5";
		public static readonly Guid  JobTypeGuid = Guid.Parse(JobType);
		public const string DestinationInstance = @"6d91ea1e-7b34-46a9-854e-2b018d4e35ef";
		public static readonly Guid  DestinationInstanceGuid = Guid.Parse(DestinationInstance);
		public const string FilesSize = @"d81817dc-91cb-44c4-b9b7-7c445da64f5a";
		public static readonly Guid  FilesSizeGuid = Guid.Parse(FilesSize);
		public const string Overwrite = @"42d49f5e-b0e7-4632-8d30-1c6ee1d97fa7";
		public static readonly Guid  OverwriteGuid = Guid.Parse(Overwrite);
		public const string JobID = @"77d797ef-96c9-4b47-9ef8-33f498b5af0d";
		public static readonly Guid  JobIDGuid = Guid.Parse(JobID);
		public const string Name = @"07061466-5fab-4581-979c-c801e8207370";
		public static readonly Guid  NameGuid = Guid.Parse(Name);
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
		public static readonly Guid  JobHistoryGuid = Guid.Parse(JobHistory);
		public const string SourceUniqueID = @"5519435e-ee82-4820-9546-f1af46121901";
		public static readonly Guid  SourceUniqueIDGuid = Guid.Parse(SourceUniqueID);
		public const string Error = @"4112b894-35b0-4e53-ab99-c9036d08269d";
		public static readonly Guid  ErrorGuid = Guid.Parse(Error);
		public const string TimestampUTC = @"b9cba772-e7c9-493e-b7f8-8d605a6bfe1f";
		public static readonly Guid  TimestampUTCGuid = Guid.Parse(TimestampUTC);
		public const string ErrorType = @"eeffa5d3-82e3-46f8-9762-b4053d73f973";
		public static readonly Guid  ErrorTypeGuid = Guid.Parse(ErrorType);
		public const string StackTrace = @"0353dbde-9e00-4227-8a8f-4380a8891cff";
		public static readonly Guid  StackTraceGuid = Guid.Parse(StackTrace);
		public const string ErrorStatus = @"de1a46d2-d615-427a-b9f2-c10769bc2678";
		public static readonly Guid  ErrorStatusGuid = Guid.Parse(ErrorStatus);
		public const string Name = @"84e757cc-9da2-435d-b288-0c21ec589e66";
		public static readonly Guid  NameGuid = Guid.Parse(Name);
	}



	public partial class DestinationWorkspaceFields : BaseFields
	{
		public const string Documents = @"Documents";
		public const string JobHistory = @"Job History";
		public const string DestinationWorkspaceArtifactID = @"Destination Workspace Artifact ID";
		public const string DestinationWorkspaceName = @"Destination Workspace Name";
		public const string DestinationInstanceName = @"Destination Instance Name";
		public const string DestinationInstanceArtifactID = @"Destination Instance Artifact ID";
		public const string Name = @"Name";
	}

	public partial class DestinationWorkspaceFieldGuids 
	{
		public const string Documents = @"94ee2bd7-76d5-4d17-99e2-04768cce05e6";
		public static readonly Guid  DocumentsGuid = Guid.Parse(Documents);
		public const string JobHistory = @"07b8a468-dec8-45bd-b50a-989a35150be2";
		public static readonly Guid  JobHistoryGuid = Guid.Parse(JobHistory);
		public const string DestinationWorkspaceArtifactID = @"207e6836-2961-466b-a0d2-29974a4fad36";
		public static readonly Guid  DestinationWorkspaceArtifactIDGuid = Guid.Parse(DestinationWorkspaceArtifactID);
		public const string DestinationWorkspaceName = @"348d7394-2658-4da4-87d0-8183824adf98";
		public static readonly Guid  DestinationWorkspaceNameGuid = Guid.Parse(DestinationWorkspaceName);
		public const string DestinationInstanceName = @"909adc7c-2bb9-46ca-9f85-da32901d6554";
		public static readonly Guid  DestinationInstanceNameGuid = Guid.Parse(DestinationInstanceName);
		public const string DestinationInstanceArtifactID = @"323458db-8a06-464b-9402-af2516cf47e0";
		public static readonly Guid  DestinationInstanceArtifactIDGuid = Guid.Parse(DestinationInstanceArtifactID);
		public const string Name = @"155649c0-db15-4ee7-b449-bfdf2a54b7b5";
		public static readonly Guid  NameGuid = Guid.Parse(Name);
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
		public static readonly Guid  ApplicationIdentifierGuid = Guid.Parse(ApplicationIdentifier);
		public const string Identifier = @"3bd675a0-555d-49bc-b108-e2d04afcc1e3";
		public static readonly Guid  IdentifierGuid = Guid.Parse(Identifier);
		public const string Name = @"ae4fe868-5428-49fd-b2e4-bb17abd597ef";
		public static readonly Guid  NameGuid = Guid.Parse(Name);
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
		public const string PromoteEligible = @"Promote Eligible";
		public const string Name = @"Name";
	}

	public partial class IntegrationPointProfileFieldGuids 
	{
		public const string DestinationConfiguration = @"5d9e425a-b59c-4119-9ceb-73665a5e7049";
		public static readonly Guid  DestinationConfigurationGuid = Guid.Parse(DestinationConfiguration);
		public const string DestinationProvider = @"7d9e7944-bf13-4c4f-a9eb-5f60e683ec0c";
		public static readonly Guid  DestinationProviderGuid = Guid.Parse(DestinationProvider);
		public const string EmailNotificationRecipients = @"b72f5bb9-2e07-45f0-903a-b20d3a17958c";
		public static readonly Guid  EmailNotificationRecipientsGuid = Guid.Parse(EmailNotificationRecipients);
		public const string EnableScheduler = @"bc2e19fd-c95c-4f1c-b4a9-1692590cef8e";
		public static readonly Guid  EnableSchedulerGuid = Guid.Parse(EnableScheduler);
		public const string FieldMappings = @"8ae37734-29d1-4441-b5d8-483134f98818";
		public static readonly Guid  FieldMappingsGuid = Guid.Parse(FieldMappings);
		public const string LogErrors = @"b582f002-00fe-4c44-b721-859dd011d4fd";
		public static readonly Guid  LogErrorsGuid = Guid.Parse(LogErrors);
		public const string NextScheduledRuntimeUTC = @"a3c48572-4ec7-4e06-b57f-1c1681cd07d1";
		public static readonly Guid  NextScheduledRuntimeUTCGuid = Guid.Parse(NextScheduledRuntimeUTC);
		public const string OverwriteFields = @"e0a14a2c-0bb6-47ad-a34f-26400258a761";
		public static readonly Guid  OverwriteFieldsGuid = Guid.Parse(OverwriteFields);
		public const string ScheduleRule = @"35b6c8b8-5b0c-4660-bfdd-226e424edeb5";
		public static readonly Guid  ScheduleRuleGuid = Guid.Parse(ScheduleRule);
		public const string SourceConfiguration = @"9ed96d44-7767-46f5-a67f-28b48b155ff2";
		public static readonly Guid  SourceConfigurationGuid = Guid.Parse(SourceConfiguration);
		public const string SourceProvider = @"60d3de54-f0d5-4744-a23f-a17609edc537";
		public static readonly Guid  SourceProviderGuid = Guid.Parse(SourceProvider);
		public const string Type = @"8999dd19-c67c-43e3-88c0-edc989e224cc";
		public static readonly Guid  TypeGuid = Guid.Parse(Type);
		public const string PromoteEligible = @"4997cdb4-d8b0-4eaa-8768-061d82aaaccf";
		public static readonly Guid  PromoteEligibleGuid = Guid.Parse(PromoteEligible);
		public const string Name = @"ad0552e0-4511-4507-a60d-23c0c6d05972";
		public static readonly Guid  NameGuid = Guid.Parse(Name);
	}



	public partial class SyncConfigurationFields : BaseFields
	{
		public const string JobHistory = @"JobHistory";
		public const string EmailNotificationRecipients = @"Email notification recipients";
		public const string FieldMappings = @"Field mappings";
		public const string DataSourceArtifactID = @"Data source artifact ID";
		public const string DataSourceType = @"Data source type";
		public const string DestinationWorkspaceArtifactID = @"Destination workspace artifact ID";
		public const string RDOArtifactTypeID = @"RDO artifact type ID";
		public const string CreateSavedSearchInDestination = @"Create saved search in destination";
		public const string DataDestinationArtifactID = @"Data destination artifact ID";
		public const string DataDestinationType = @"Data destination type";
		public const string ImportOverwriteMode = @"Import overwrite mode";
		public const string NativesBehavior = @"Natives behavior";
		public const string DestinationFolderStructureBehavior = @"Destination folder structure behavior";
		public const string MoveExistingDocuments = @"Move existing documents";
		public const string FieldOverlayBehavior = @"Field overlay behavior";
		public const string SnapshotID = @"Snapshot ID";
		public const string SnapshotRecordsCount = @"Snapshot records count";
		public const string SavedSearchInDestinationArtifactID = @"Saved search in destination artifact ID";
		public const string SourceJobTagArtifactID = @"Source job tag artifact ID";
		public const string SourceJobTagName = @"Source job tag name";
		public const string SourceWorkspaceTagArtifactID = @"Source workspace tag artifact ID";
		public const string SourceWorkspaceTagName = @"Source workspace tag name";
		public const string DestinationWorkspaceTagArtifactID = @"Destination workspace tag artifact ID";
		public const string FolderPathSourceFieldArtifactID = @"Folder path source field artifact ID";
		public const string Name = @"Name";
	}

	public partial class SyncConfigurationFieldGuids 
	{
		public const string JobHistory = @"5d8f7f01-25cf-4246-b2e2-c05882539bb2";
		public static readonly Guid  JobHistoryGuid = Guid.Parse(JobHistory);
		public const string EmailNotificationRecipients = @"4f03914d-9e86-4b72-b75c-ee48feebb583";
		public static readonly Guid  EmailNotificationRecipientsGuid = Guid.Parse(EmailNotificationRecipients);
		public const string FieldMappings = @"e3cb5c64-c726-47f8-9cb0-1391c5911628";
		public static readonly Guid  FieldMappingsGuid = Guid.Parse(FieldMappings);
		public const string DataSourceArtifactID = @"6d8631f9-0ea1-4eb9-b7b2-c552f43959d0";
		public static readonly Guid  DataSourceArtifactIDGuid = Guid.Parse(DataSourceArtifactID);
		public const string DataSourceType = @"a00e6bc1-ca1c-48d9-9712-629a63061f0d";
		public static readonly Guid  DataSourceTypeGuid = Guid.Parse(DataSourceType);
		public const string DestinationWorkspaceArtifactID = @"15b88438-6cf7-47ab-b630-424633159c69";
		public static readonly Guid  DestinationWorkspaceArtifactIDGuid = Guid.Parse(DestinationWorkspaceArtifactID);
		public const string RDOArtifactTypeID = @"4df15f2b-e566-43ce-830d-671bd0786737";
		public static readonly Guid  RDOArtifactTypeIDGuid = Guid.Parse(RDOArtifactTypeID);
		public const string CreateSavedSearchInDestination = @"bfab4af6-4704-4a12-a8ca-c96a1fbcb77d";
		public static readonly Guid  CreateSavedSearchInDestinationGuid = Guid.Parse(CreateSavedSearchInDestination);
		public const string DataDestinationArtifactID = @"0e9d7b8e-4643-41cc-9b07-3a66c98248a1";
		public static readonly Guid  DataDestinationArtifactIDGuid = Guid.Parse(DataDestinationArtifactID);
		public const string DataDestinationType = @"86d9a34a-b394-41cf-bff4-bd4ff49a932d";
		public static readonly Guid  DataDestinationTypeGuid = Guid.Parse(DataDestinationType);
		public const string ImportOverwriteMode = @"1914d2a3-a1ff-480b-81dc-7a2aa563047a";
		public static readonly Guid  ImportOverwriteModeGuid = Guid.Parse(ImportOverwriteMode);
		public const string NativesBehavior = @"d18f0199-7096-4b0c-ab37-4c9a3ea1d3d2";
		public static readonly Guid  NativesBehaviorGuid = Guid.Parse(NativesBehavior);
		public const string DestinationFolderStructureBehavior = @"a1593105-bd99-4a15-a51a-3aa8d4195908";
		public static readonly Guid  DestinationFolderStructureBehaviorGuid = Guid.Parse(DestinationFolderStructureBehavior);
		public const string MoveExistingDocuments = @"26f9bf88-420d-4eff-914b-c47ba36e10bf";
		public static readonly Guid  MoveExistingDocumentsGuid = Guid.Parse(MoveExistingDocuments);
		public const string FieldOverlayBehavior = @"34ecb263-1370-4d6c-ac11-558447504ec4";
		public static readonly Guid  FieldOverlayBehaviorGuid = Guid.Parse(FieldOverlayBehavior);
		public const string SnapshotID = @"d1210a1b-c461-46cb-9b73-9d22d05880c5";
		public static readonly Guid  SnapshotIDGuid = Guid.Parse(SnapshotID);
		public const string SnapshotRecordsCount = @"57b93f20-2648-4acf-973b-bcba8a08e2bd";
		public static readonly Guid  SnapshotRecordsCountGuid = Guid.Parse(SnapshotRecordsCount);
		public const string SavedSearchInDestinationArtifactID = @"83f4dd7a-2231-4c54-baaa-d1d5b0fe6e31";
		public static readonly Guid  SavedSearchInDestinationArtifactIDGuid = Guid.Parse(SavedSearchInDestinationArtifactID);
		public const string SourceJobTagArtifactID = @"c0a63a29-abae-4bf4-a3f4-59e5bd87a33e";
		public static readonly Guid  SourceJobTagArtifactIDGuid = Guid.Parse(SourceJobTagArtifactID);
		public const string SourceJobTagName = @"da0e1931-9460-4a61-9033-a8035697c1a4";
		public static readonly Guid  SourceJobTagNameGuid = Guid.Parse(SourceJobTagName);
		public const string SourceWorkspaceTagArtifactID = @"feab129b-aeef-4aa4-bc91-9eae9a4c35f6";
		public static readonly Guid  SourceWorkspaceTagArtifactIDGuid = Guid.Parse(SourceWorkspaceTagArtifactID);
		public const string SourceWorkspaceTagName = @"d828b69e-aaae-4639-91e2-416e35c163b1";
		public static readonly Guid  SourceWorkspaceTagNameGuid = Guid.Parse(SourceWorkspaceTagName);
		public const string DestinationWorkspaceTagArtifactID = @"e2100c10-b53b-43fa-bb1b-51e43dce8208";
		public static readonly Guid  DestinationWorkspaceTagArtifactIDGuid = Guid.Parse(DestinationWorkspaceTagArtifactID);
		public const string FolderPathSourceFieldArtifactID = @"bf5f07a3-6349-47ee-9618-1dd32c9fd998";
		public static readonly Guid  FolderPathSourceFieldArtifactIDGuid = Guid.Parse(FolderPathSourceFieldArtifactID);
		public const string Name = @"ef8bbd3d-79b0-47fa-9fc9-8faa535788b0";
		public static readonly Guid  NameGuid = Guid.Parse(Name);
	}



	public partial class SyncBatchFields : BaseFields
	{
		public const string SyncConfiguration = @"SyncConfiguration";
		public const string FailedItemsCount = @"Failed items count";
		public const string TransferredItemsCount = @"Transferred items count";
		public const string TotalItemsCount = @"Total items count";
		public const string StartingIndex = @"Starting index";
		public const string LockedBy = @"Locked by";
		public const string Progress = @"Progress";
		public const string Status = @"Status";
		public const string Name = @"Name";
	}

	public partial class SyncBatchFieldGuids 
	{
		public const string SyncConfiguration = @"f673e67f-e606-4155-8e15-ca1c83931e16";
		public static readonly Guid  SyncConfigurationGuid = Guid.Parse(SyncConfiguration);
		public const string FailedItemsCount = @"dc3228e4-2765-4c3b-b3b1-a0f054e280f6";
		public static readonly Guid  FailedItemsCountGuid = Guid.Parse(FailedItemsCount);
		public const string TransferredItemsCount = @"b2d112ca-e81e-42c7-a6b2-c0e89f32f567";
		public static readonly Guid  TransferredItemsCountGuid = Guid.Parse(TransferredItemsCount);
		public const string TotalItemsCount = @"f84589fe-a583-4eb3-ba8a-4a2eee085c81";
		public static readonly Guid  TotalItemsCountGuid = Guid.Parse(TotalItemsCount);
		public const string StartingIndex = @"b56f4f70-ceb3-49b8-bc2b-662d481ddc8a";
		public static readonly Guid  StartingIndexGuid = Guid.Parse(StartingIndex);
		public const string LockedBy = @"befc75d3-5825-4479-b499-58c6ef719ddb";
		public static readonly Guid  LockedByGuid = Guid.Parse(LockedBy);
		public const string Progress = @"8c6daf67-9428-4f5f-98d7-3c71a1ff3ae8";
		public static readonly Guid  ProgressGuid = Guid.Parse(Progress);
		public const string Status = @"d16faf24-bc87-486c-a0ab-6354f36af38e";
		public static readonly Guid  StatusGuid = Guid.Parse(Status);
		public const string Name = @"3ab49704-f843-4e09-aff2-5380b1bf7a35";
		public static readonly Guid  NameGuid = Guid.Parse(Name);
	}



	public partial class SyncProgressFields : BaseFields
	{
		public const string SyncConfiguration = @"SyncConfiguration";
		public const string Exception = @"Exception";
		public const string Message = @"Message";
		public const string Order = @"Order";
		public const string Status = @"Status";
		public const string Name = @"Name";
	}

	public partial class SyncProgressFieldGuids 
	{
		public const string SyncConfiguration = @"e0188dd7-4b1b-454d-afa4-3ccc7f9dc001";
		public static readonly Guid  SyncConfigurationGuid = Guid.Parse(SyncConfiguration);
		public const string Exception = @"2f2cfc2b-c9c0-406d-bd90-fb0133bcb939";
		public static readonly Guid  ExceptionGuid = Guid.Parse(Exception);
		public const string Message = @"2e296f79-1b81-4bf6-98ad-68da13f8da44";
		public static readonly Guid  MessageGuid = Guid.Parse(Message);
		public const string Order = @"610a1e44-7aaa-47fc-8fa0-92f8c8c8a94a";
		public static readonly Guid  OrderGuid = Guid.Parse(Order);
		public const string Status = @"698e1bbe-13b7-445c-8a28-7d40fd232e1b";
		public static readonly Guid  StatusGuid = Guid.Parse(Status);
		public const string Name = @"ae2fca2b-0e5c-4f35-948f-6c1654d5cf95";
		public static readonly Guid  NameGuid = Guid.Parse(Name);
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
		public static Choice JobHistoryValidating = new Choice(Guid.Parse("6a2dcef5-5826-4f61-9bac-59fef879ebc2")) {Name=@"Validating"};
		public static Choice JobHistoryValidationFailed = new Choice(Guid.Parse("d0b43a57-bdc8-4c14-b2f0-2928ae4f750a")) {Name=@"Validation Failed"};
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
		public static readonly Guid  IntegrationPointDetailsGuid = Guid.Parse(IntegrationPointDetails);
	}

	public partial class IntegrationPointLayouts
	{
		public const string IntegrationPointDetails = @"Integration Point Details";
	}

	public partial class SourceProviderLayoutGuids
	{
		public const string SourceProviderLayout = @"6d2ecb5d-ec2d-4b4b-b631-47fada8af8d4";
		public static readonly Guid  SourceProviderLayoutGuid = Guid.Parse(SourceProviderLayout);
	}

	public partial class SourceProviderLayouts
	{
		public const string SourceProviderLayout = @"Source Provider Layout";
	}

	public partial class DestinationProviderLayoutGuids
	{
		public const string DestinationProviderLayout = @"806a3f21-3171-4093-afe6-b7a53cd2c4b5";
		public static readonly Guid  DestinationProviderLayoutGuid = Guid.Parse(DestinationProviderLayout);
	}

	public partial class DestinationProviderLayouts
	{
		public const string DestinationProviderLayout = @"DestinationProvider Layout";
	}

	public partial class JobHistoryLayoutGuids
	{
		public const string JobDetails = @"04081530-f66e-4f56-bf07-7b325b53ffa9";
		public static readonly Guid  JobDetailsGuid = Guid.Parse(JobDetails);
	}

	public partial class JobHistoryLayouts
	{
		public const string JobDetails = @"Job Details";
	}

	public partial class JobHistoryErrorLayoutGuids
	{
		public const string JobHistoryErrorLayout = @"52f405af-9903-47a8-a00b-f1359b548526";
		public static readonly Guid  JobHistoryErrorLayoutGuid = Guid.Parse(JobHistoryErrorLayout);
	}

	public partial class JobHistoryErrorLayouts
	{
		public const string JobHistoryErrorLayout = @"Job History Error Layout";
	}

	public partial class DestinationWorkspaceLayoutGuids
	{
		public const string DestinationWorkspaceLayout = @"ed0da23b-191d-40b3-8171-fcdf57342436";
		public static readonly Guid  DestinationWorkspaceLayoutGuid = Guid.Parse(DestinationWorkspaceLayout);
	}

	public partial class DestinationWorkspaceLayouts
	{
		public const string DestinationWorkspaceLayout = @"Destination Workspace Layout";
	}

	public partial class IntegrationPointTypeLayoutGuids
	{
		public const string IntegrationPointTypeLayout = @"64197e82-591d-4b2c-a971-b760b11e8307";
		public static readonly Guid  IntegrationPointTypeLayoutGuid = Guid.Parse(IntegrationPointTypeLayout);
	}

	public partial class IntegrationPointTypeLayouts
	{
		public const string IntegrationPointTypeLayout = @"Integration Point Type Layout";
	}

	public partial class IntegrationPointProfileLayoutGuids
	{
		public const string IntegrationPointProfileLayout = @"f8505b51-802b-466f-952b-2e0eb7aadb2f";
		public static readonly Guid  IntegrationPointProfileLayoutGuid = Guid.Parse(IntegrationPointProfileLayout);
	}

	public partial class IntegrationPointProfileLayouts
	{
		public const string IntegrationPointProfileLayout = @"Integration Point Profile Layout";
	}

	public partial class SyncConfigurationLayoutGuids
	{
		public const string SyncConfigurationLayout = @"052be7a9-8772-4c38-a07a-7801ee7b3623";
		public static readonly Guid  SyncConfigurationLayoutGuid = Guid.Parse(SyncConfigurationLayout);
	}

	public partial class SyncConfigurationLayouts
	{
		public const string SyncConfigurationLayout = @"Sync Configuration Layout";
	}

	public partial class SyncBatchLayoutGuids
	{
		public const string SyncBatchLayout = @"b6eb6e0d-c2c2-4aea-979e-49bd4bb8a8ae";
		public static readonly Guid  SyncBatchLayoutGuid = Guid.Parse(SyncBatchLayout);
	}

	public partial class SyncBatchLayouts
	{
		public const string SyncBatchLayout = @"Sync Batch Layout";
	}

	public partial class SyncProgressLayoutGuids
	{
		public const string SyncProgressLayout = @"ecf783a2-635d-455c-9a7f-2d1d30e2a751";
		public static readonly Guid  SyncProgressLayoutGuid = Guid.Parse(SyncProgressLayout);
	}

	public partial class SyncProgressLayouts
	{
		public const string SyncProgressLayout = @"Sync Progress Layout";
	}

	#endregion
	
	
	#region "Tabs"

	public partial class IntegrationPointTabGuids
	{
		public const string IntegrationPoints = @"4a9d9b4c-cec1-4c68-8a6d-c0f22516f032";
		public static readonly Guid  IntegrationPointsGuid = Guid.Parse(IntegrationPoints);
	}

	public partial class IntegrationPointTabs
	{
		public const string IntegrationPoints = @"Integration Points";
	}

	public partial class JobHistoryTabGuids
	{
		public const string JobHistory = @"677d1324-f251-4cb5-80c1-fb079f05962a";
		public static readonly Guid  JobHistoryGuid = Guid.Parse(JobHistory);
	}

	public partial class JobHistoryTabs
	{
		public const string JobHistory = @"Job History";
	}

	public partial class JobHistoryErrorTabGuids
	{
		public const string JobHistoryErrors = @"fd585dbf-98ea-427b-8ce5-3e09a053dc14";
		public static readonly Guid  JobHistoryErrorsGuid = Guid.Parse(JobHistoryErrors);
	}

	public partial class JobHistoryErrorTabs
	{
		public const string JobHistoryErrors = @"Job History Errors";
	}

	public partial class DestinationWorkspaceTabGuids
	{
		public const string DestinationWorkspaces = @"d04fc871-7a46-47a2-bbe5-83318b2d641e";
		public static readonly Guid  DestinationWorkspacesGuid = Guid.Parse(DestinationWorkspaces);
	}

	public partial class DestinationWorkspaceTabs
	{
		public const string DestinationWorkspaces = @"Destination Workspaces";
	}

	public partial class IntegrationPointProfileTabGuids
	{
		public const string IntegrationPointProfile = @"0ac27fca-a6fe-425b-87f0-afb12e40563a";
		public static readonly Guid  IntegrationPointProfileGuid = Guid.Parse(IntegrationPointProfile);
	}

	public partial class IntegrationPointProfileTabs
	{
		public const string IntegrationPointProfile = @"Integration Point Profile";
	}

	#endregion
	
	#region "Views"

	public partial class DocumentViewGuids
	{
		public const string DocumentsDestinationWorkspace = @"c18d034d-e911-4401-a370-827aa303bdd8";
		public static readonly Guid  DocumentsDestinationWorkspaceGuid = Guid.Parse(DocumentsDestinationWorkspace);
	}

	public partial class DocumentViews
	{
		public const string DocumentsDestinationWorkspace = @"Documents (Destination Workspace)";
	}

	public partial class IntegrationPointViewGuids
	{
		public const string AllIntegrationPoints = @"181bf82a-e0dc-4a95-955a-0630bccb6afa";
		public static readonly Guid  AllIntegrationPointsGuid = Guid.Parse(AllIntegrationPoints);
	}

	public partial class IntegrationPointViews
	{
		public const string AllIntegrationPoints = @"All Integration Points";
	}

	public partial class SourceProviderViewGuids
	{
		public const string AllSourceProviders = @"f4e2c372-da19-4bb2-9c46-a1d6fa037136";
		public static readonly Guid  AllSourceProvidersGuid = Guid.Parse(AllSourceProviders);
	}

	public partial class SourceProviderViews
	{
		public const string AllSourceProviders = @"All Source Providers";
	}

	public partial class DestinationProviderViewGuids
	{
		public const string AllDestinationProviders = @"602c03fd-3694-4547-ab39-598a95a957d2";
		public static readonly Guid  AllDestinationProvidersGuid = Guid.Parse(AllDestinationProviders);
	}

	public partial class DestinationProviderViews
	{
		public const string AllDestinationProviders = @"All DestinationProviders";
	}

	public partial class JobHistoryViewGuids
	{
		public const string AllJobs = @"067aad47-7092-4782-aedf-157f5f1c7f4c";
		public static readonly Guid  AllJobsGuid = Guid.Parse(AllJobs);
		public const string AllJobsWithErrors = @"2d438c4d-afeb-4662-bfcd-935f4094751c";
		public static readonly Guid  AllJobsWithErrorsGuid = Guid.Parse(AllJobsWithErrors);
	}

	public partial class JobHistoryViews
	{
		public const string AllJobs = @"All Jobs";
		public const string AllJobsWithErrors = @"All Jobs with Errors";
	}

	public partial class JobHistoryErrorViewGuids
	{
		public const string AllJobHistoryErrors = @"c0f2e9de-74f5-44b1-b3bf-335ac4ed50e0";
		public static readonly Guid  AllJobHistoryErrorsGuid = Guid.Parse(AllJobHistoryErrors);
	}

	public partial class JobHistoryErrorViews
	{
		public const string AllJobHistoryErrors = @"All Job History Errors";
	}

	public partial class DestinationWorkspaceViewGuids
	{
		public const string AllDestinationWorkspaces = @"a74341a4-08c7-4671-883f-59bdd12780a7";
		public static readonly Guid  AllDestinationWorkspacesGuid = Guid.Parse(AllDestinationWorkspaces);
	}

	public partial class DestinationWorkspaceViews
	{
		public const string AllDestinationWorkspaces = @"All Destination Workspaces";
	}

	public partial class IntegrationPointTypeViewGuids
	{
		public const string AllIntegrationPointTypes = @"57fd219a-a2e4-4110-8956-af6c026041ee";
		public static readonly Guid  AllIntegrationPointTypesGuid = Guid.Parse(AllIntegrationPointTypes);
	}

	public partial class IntegrationPointTypeViews
	{
		public const string AllIntegrationPointTypes = @"All Integration Point Types";
	}

	public partial class IntegrationPointProfileViewGuids
	{
		public const string AllProfiles = @"2b9d50cb-6d0e-46f3-a4c5-110001526704";
		public static readonly Guid  AllProfilesGuid = Guid.Parse(AllProfiles);
	}

	public partial class IntegrationPointProfileViews
	{
		public const string AllProfiles = @"All Profiles";
	}

	public partial class SyncConfigurationViewGuids
	{
		public const string AllSyncConfigurations = @"d4e5c466-dbb2-4049-9249-f19a51c7abf9";
		public static readonly Guid  AllSyncConfigurationsGuid = Guid.Parse(AllSyncConfigurations);
	}

	public partial class SyncConfigurationViews
	{
		public const string AllSyncConfigurations = @"All Sync Configurations";
	}

	public partial class SyncBatchViewGuids
	{
		public const string AllSyncBatchs = @"e0eca48d-969a-48b4-980f-7502d057111c";
		public static readonly Guid  AllSyncBatchsGuid = Guid.Parse(AllSyncBatchs);
	}

	public partial class SyncBatchViews
	{
		public const string AllSyncBatchs = @"All Sync Batchs";
	}

	public partial class SyncProgressViewGuids
	{
		public const string AllSyncProgress = @"0081034e-9226-45cc-8037-f35b01b2c85e";
		public static readonly Guid  AllSyncProgressGuid = Guid.Parse(AllSyncProgress);
	}

	public partial class SyncProgressViews
	{
		public const string AllSyncProgress = @"All Sync Progress";
	}

	#endregion									

}

