using System;
using System.Collections.Generic;
using Relativity.Services.Choice;

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
        internal const string Document = @"15c36703-74ea-4ff8-9dfb-ad30ece7530d";

        public static readonly Guid DocumentGuid = Guid.Parse(Document);
        internal const string IntegrationPoint = @"03d4f67e-22c9-488c-bee6-411f05c52e01";

        public static readonly Guid IntegrationPointGuid = Guid.Parse(IntegrationPoint);
        internal const string SourceProvider = @"5be4a1f7-87a8-4cbe-a53f-5027d4f70b80";

        public static readonly Guid SourceProviderGuid = Guid.Parse(SourceProvider);
        internal const string DestinationProvider = @"d014f00d-f2c0-4e7a-b335-84fcb6eae980";

        public static readonly Guid DestinationProviderGuid = Guid.Parse(DestinationProvider);
        internal const string JobHistory = @"08f4b1f7-9692-4a08-94ab-b5f3a88b6cc9";

        public static readonly Guid JobHistoryGuid = Guid.Parse(JobHistory);
        internal const string JobHistoryError = @"17e7912d-4f57-4890-9a37-abc2b8a37bdb";

        public static readonly Guid JobHistoryErrorGuid = Guid.Parse(JobHistoryError);
        internal const string DestinationWorkspace = @"3f45e490-b4cf-4c7d-8bb6-9ca891c0c198";

        public static readonly Guid DestinationWorkspaceGuid = Guid.Parse(DestinationWorkspace);
        internal const string IntegrationPointType = @"3c4acdf0-41c8-4888-a374-383c25a734cb";

        public static readonly Guid IntegrationPointTypeGuid = Guid.Parse(IntegrationPointType);
        internal const string IntegrationPointProfile = @"6dc915a9-25d7-4500-97f7-07cb98a06f64";

        public static readonly Guid IntegrationPointProfileGuid = Guid.Parse(IntegrationPointProfile);
        internal const string SyncConfiguration = @"3be3de56-839f-4f0e-8446-e1691ed5fd57";

        public static readonly Guid SyncConfigurationGuid = Guid.Parse(SyncConfiguration);
        internal const string SyncBatch = @"18c766eb-eb71-49e4-983e-ffde29b1a44e";

        public static readonly Guid SyncBatchGuid = Guid.Parse(SyncBatch);
        internal const string SyncProgress = @"3d107450-db18-4fe1-8219-73ee1f921ed9";

        public static readonly Guid SyncProgressGuid = Guid.Parse(SyncProgress);
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
        internal const string MarkupSetPrimary = @"14292546-9c9c-4210-998b-4b5ff4d89e58";

        public static readonly Guid MarkupSetPrimaryGuid = Guid.Parse(MarkupSetPrimary);
        internal const string Batch = @"d7a9d9fd-68fc-4c85-ad44-ba524a0ca872";

        public static readonly Guid BatchGuid = Guid.Parse(Batch);
        internal const string BatchBatchSet = @"705dad6c-843d-4131-b470-f65874366fa7";

        public static readonly Guid BatchBatchSetGuid = Guid.Parse(BatchBatchSet);
        internal const string BatchAssignedTo = @"e8de43da-fbdf-4319-8ee6-e865dbf70bdb";

        public static readonly Guid BatchAssignedToGuid = Guid.Parse(BatchAssignedTo);
        internal const string BatchStatus = @"2d8b37df-02ac-4a66-834a-3bfc2de78486";

        public static readonly Guid BatchStatusGuid = Guid.Parse(BatchStatus);
        internal const string RelativityDestinationCase = @"8980c2fa-0d33-4686-9a97-ea9d6f0b4196";

        public static readonly Guid RelativityDestinationCaseGuid = Guid.Parse(RelativityDestinationCase);
        internal const string JobHistory = @"97bc12fa-509b-4c75-8413-6889387d8ef6";

        public static readonly Guid JobHistoryGuid = Guid.Parse(JobHistory);
        internal const string ControlNumber = @"2a3f1212-c8ca-4fa9-ad6b-f76c97f05438";

        public static readonly Guid ControlNumberGuid = Guid.Parse(ControlNumber);
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
        public const string CalculationState = @"Calculation State";
    }

    public partial class IntegrationPointFieldGuids
    {
        internal const string NextScheduledRuntimeUTC = @"5b1c9986-f166-40e4-a0dd-a56f185ff30b";

        public static readonly Guid NextScheduledRuntimeUTCGuid = Guid.Parse(NextScheduledRuntimeUTC);
        internal const string LastRuntimeUTC = @"90d58af1-f79f-40ae-85fc-7e42f84dbcc1";

        public static readonly Guid LastRuntimeUTCGuid = Guid.Parse(LastRuntimeUTC);
        internal const string FieldMappings = @"1b065787-a6e4-4d70-a7ed-f49d770f0bc7";

        public static readonly Guid FieldMappingsGuid = Guid.Parse(FieldMappings);
        internal const string EnableScheduler = @"bcdafc41-311e-4b66-8084-4a8e0f56ca00";

        public static readonly Guid EnableSchedulerGuid = Guid.Parse(EnableScheduler);
        internal const string SourceConfiguration = @"b5000e91-82bd-475a-86e9-32fefc04f4b8";

        public static readonly Guid SourceConfigurationGuid = Guid.Parse(SourceConfiguration);
        internal const string DestinationConfiguration = @"b1323ca7-34e5-4e6b-8ff1-e8d3b1a5fd0a";

        public static readonly Guid DestinationConfigurationGuid = Guid.Parse(DestinationConfiguration);
        internal const string SourceProvider = @"dc902551-2c9c-4f41-a917-41f4a3ef7409";

        public static readonly Guid SourceProviderGuid = Guid.Parse(SourceProvider);
        internal const string ScheduleRule = @"000f25ef-d714-4671-8075-d2a71cac396b";

        public static readonly Guid ScheduleRuleGuid = Guid.Parse(ScheduleRule);
        internal const string OverwriteFields = @"0cae01d8-0dc3-4852-9359-fb954215c36f";

        public static readonly Guid OverwriteFieldsGuid = Guid.Parse(OverwriteFields);
        internal const string DestinationProvider = @"d6f4384a-0d2c-4eee-aab8-033cc77155ee";

        public static readonly Guid DestinationProviderGuid = Guid.Parse(DestinationProvider);
        internal const string JobHistory = @"14b230cf-a505-4dd3-b05c-c54d05e62966";

        public static readonly Guid JobHistoryGuid = Guid.Parse(JobHistory);
        internal const string LogErrors = @"0319869e-37aa-499c-a95b-6d8d0e96a711";

        public static readonly Guid LogErrorsGuid = Guid.Parse(LogErrors);
        internal const string EmailNotificationRecipients = @"1bac59db-f7bf-48e0-91d4-18cf09ff0e39";

        public static readonly Guid EmailNotificationRecipientsGuid = Guid.Parse(EmailNotificationRecipients);
        internal const string HasErrors = @"a9853e55-0ba0-43d8-a766-747a61471981";

        public static readonly Guid HasErrorsGuid = Guid.Parse(HasErrors);
        internal const string Type = @"e646016e-5df6-4440-b218-18a00926d002";

        public static readonly Guid TypeGuid = Guid.Parse(Type);
        internal const string SecuredConfiguration = @"48b0a4cb-bc21-45b5-b124-76ae27e03c42";

        public static readonly Guid SecuredConfigurationGuid = Guid.Parse(SecuredConfiguration);
        internal const string PromoteEligible = @"bf85f332-8c8f-4c69-86fd-6ce4c567ebf9";

        public static readonly Guid PromoteEligibleGuid = Guid.Parse(PromoteEligible);
        internal const string Name = @"d534f433-dd92-4a53-b12d-bf85472e6d7a";

        public static readonly Guid NameGuid = Guid.Parse(Name);
        internal const string CalculationState = @"e64163ea-a58c-4a90-af30-dd190069210d";

        public static readonly Guid CalculationStateGuid = Guid.Parse(CalculationState);
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
        internal const string Identifier = @"d0ecc6c9-472c-4296-83e1-0906f0c0fbb9";

        public static readonly Guid IdentifierGuid = Guid.Parse(Identifier);
        internal const string SourceConfigurationUrl = @"b1b34def-3e77-48c3-97d4-eae7b5ee2213";

        public static readonly Guid SourceConfigurationUrlGuid = Guid.Parse(SourceConfigurationUrl);
        internal const string ApplicationIdentifier = @"0e696f9e-0e14-40f9-8cd7-34195defe5de";

        public static readonly Guid ApplicationIdentifierGuid = Guid.Parse(ApplicationIdentifier);
        internal const string ViewConfigurationUrl = @"bb036af8-1309-4f66-98f3-3495285b4a4b";

        public static readonly Guid ViewConfigurationUrlGuid = Guid.Parse(ViewConfigurationUrl);
        internal const string Configuration = @"a85e3e30-e56a-4ddb-9282-fc37dc5e70d3";

        public static readonly Guid ConfigurationGuid = Guid.Parse(Configuration);
        internal const string Name = @"9073997b-319e-482f-92fe-67e0b5860c1b";

        public static readonly Guid NameGuid = Guid.Parse(Name);
    }

    public partial class DestinationProviderFields : BaseFields
    {
        public const string Identifier = @"Identifier";
        public const string ApplicationIdentifier = @"Application Identifier";
        public const string Name = @"Name";
    }

    public partial class DestinationProviderFieldGuids
    {
        internal const string Identifier = @"9fa104ac-13ea-4868-b716-17d6d786c77a";

        public static readonly Guid IdentifierGuid = Guid.Parse(Identifier);
        internal const string ApplicationIdentifier = @"92892e25-0927-4073-b03d-e6a94ff80450";

        public static readonly Guid ApplicationIdentifierGuid = Guid.Parse(ApplicationIdentifier);
        internal const string Name = @"3ed18f54-c75a-4879-92a8-5ae23142bbeb";

        public static readonly Guid NameGuid = Guid.Parse(Name);
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
        public const string ItemsRead = @"Items Read";
    }

    public partial class JobHistoryFieldGuids
    {
        internal const string Documents = @"5d99f717-3b5e-4773-9f51-9ca5d4c1a0fc";

        public static readonly Guid DocumentsGuid = Guid.Parse(Documents);
        internal const string IntegrationPoint = @"d3e791d3-2e21-45f4-b403-e7196bd25eea";

        public static readonly Guid IntegrationPointGuid = Guid.Parse(IntegrationPoint);
        internal const string JobStatus = @"5c28ce93-c62f-4d25-98c9-9a330a6feb52";

        public static readonly Guid JobStatusGuid = Guid.Parse(JobStatus);
        internal const string ItemsTransferred = @"70680399-c8ea-4b12-b711-e9ecbc53cb1c";

        public static readonly Guid ItemsTransferredGuid = Guid.Parse(ItemsTransferred);
        internal const string ItemsWithErrors = @"c224104f-c1ca-4caa-9189-657e01d5504e";

        public static readonly Guid ItemsWithErrorsGuid = Guid.Parse(ItemsWithErrors);
        internal const string StartTimeUTC = @"25b7c8ef-66d9-41d1-a8de-29a93e47fb11";

        public static readonly Guid StartTimeUTCGuid = Guid.Parse(StartTimeUTC);
        internal const string EndTimeUTC = @"4736cf49-ad0f-4f02-aaaa-898e07400f22";

        public static readonly Guid EndTimeUTCGuid = Guid.Parse(EndTimeUTC);
        internal const string BatchInstance = @"08ba2c77-a9cd-4faf-a77a-be35e1ef1517";

        public static readonly Guid BatchInstanceGuid = Guid.Parse(BatchInstance);
        internal const string DestinationWorkspace = @"ff01a766-b494-4f2c-9cbb-10a5ab163b8d";

        public static readonly Guid DestinationWorkspaceGuid = Guid.Parse(DestinationWorkspace);
        internal const string TotalItems = @"576189a9-0347-4b20-9369-b16d1ac89b4b";

        public static readonly Guid TotalItemsGuid = Guid.Parse(TotalItems);
        internal const string DestinationWorkspaceInformation = @"20a24c4e-55e8-4fc2-abbe-f75c07fad91b";

        public static readonly Guid DestinationWorkspaceInformationGuid = Guid.Parse(DestinationWorkspaceInformation);
        internal const string JobType = @"e809db5e-5e99-4a75-98a1-26129313a3f5";

        public static readonly Guid JobTypeGuid = Guid.Parse(JobType);
        internal const string DestinationInstance = @"6d91ea1e-7b34-46a9-854e-2b018d4e35ef";

        public static readonly Guid DestinationInstanceGuid = Guid.Parse(DestinationInstance);
        internal const string FilesSize = @"d81817dc-91cb-44c4-b9b7-7c445da64f5a";

        public static readonly Guid FilesSizeGuid = Guid.Parse(FilesSize);
        internal const string Overwrite = @"42d49f5e-b0e7-4632-8d30-1c6ee1d97fa7";

        public static readonly Guid OverwriteGuid = Guid.Parse(Overwrite);
        internal const string JobID = @"77d797ef-96c9-4b47-9ef8-33f498b5af0d";

        public static readonly Guid JobIDGuid = Guid.Parse(JobID);
        internal const string Name = @"07061466-5fab-4581-979c-c801e8207370";

        public static readonly Guid NameGuid = Guid.Parse(Name);
        internal const string ItemsRead = @"2b76010a-9cf8-4276-9d6d-504d026f0b27";

        public static readonly Guid ItemsReadGuid = Guid.Parse(ItemsRead);
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
        internal const string JobHistory = @"8b747b91-0627-4130-8e53-2931ffc4135f";

        public static readonly Guid JobHistoryGuid = Guid.Parse(JobHistory);
        internal const string SourceUniqueID = @"5519435e-ee82-4820-9546-f1af46121901";

        public static readonly Guid SourceUniqueIDGuid = Guid.Parse(SourceUniqueID);
        internal const string Error = @"4112b894-35b0-4e53-ab99-c9036d08269d";

        public static readonly Guid ErrorGuid = Guid.Parse(Error);
        internal const string TimestampUTC = @"b9cba772-e7c9-493e-b7f8-8d605a6bfe1f";

        public static readonly Guid TimestampUTCGuid = Guid.Parse(TimestampUTC);
        internal const string ErrorType = @"eeffa5d3-82e3-46f8-9762-b4053d73f973";

        public static readonly Guid ErrorTypeGuid = Guid.Parse(ErrorType);
        internal const string StackTrace = @"0353dbde-9e00-4227-8a8f-4380a8891cff";

        public static readonly Guid StackTraceGuid = Guid.Parse(StackTrace);
        internal const string ErrorStatus = @"de1a46d2-d615-427a-b9f2-c10769bc2678";

        public static readonly Guid ErrorStatusGuid = Guid.Parse(ErrorStatus);
        internal const string Name = @"84e757cc-9da2-435d-b288-0c21ec589e66";

        public static readonly Guid NameGuid = Guid.Parse(Name);
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
        internal const string Documents = @"94ee2bd7-76d5-4d17-99e2-04768cce05e6";

        public static readonly Guid DocumentsGuid = Guid.Parse(Documents);
        internal const string JobHistory = @"07b8a468-dec8-45bd-b50a-989a35150be2";

        public static readonly Guid JobHistoryGuid = Guid.Parse(JobHistory);
        internal const string DestinationWorkspaceArtifactID = @"207e6836-2961-466b-a0d2-29974a4fad36";

        public static readonly Guid DestinationWorkspaceArtifactIDGuid = Guid.Parse(DestinationWorkspaceArtifactID);
        internal const string DestinationWorkspaceName = @"348d7394-2658-4da4-87d0-8183824adf98";

        public static readonly Guid DestinationWorkspaceNameGuid = Guid.Parse(DestinationWorkspaceName);
        internal const string DestinationInstanceName = @"909adc7c-2bb9-46ca-9f85-da32901d6554";

        public static readonly Guid DestinationInstanceNameGuid = Guid.Parse(DestinationInstanceName);
        internal const string DestinationInstanceArtifactID = @"323458db-8a06-464b-9402-af2516cf47e0";

        public static readonly Guid DestinationInstanceArtifactIDGuid = Guid.Parse(DestinationInstanceArtifactID);
        internal const string Name = @"155649c0-db15-4ee7-b449-bfdf2a54b7b5";

        public static readonly Guid NameGuid = Guid.Parse(Name);
    }

    public partial class IntegrationPointTypeFields : BaseFields
    {
        public const string ApplicationIdentifier = @"Application Identifier";
        public const string Identifier = @"Identifier";
        public const string Name = @"Name";
    }

    public partial class IntegrationPointTypeFieldGuids
    {
        internal const string ApplicationIdentifier = @"9720e543-cce0-445c-8af7-042355671a71";

        public static readonly Guid ApplicationIdentifierGuid = Guid.Parse(ApplicationIdentifier);
        internal const string Identifier = @"3bd675a0-555d-49bc-b108-e2d04afcc1e3";

        public static readonly Guid IdentifierGuid = Guid.Parse(Identifier);
        internal const string Name = @"ae4fe868-5428-49fd-b2e4-bb17abd597ef";

        public static readonly Guid NameGuid = Guid.Parse(Name);
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
        public const string CalculationState = @"Calculation State";
    }

    public partial class IntegrationPointProfileFieldGuids
    {
        internal const string DestinationConfiguration = @"5d9e425a-b59c-4119-9ceb-73665a5e7049";

        public static readonly Guid DestinationConfigurationGuid = Guid.Parse(DestinationConfiguration);
        internal const string DestinationProvider = @"7d9e7944-bf13-4c4f-a9eb-5f60e683ec0c";

        public static readonly Guid DestinationProviderGuid = Guid.Parse(DestinationProvider);
        internal const string EmailNotificationRecipients = @"b72f5bb9-2e07-45f0-903a-b20d3a17958c";

        public static readonly Guid EmailNotificationRecipientsGuid = Guid.Parse(EmailNotificationRecipients);
        internal const string EnableScheduler = @"bc2e19fd-c95c-4f1c-b4a9-1692590cef8e";

        public static readonly Guid EnableSchedulerGuid = Guid.Parse(EnableScheduler);
        internal const string FieldMappings = @"8ae37734-29d1-4441-b5d8-483134f98818";

        public static readonly Guid FieldMappingsGuid = Guid.Parse(FieldMappings);
        internal const string LogErrors = @"b582f002-00fe-4c44-b721-859dd011d4fd";

        public static readonly Guid LogErrorsGuid = Guid.Parse(LogErrors);
        internal const string NextScheduledRuntimeUTC = @"a3c48572-4ec7-4e06-b57f-1c1681cd07d1";

        public static readonly Guid NextScheduledRuntimeUTCGuid = Guid.Parse(NextScheduledRuntimeUTC);
        internal const string OverwriteFields = @"e0a14a2c-0bb6-47ad-a34f-26400258a761";

        public static readonly Guid OverwriteFieldsGuid = Guid.Parse(OverwriteFields);
        internal const string ScheduleRule = @"35b6c8b8-5b0c-4660-bfdd-226e424edeb5";

        public static readonly Guid ScheduleRuleGuid = Guid.Parse(ScheduleRule);
        internal const string SourceConfiguration = @"9ed96d44-7767-46f5-a67f-28b48b155ff2";

        public static readonly Guid SourceConfigurationGuid = Guid.Parse(SourceConfiguration);
        internal const string SourceProvider = @"60d3de54-f0d5-4744-a23f-a17609edc537";

        public static readonly Guid SourceProviderGuid = Guid.Parse(SourceProvider);
        internal const string Type = @"8999dd19-c67c-43e3-88c0-edc989e224cc";

        public static readonly Guid TypeGuid = Guid.Parse(Type);
        internal const string PromoteEligible = @"4997cdb4-d8b0-4eaa-8768-061d82aaaccf";

        public static readonly Guid PromoteEligibleGuid = Guid.Parse(PromoteEligible);
        internal const string Name = @"ad0552e0-4511-4507-a60d-23c0c6d05972";

        public static readonly Guid NameGuid = Guid.Parse(Name);
        internal const string CalculationState = @"89a16eb5-b493-43f0-b09c-ce1a4f4b3786";

        public static readonly Guid CalculationStateGuid = Guid.Parse(CalculationState);
    }

    #endregion

    #region "ChoiceRef Constants"

    public partial class OverwriteFieldsChoices
    {
        public static Guid IntegrationPointAppendOnlyGuid = Guid.Parse("998c2b04-d42e-435b-9fba-11fec836aad8");

        public static ChoiceRef IntegrationPointAppendOnly = new ChoiceRef() {Name=@"Append Only", Guids = new List<Guid>(){ IntegrationPointAppendOnlyGuid } };

        public static Guid IntegrationPointAppendOverlayGuid = Guid.Parse("5450ebc3-ac57-4e6a-9d28-d607bbdcf6fd");

        public static ChoiceRef IntegrationPointAppendOverlay = new ChoiceRef() {Name=@"Append/Overlay", Guids = new List<Guid>() { IntegrationPointAppendOverlayGuid } };

        public static Guid IntegrationPointOverlayOnlyGuid = Guid.Parse("70a1052d-93a3-4b72-9235-ac65f0d5a515");

        public static ChoiceRef IntegrationPointOverlayOnly = new ChoiceRef() {Name=@"Overlay Only", Guids = new List<Guid>(){ IntegrationPointOverlayOnlyGuid } };
    }

    public partial class JobStatusChoices
    {
        public static Guid JobHistoryValidatingGuid = Guid.Parse("6a2dcef5-5826-4f61-9bac-59fef879ebc2");

        public static ChoiceRef JobHistoryValidating = new ChoiceRef() {Name=@"Validating", Guids = new List<Guid>(){ JobHistoryValidatingGuid } };

        public static Guid JobHistoryValidationFailedGuid = Guid.Parse("d0b43a57-bdc8-4c14-b2f0-2928ae4f750a");

        public static ChoiceRef JobHistoryValidationFailed = new ChoiceRef() {Name=@"Validation Failed", Guids = new List<Guid>(){ JobHistoryValidationFailedGuid } };

        public static Guid JobHistoryPendingGuid = Guid.Parse("24512aba-b8aa-4858-9324-5799033d7e96");

        public static ChoiceRef JobHistoryPending = new ChoiceRef() {Name=@"Pending", Guids = new List<Guid>() { JobHistoryPendingGuid } };

        public static Guid JobHistoryProcessingGuid = Guid.Parse("bb170e53-2264-4708-9b00-86156187ed54");

        public static ChoiceRef JobHistoryProcessing = new ChoiceRef() {Name=@"Processing", Guids = new List<Guid>() { JobHistoryProcessingGuid } };

        public static Guid JobHistoryCompletedGuid = Guid.Parse("c7d1eb34-166e-48d0-bce7-0be0df43511c");

        public static ChoiceRef JobHistoryCompleted = new ChoiceRef() {Name=@"Completed", Guids = new List<Guid>() { JobHistoryCompletedGuid } };

        public static Guid JobHistoryCompletedWithErrorsGuid = Guid.Parse("c0f4a2b2-499e-45bc-96d7-f8bc25e18b37");

        public static ChoiceRef JobHistoryCompletedWithErrors = new ChoiceRef() {Name=@"Completed with errors", Guids = new List<Guid>() { JobHistoryCompletedWithErrorsGuid } };

        public static Guid JobHistoryErrorJobFailedGuid = Guid.Parse("3152ece9-40e6-44dd-afc8-1004f55dfb63");

        public static ChoiceRef JobHistoryErrorJobFailed = new ChoiceRef() {Name=@"Error - job failed", Guids = new List<Guid>() { JobHistoryErrorJobFailedGuid } };

        public static Guid JobHistoryStoppingGuid = Guid.Parse("97c1410d-864d-4811-857b-952464872baa");

        public static ChoiceRef JobHistoryStopping = new ChoiceRef() {Name=@"Stopping", Guids = new List<Guid>() { JobHistoryStoppingGuid } };

        public static Guid JobHistoryStoppedGuid = Guid.Parse("a29c5bcb-d3a6-4f81-877a-2a6556c996c3");

        public static ChoiceRef JobHistoryStopped = new ChoiceRef() {Name=@"Stopped", Guids = new List<Guid>() { JobHistoryStoppedGuid } };

        public static Guid JobHistorySuspendingGuid = Guid.Parse("c65658c3-79ea-4762-b78e-85d9f38785b6");

        public static ChoiceRef JobHistorySuspending = new ChoiceRef() {Name = "Suspending", Guids = new List<Guid>() {JobHistorySuspendingGuid}};

        public static Guid JobHistorySuspendedGuid = Guid.Parse("f219e060-d7e1-4666-964d-f229a1a13baa");

        public static ChoiceRef JobHistorySuspended = new ChoiceRef() {Name = "Suspended", Guids = new List<Guid>() {JobHistorySuspendedGuid}};
    }

    public partial class JobTypeChoices
    {
        public static Guid JobHistoryRunGuid = Guid.Parse("86c8c17d-74ec-4187-bdb1-9380252f4c20");

        public static ChoiceRef JobHistoryRun = new ChoiceRef() {Name=@"Run", Guids = new List<Guid>() { JobHistoryRunGuid } };

        public static Guid JobHistoryScheduledRunGuid = Guid.Parse("79510ad3-49cb-4b4f-840c-c64247404a4d");

        public static ChoiceRef JobHistoryScheduledRun = new ChoiceRef() {Name=@"Scheduled Run", Guids = new List<Guid>() { JobHistoryScheduledRunGuid } };

        public static Guid JobHistoryRetryErrorsGuid = Guid.Parse("b0171a20-2042-44eb-a957-5dbc9c377c2f");

        public static ChoiceRef JobHistoryRetryErrors = new ChoiceRef() {Name=@"Retry Errors", Guids = new List<Guid>() { JobHistoryRetryErrorsGuid } };
    }

    public partial class ErrorTypeChoices
    {
        public static Guid JobHistoryErrorItemGuid = Guid.Parse("9ddc4914-fef3-401f-89b7-2967cd76714b");

        public static ChoiceRef JobHistoryErrorItem = new ChoiceRef() {Name=@"Item", Guids = new List<Guid>() { JobHistoryErrorItemGuid } };

        public static Guid JobHistoryErrorJobGuid = Guid.Parse("fa8bb625-05e6-4bf7-8573-012146baf19b");

        public static ChoiceRef JobHistoryErrorJob = new ChoiceRef() {Name=@"Job", Guids = new List<Guid>() { JobHistoryErrorJobGuid } };
    }

    public partial class ErrorStatusChoices
    {
        public static Guid JobHistoryErrorNewGuid = Guid.Parse("f881b199-8a67-4d49-b1c1-f9e68658fb5a");

        public static ChoiceRef JobHistoryErrorNew = new ChoiceRef() {Name=@"New", Guids = new List<Guid>() { JobHistoryErrorNewGuid } };

        public static Guid JobHistoryErrorExpiredGuid = Guid.Parse("af01a8fa-b419-49b1-bd71-25296e221e57");

        public static ChoiceRef JobHistoryErrorExpired = new ChoiceRef() {Name=@"Expired", Guids = new List<Guid>() { JobHistoryErrorExpiredGuid } };

        public static Guid JobHistoryErrorInProgressGuid = Guid.Parse("e5ebd98c-c976-4fa2-936f-434e265ea0aa");

        public static ChoiceRef JobHistoryErrorInProgress = new ChoiceRef() {Name=@"In Progress", Guids = new List<Guid>() { JobHistoryErrorInProgressGuid } };

        public static Guid JobHistoryErrorRetriedGuid = Guid.Parse("7d3d393d-384f-434e-9776-f9966550d29a");

        public static ChoiceRef JobHistoryErrorRetried = new ChoiceRef() {Name=@"Retried", Guids = new List<Guid>() { JobHistoryErrorRetriedGuid } };
    }

    public partial class OverwriteFieldsChoices
    {
        public static Guid IntegrationPointProfileAppendOnlyGuid = Guid.Parse("12105945-5fb8-4640-8516-11a96f12279c");

        public static ChoiceRef IntegrationPointProfileAppendOnly = new ChoiceRef() {Name=@"Append Only", Guids = new List<Guid>() { IntegrationPointProfileAppendOnlyGuid } };

        public static Guid IntegrationPointProfileAppendOverlayGuid = Guid.Parse("e5c80435-d876-4cba-b645-658a545eaea1");

        public static ChoiceRef IntegrationPointProfileAppendOverlay = new ChoiceRef() {Name=@"Append/Overlay", Guids = new List<Guid>() { IntegrationPointProfileAppendOverlayGuid } };

        public static Guid IntegrationPointProfileOverlayOnlyGuid = Guid.Parse("e08fc9e5-416c-4656-a9a1-6323013160fb");

        public static ChoiceRef IntegrationPointProfileOverlayOnly = new ChoiceRef() {Name=@"Overlay Only", Guids = new List<Guid>() { IntegrationPointProfileOverlayOnlyGuid } };
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
        internal const string IntegrationPointDetails = @"f4a9ed1f-d874-4b07-b127-043e8ad0d506";

        public static readonly Guid IntegrationPointDetailsGuid = Guid.Parse(IntegrationPointDetails);
    }

    public partial class IntegrationPointLayouts
    {
        public const string IntegrationPointDetails = @"Integration Point Details";
    }

    public partial class SourceProviderLayoutGuids
    {
        internal const string SourceProviderLayout = @"6d2ecb5d-ec2d-4b4b-b631-47fada8af8d4";

        public static readonly Guid SourceProviderLayoutGuid = Guid.Parse(SourceProviderLayout);
    }

    public partial class SourceProviderLayouts
    {
        public const string SourceProviderLayout = @"Source Provider Layout";
    }

    public partial class DestinationProviderLayoutGuids
    {
        internal const string DestinationProviderLayout = @"806a3f21-3171-4093-afe6-b7a53cd2c4b5";

        public static readonly Guid DestinationProviderLayoutGuid = Guid.Parse(DestinationProviderLayout);
    }

    public partial class DestinationProviderLayouts
    {
        public const string DestinationProviderLayout = @"DestinationProvider Layout";
    }

    public partial class JobHistoryLayoutGuids
    {
        internal const string JobDetails = @"04081530-f66e-4f56-bf07-7b325b53ffa9";

        public static readonly Guid JobDetailsGuid = Guid.Parse(JobDetails);
    }

    public partial class JobHistoryLayouts
    {
        public const string JobDetails = @"Job Details";
    }

    public partial class JobHistoryErrorLayoutGuids
    {
        internal const string JobHistoryErrorLayout = @"52f405af-9903-47a8-a00b-f1359b548526";

        public static readonly Guid JobHistoryErrorLayoutGuid = Guid.Parse(JobHistoryErrorLayout);
    }

    public partial class JobHistoryErrorLayouts
    {
        public const string JobHistoryErrorLayout = @"Job History Error Layout";
    }

    public partial class DestinationWorkspaceLayoutGuids
    {
        internal const string DestinationWorkspaceLayout = @"ed0da23b-191d-40b3-8171-fcdf57342436";

        public static readonly Guid DestinationWorkspaceLayoutGuid = Guid.Parse(DestinationWorkspaceLayout);
    }

    public partial class DestinationWorkspaceLayouts
    {
        public const string DestinationWorkspaceLayout = @"Destination Workspace Layout";
    }

    public partial class IntegrationPointTypeLayoutGuids
    {
        internal const string IntegrationPointTypeLayout = @"64197e82-591d-4b2c-a971-b760b11e8307";

        public static readonly Guid IntegrationPointTypeLayoutGuid = Guid.Parse(IntegrationPointTypeLayout);
    }

    public partial class IntegrationPointTypeLayouts
    {
        public const string IntegrationPointTypeLayout = @"Integration Point Type Layout";
    }

    public partial class IntegrationPointProfileLayoutGuids
    {
        internal const string IntegrationPointProfileLayout = @"f8505b51-802b-466f-952b-2e0eb7aadb2f";

        public static readonly Guid IntegrationPointProfileLayoutGuid = Guid.Parse(IntegrationPointProfileLayout);
    }

    public partial class IntegrationPointProfileLayouts
    {
        public const string IntegrationPointProfileLayout = @"Integration Point Profile Layout";
    }

    public partial class SyncConfigurationLayoutGuids
    {
        internal const string SyncConfigurationLayout = @"052be7a9-8772-4c38-a07a-7801ee7b3623";

        public static readonly Guid SyncConfigurationLayoutGuid = Guid.Parse(SyncConfigurationLayout);
    }

    public partial class SyncConfigurationLayouts
    {
        public const string SyncConfigurationLayout = @"Sync Configuration Layout";
    }

    public partial class SyncBatchLayoutGuids
    {
        internal const string SyncBatchLayout = @"b6eb6e0d-c2c2-4aea-979e-49bd4bb8a8ae";

        public static readonly Guid SyncBatchLayoutGuid = Guid.Parse(SyncBatchLayout);
    }

    public partial class SyncBatchLayouts
    {
        public const string SyncBatchLayout = @"Sync Batch Layout";
    }

    public partial class SyncProgressLayoutGuids
    {
        internal const string SyncProgressLayout = @"ecf783a2-635d-455c-9a7f-2d1d30e2a751";

        public static readonly Guid SyncProgressLayoutGuid = Guid.Parse(SyncProgressLayout);
    }

    public partial class SyncProgressLayouts
    {
        public const string SyncProgressLayout = @"Sync Progress Layout";
    }

    #endregion

    #region "Tabs"

    public partial class IntegrationPointTabGuids
    {
        internal const string IntegrationPoints = @"4a9d9b4c-cec1-4c68-8a6d-c0f22516f032";

        public static readonly Guid IntegrationPointsGuid = Guid.Parse(IntegrationPoints);
    }

    public partial class IntegrationPointTabs
    {
        public const string IntegrationPoints = @"Integration Points";
    }

    public partial class JobHistoryTabGuids
    {
        internal const string JobHistory = @"677d1324-f251-4cb5-80c1-fb079f05962a";

        public static readonly Guid JobHistoryGuid = Guid.Parse(JobHistory);
    }

    public partial class JobHistoryTabs
    {
        public const string JobHistory = @"Job History";
    }

    public partial class JobHistoryErrorTabGuids
    {
        internal const string JobHistoryErrors = @"fd585dbf-98ea-427b-8ce5-3e09a053dc14";

        public static readonly Guid JobHistoryErrorsGuid = Guid.Parse(JobHistoryErrors);
    }

    public partial class JobHistoryErrorTabs
    {
        public const string JobHistoryErrors = @"Job History Errors";
    }

    public partial class DestinationWorkspaceTabGuids
    {
        internal const string DestinationWorkspaces = @"d04fc871-7a46-47a2-bbe5-83318b2d641e";

        public static readonly Guid DestinationWorkspacesGuid = Guid.Parse(DestinationWorkspaces);
    }

    public partial class DestinationWorkspaceTabs
    {
        public const string DestinationWorkspaces = @"Destination Workspaces";
    }

    public partial class IntegrationPointProfileTabGuids
    {
        internal const string IntegrationPointProfile = @"0ac27fca-a6fe-425b-87f0-afb12e40563a";

        public static readonly Guid IntegrationPointProfileGuid = Guid.Parse(IntegrationPointProfile);
    }

    public partial class IntegrationPointProfileTabs
    {
        public const string IntegrationPointProfile = @"Integration Point Profile";
    }

    #endregion

    #region "Views"

    public partial class DocumentViewGuids
    {
        internal const string DocumentsDestinationWorkspace = @"c18d034d-e911-4401-a370-827aa303bdd8";

        public static readonly Guid DocumentsDestinationWorkspaceGuid = Guid.Parse(DocumentsDestinationWorkspace);
    }

    public partial class DocumentViews
    {
        public const string DocumentsDestinationWorkspace = @"Documents (Destination Workspace)";
    }

    public partial class IntegrationPointViewGuids
    {
        internal const string AllIntegrationPoints = @"181bf82a-e0dc-4a95-955a-0630bccb6afa";

        public static readonly Guid AllIntegrationPointsGuid = Guid.Parse(AllIntegrationPoints);
    }

    public partial class IntegrationPointViews
    {
        public const string AllIntegrationPoints = @"All Integration Points";
    }

    public partial class SourceProviderViewGuids
    {
        internal const string AllSourceProviders = @"f4e2c372-da19-4bb2-9c46-a1d6fa037136";

        public static readonly Guid AllSourceProvidersGuid = Guid.Parse(AllSourceProviders);
    }

    public partial class SourceProviderViews
    {
        public const string AllSourceProviders = @"All Source Providers";
    }

    public partial class DestinationProviderViewGuids
    {
        internal const string AllDestinationProviders = @"602c03fd-3694-4547-ab39-598a95a957d2";

        public static readonly Guid AllDestinationProvidersGuid = Guid.Parse(AllDestinationProviders);
    }

    public partial class DestinationProviderViews
    {
        public const string AllDestinationProviders = @"All DestinationProviders";
    }

    public partial class JobHistoryViewGuids
    {
        internal const string AllJobs = @"067aad47-7092-4782-aedf-157f5f1c7f4c";

        public static readonly Guid AllJobsGuid = Guid.Parse(AllJobs);
        internal const string AllJobsWithErrors = @"2d438c4d-afeb-4662-bfcd-935f4094751c";

        public static readonly Guid AllJobsWithErrorsGuid = Guid.Parse(AllJobsWithErrors);
    }

    public partial class JobHistoryViews
    {
        public const string AllJobs = @"All Jobs";
        public const string AllJobsWithErrors = @"All Jobs with Errors";
    }

    public partial class JobHistoryErrorViewGuids
    {
        internal const string AllJobHistoryErrors = @"c0f2e9de-74f5-44b1-b3bf-335ac4ed50e0";

        public static readonly Guid AllJobHistoryErrorsGuid = Guid.Parse(AllJobHistoryErrors);
    }

    public partial class JobHistoryErrorViews
    {
        public const string AllJobHistoryErrors = @"All Job History Errors";
    }

    public partial class DestinationWorkspaceViewGuids
    {
        internal const string AllDestinationWorkspaces = @"a74341a4-08c7-4671-883f-59bdd12780a7";

        public static readonly Guid AllDestinationWorkspacesGuid = Guid.Parse(AllDestinationWorkspaces);
    }

    public partial class DestinationWorkspaceViews
    {
        public const string AllDestinationWorkspaces = @"All Destination Workspaces";
    }

    public partial class IntegrationPointTypeViewGuids
    {
        internal const string AllIntegrationPointTypes = @"57fd219a-a2e4-4110-8956-af6c026041ee";

        public static readonly Guid AllIntegrationPointTypesGuid = Guid.Parse(AllIntegrationPointTypes);
    }

    public partial class IntegrationPointTypeViews
    {
        public const string AllIntegrationPointTypes = @"All Integration Point Types";
    }

    public partial class IntegrationPointProfileViewGuids
    {
        internal const string AllProfiles = @"2b9d50cb-6d0e-46f3-a4c5-110001526704";

        public static readonly Guid AllProfilesGuid = Guid.Parse(AllProfiles);
    }

    public partial class IntegrationPointProfileViews
    {
        public const string AllProfiles = @"All Profiles";
    }

    public partial class SyncConfigurationViewGuids
    {
        internal const string AllSyncConfigurations = @"d4e5c466-dbb2-4049-9249-f19a51c7abf9";

        public static readonly Guid AllSyncConfigurationsGuid = Guid.Parse(AllSyncConfigurations);
    }

    public partial class SyncConfigurationViews
    {
        public const string AllSyncConfigurations = @"All Sync Configurations";
    }

    public partial class SyncBatchViewGuids
    {
        internal const string AllSyncBatchs = @"e0eca48d-969a-48b4-980f-7502d057111c";

        public static readonly Guid AllSyncBatchsGuid = Guid.Parse(AllSyncBatchs);
    }

    public partial class SyncBatchViews
    {
        public const string AllSyncBatchs = @"All Sync Batchs";
    }

    public partial class SyncProgressViewGuids
    {
        internal const string AllSyncProgress = @"0081034e-9226-45cc-8037-f35b01b2c85e";

        public static readonly Guid AllSyncProgressGuid = Guid.Parse(AllSyncProgress);
    }

    public partial class SyncProgressViews
    {
        public const string AllSyncProgress = @"All Sync Progress";
    }

    #endregion

}
