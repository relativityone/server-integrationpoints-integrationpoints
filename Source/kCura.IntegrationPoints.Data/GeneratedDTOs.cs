using System;
using kCura.IntegrationPoints.Data.Attributes;
using Relativity.Services.Choice;
using Relativity.Services.User;

namespace kCura.IntegrationPoints.Data
{
    [DynamicObject(ObjectTypes.Document, ObjectTypes.Folder, "", ObjectTypeGuids.Document)]
    public partial class Document : BaseRdo
    {
        [DynamicField(DocumentFields.RelativityDestinationCase, DocumentFieldGuids.RelativityDestinationCase, FieldTypes.MultipleObject)]
        public int[] RelativityDestinationCase
        {
            get
            {
                return GetField<int[]>(new System.Guid(DocumentFieldGuids.RelativityDestinationCase));
            }

            set
            {
                SetField<int[]>(new System.Guid(DocumentFieldGuids.RelativityDestinationCase), value);
            }
        }

        [DynamicField(DocumentFields.JobHistory, DocumentFieldGuids.JobHistory, FieldTypes.MultipleObject)]
        public int[] JobHistory
        {
            get
            {
                return GetField<int[]>(new System.Guid(DocumentFieldGuids.JobHistory));
            }

            set
            {
                SetField<int[]>(new System.Guid(DocumentFieldGuids.JobHistory), value);
            }
        }

        public const int ControlNumberFieldLength = 255;

        [DynamicField(DocumentFields.ControlNumber, DocumentFieldGuids.ControlNumber, FieldTypes.FixedLengthText, 255)]
        public string ControlNumber
        {
            get
            {
                return GetField<string>(new System.Guid(DocumentFieldGuids.ControlNumber));
            }

            set
            {
                SetField<string>(new System.Guid(DocumentFieldGuids.ControlNumber), value);
            }
        }

        [DynamicField(DocumentFields.MarkupSetPrimary, DocumentFieldGuids.MarkupSetPrimary, FieldTypes.MultipleChoice)]
        public ChoiceRef[] MarkupSetPrimary
        {
            get
            {
                return GetField<ChoiceRef[]>(new System.Guid(DocumentFieldGuids.MarkupSetPrimary));
            }

            set
            {
                SetField<ChoiceRef[]>(new System.Guid(DocumentFieldGuids.MarkupSetPrimary), value);
            }
        }

        [DynamicField(DocumentFields.Batch, DocumentFieldGuids.Batch, FieldTypes.MultipleObject)]
        public int[] Batch
        {
            get
            {
                return GetField<int[]>(new System.Guid(DocumentFieldGuids.Batch));
            }

            set
            {
                SetField<int[]>(new System.Guid(DocumentFieldGuids.Batch), value);
            }
        }

        [DynamicField(DocumentFields.BatchBatchSet, DocumentFieldGuids.BatchBatchSet, FieldTypes.SingleObject)]
        public int? BatchBatchSet
        {
            get
            {
                return GetField<int?>(new System.Guid(DocumentFieldGuids.BatchBatchSet));
            }

            set
            {
                SetField<int?>(new System.Guid(DocumentFieldGuids.BatchBatchSet), value);
            }
        }

        [DynamicField(DocumentFields.BatchAssignedTo, DocumentFieldGuids.BatchAssignedTo, FieldTypes.User)]
        public User BatchAssignedTo
        {
            get
            {
                return GetField<User>(new System.Guid(DocumentFieldGuids.BatchAssignedTo));
            }

            set
            {
                SetField<User>(new System.Guid(DocumentFieldGuids.BatchAssignedTo), value);
            }
        }

        [DynamicField(DocumentFields.BatchStatus, DocumentFieldGuids.BatchStatus, FieldTypes.SingleChoice)]
        public ChoiceRef BatchStatus
        {
            get
            {
                return GetField<ChoiceRef>(new System.Guid(DocumentFieldGuids.BatchStatus));
            }

            set
            {
                SetField<ChoiceRef>(new System.Guid(DocumentFieldGuids.BatchStatus), value);
            }
        }

        private static System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> _fieldMetadata;

        public override System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> FieldMetadata
        {
            get
            {
                if (!(_fieldMetadata == null))
                    return _fieldMetadata;
                _fieldMetadata = GetFieldMetadata(typeof(Document));
                return _fieldMetadata;
            }
        }
    }

    [DynamicObject(ObjectTypes.IntegrationPoint, ObjectTypes.Workspace, "", ObjectTypeGuids.IntegrationPoint)]
    public partial class IntegrationPoint : BaseRdo
    {
        [DynamicField(IntegrationPointFields.NextScheduledRuntimeUTC, IntegrationPointFieldGuids.NextScheduledRuntimeUTC, FieldTypes.Date)]
        public DateTime? NextScheduledRuntimeUTC
        {
            get
            {
                return GetField<DateTime?>(new System.Guid(IntegrationPointFieldGuids.NextScheduledRuntimeUTC));
            }

            set
            {
                SetField<DateTime?>(new System.Guid(IntegrationPointFieldGuids.NextScheduledRuntimeUTC), value);
            }
        }

        [DynamicField(IntegrationPointFields.LastRuntimeUTC, IntegrationPointFieldGuids.LastRuntimeUTC, FieldTypes.Date)]
        public DateTime? LastRuntimeUTC
        {
            get
            {
                return GetField<DateTime?>(new System.Guid(IntegrationPointFieldGuids.LastRuntimeUTC));
            }

            set
            {
                SetField<DateTime?>(new System.Guid(IntegrationPointFieldGuids.LastRuntimeUTC), value);
            }
        }

        [DynamicField(IntegrationPointFields.FieldMappings, IntegrationPointFieldGuids.FieldMappings, FieldTypes.LongText)]
        public string FieldMappings
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointFieldGuids.FieldMappings));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointFieldGuids.FieldMappings), value);
            }
        }

        [DynamicField(IntegrationPointFields.EnableScheduler, IntegrationPointFieldGuids.EnableScheduler, FieldTypes.YesNo)]
        public bool? EnableScheduler
        {
            get
            {
                return GetField<bool?>(new System.Guid(IntegrationPointFieldGuids.EnableScheduler));
            }

            set
            {
                SetField<bool?>(new System.Guid(IntegrationPointFieldGuids.EnableScheduler), value);
            }
        }

        [DynamicField(IntegrationPointFields.SourceConfiguration, IntegrationPointFieldGuids.SourceConfiguration, FieldTypes.LongText)]
        public string SourceConfiguration
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointFieldGuids.SourceConfiguration));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointFieldGuids.SourceConfiguration), value);
            }
        }

        [DynamicField(IntegrationPointFields.DestinationConfiguration, IntegrationPointFieldGuids.DestinationConfiguration, FieldTypes.LongText)]
        public string DestinationConfiguration
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointFieldGuids.DestinationConfiguration));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointFieldGuids.DestinationConfiguration), value);
            }
        }

        [DynamicField(IntegrationPointFields.SourceProvider, IntegrationPointFieldGuids.SourceProvider, FieldTypes.SingleObject)]
        public int? SourceProvider
        {
            get
            {
                return GetField<int?>(new System.Guid(IntegrationPointFieldGuids.SourceProvider));
            }

            set
            {
                SetField<int?>(new System.Guid(IntegrationPointFieldGuids.SourceProvider), value);
            }
        }

        [DynamicField(IntegrationPointFields.ScheduleRule, IntegrationPointFieldGuids.ScheduleRule, FieldTypes.LongText)]
        public string ScheduleRule
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointFieldGuids.ScheduleRule));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointFieldGuids.ScheduleRule), value);
            }
        }

        [DynamicField(IntegrationPointFields.OverwriteFields, IntegrationPointFieldGuids.OverwriteFields, FieldTypes.SingleChoice)]
        public ChoiceRef OverwriteFields
        {
            get
            {
                return GetField<ChoiceRef>(new System.Guid(IntegrationPointFieldGuids.OverwriteFields));
            }

            set
            {
                SetField<ChoiceRef>(new System.Guid(IntegrationPointFieldGuids.OverwriteFields), value);
            }
        }

        [DynamicField(IntegrationPointFields.DestinationProvider, IntegrationPointFieldGuids.DestinationProvider, FieldTypes.SingleObject)]
        public int? DestinationProvider
        {
            get
            {
                return GetField<int?>(new System.Guid(IntegrationPointFieldGuids.DestinationProvider));
            }

            set
            {
                SetField<int?>(new System.Guid(IntegrationPointFieldGuids.DestinationProvider), value);
            }
        }

        [DynamicField(IntegrationPointFields.JobHistory, IntegrationPointFieldGuids.JobHistory, FieldTypes.MultipleObject)]
        public int[] JobHistory
        {
            get
            {
                return GetField<int[]>(new System.Guid(IntegrationPointFieldGuids.JobHistory));
            }

            set
            {
                SetField<int[]>(new System.Guid(IntegrationPointFieldGuids.JobHistory), value);
            }
        }

        [DynamicField(IntegrationPointFields.LogErrors, IntegrationPointFieldGuids.LogErrors, FieldTypes.YesNo)]
        public bool? LogErrors
        {
            get
            {
                return GetField<bool?>(new System.Guid(IntegrationPointFieldGuids.LogErrors));
            }

            set
            {
                SetField<bool?>(new System.Guid(IntegrationPointFieldGuids.LogErrors), value);
            }
        }

        [DynamicField(IntegrationPointFields.EmailNotificationRecipients, IntegrationPointFieldGuids.EmailNotificationRecipients, FieldTypes.LongText)]
        public string EmailNotificationRecipients
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointFieldGuids.EmailNotificationRecipients));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointFieldGuids.EmailNotificationRecipients), value);
            }
        }

        [DynamicField(IntegrationPointFields.HasErrors, IntegrationPointFieldGuids.HasErrors, FieldTypes.YesNo)]
        public bool? HasErrors
        {
            get
            {
                return GetField<bool?>(new System.Guid(IntegrationPointFieldGuids.HasErrors));
            }

            set
            {
                SetField<bool?>(new System.Guid(IntegrationPointFieldGuids.HasErrors), value);
            }
        }

        [DynamicField(IntegrationPointFields.Type, IntegrationPointFieldGuids.Type, FieldTypes.SingleObject)]
        public int? Type
        {
            get
            {
                return GetField<int?>(new System.Guid(IntegrationPointFieldGuids.Type));
            }

            set
            {
                SetField<int?>(new System.Guid(IntegrationPointFieldGuids.Type), value);
            }
        }

        [DynamicField(IntegrationPointFields.SecuredConfiguration, IntegrationPointFieldGuids.SecuredConfiguration, FieldTypes.LongText)]
        public string SecuredConfiguration
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointFieldGuids.SecuredConfiguration));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointFieldGuids.SecuredConfiguration), value);
            }
        }

        [DynamicField(IntegrationPointFields.PromoteEligible, IntegrationPointFieldGuids.PromoteEligible, FieldTypes.YesNo)]
        public bool? PromoteEligible
        {
            get
            {
                return GetField<bool?>(new System.Guid(IntegrationPointFieldGuids.PromoteEligible));
            }

            set
            {
                SetField<bool?>(new System.Guid(IntegrationPointFieldGuids.PromoteEligible), value);
            }
        }

        public const int NameFieldLength = 255;

        [DynamicField(IntegrationPointFields.Name, IntegrationPointFieldGuids.Name, FieldTypes.FixedLengthText, 255)]
        public string Name
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointFieldGuids.Name));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointFieldGuids.Name), value);
            }
        }

        [DynamicField(IntegrationPointFields.CalculationState, IntegrationPointFieldGuids.CalculationState, FieldTypes.LongText)]
        public string CalculationState
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointFieldGuids.CalculationState));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointFieldGuids.CalculationState), value);
            }
        }

        private static System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> _fieldMetadata;

        public override System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> FieldMetadata
        {
            get
            {
                if (!(_fieldMetadata == null))
                    return _fieldMetadata;
                _fieldMetadata = GetFieldMetadata(typeof(IntegrationPoint));
                return _fieldMetadata;
            }
        }
    }

    [DynamicObject(ObjectTypes.SourceProvider, ObjectTypes.Workspace, "", ObjectTypeGuids.SourceProvider)]
    public partial class SourceProvider : BaseRdo
    {
        public const int IdentifierFieldLength = 40;

        [DynamicField(SourceProviderFields.Identifier, SourceProviderFieldGuids.Identifier, FieldTypes.FixedLengthText, 40)]
        public string Identifier
        {
            get
            {
                return GetField<string>(new System.Guid(SourceProviderFieldGuids.Identifier));
            }

            set
            {
                SetField<string>(new System.Guid(SourceProviderFieldGuids.Identifier), value);
            }
        }

        [DynamicField(SourceProviderFields.SourceConfigurationUrl, SourceProviderFieldGuids.SourceConfigurationUrl, FieldTypes.LongText)]
        public string SourceConfigurationUrl
        {
            get
            {
                return GetField<string>(new System.Guid(SourceProviderFieldGuids.SourceConfigurationUrl));
            }

            set
            {
                SetField<string>(new System.Guid(SourceProviderFieldGuids.SourceConfigurationUrl), value);
            }
        }

        public const int ApplicationIdentifierFieldLength = 50;

        [DynamicField(SourceProviderFields.ApplicationIdentifier, SourceProviderFieldGuids.ApplicationIdentifier, FieldTypes.FixedLengthText, 50)]
        public string ApplicationIdentifier
        {
            get
            {
                return GetField<string>(new System.Guid(SourceProviderFieldGuids.ApplicationIdentifier));
            }

            set
            {
                SetField<string>(new System.Guid(SourceProviderFieldGuids.ApplicationIdentifier), value);
            }
        }

        [DynamicField(SourceProviderFields.ViewConfigurationUrl, SourceProviderFieldGuids.ViewConfigurationUrl, FieldTypes.LongText)]
        public string ViewConfigurationUrl
        {
            get
            {
                return GetField<string>(new System.Guid(SourceProviderFieldGuids.ViewConfigurationUrl));
            }

            set
            {
                SetField<string>(new System.Guid(SourceProviderFieldGuids.ViewConfigurationUrl), value);
            }
        }

        [DynamicField(SourceProviderFields.Configuration, SourceProviderFieldGuids.Configuration, FieldTypes.LongText)]
        public string Configuration
        {
            get
            {
                return GetField<string>(new System.Guid(SourceProviderFieldGuids.Configuration));
            }

            set
            {
                SetField<string>(new System.Guid(SourceProviderFieldGuids.Configuration), value);
            }
        }

        public const int NameFieldLength = 255;

        [DynamicField(SourceProviderFields.Name, SourceProviderFieldGuids.Name, FieldTypes.FixedLengthText, 255)]
        public string Name
        {
            get
            {
                return GetField<string>(new System.Guid(SourceProviderFieldGuids.Name));
            }

            set
            {
                SetField<string>(new System.Guid(SourceProviderFieldGuids.Name), value);
            }
        }

        private static System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> _fieldMetadata;

        public override System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> FieldMetadata
        {
            get
            {
                if (!(_fieldMetadata == null))
                    return _fieldMetadata;
                _fieldMetadata = GetFieldMetadata(typeof(SourceProvider));
                return _fieldMetadata;
            }
        }
    }

    [DynamicObject(ObjectTypes.DestinationProvider, ObjectTypes.Workspace, "", ObjectTypeGuids.DestinationProvider)]
    public partial class DestinationProvider : BaseRdo
    {
        public const int IdentifierFieldLength = 40;

        [DynamicField(DestinationProviderFields.Identifier, DestinationProviderFieldGuids.Identifier, FieldTypes.FixedLengthText, 40)]
        public string Identifier
        {
            get
            {
                return GetField<string>(new System.Guid(DestinationProviderFieldGuids.Identifier));
            }

            set
            {
                SetField<string>(new System.Guid(DestinationProviderFieldGuids.Identifier), value);
            }
        }

        public const int ApplicationIdentifierFieldLength = 50;

        [DynamicField(DestinationProviderFields.ApplicationIdentifier, DestinationProviderFieldGuids.ApplicationIdentifier, FieldTypes.FixedLengthText, 50)]
        public string ApplicationIdentifier
        {
            get
            {
                return GetField<string>(new System.Guid(DestinationProviderFieldGuids.ApplicationIdentifier));
            }

            set
            {
                SetField<string>(new System.Guid(DestinationProviderFieldGuids.ApplicationIdentifier), value);
            }
        }

        public const int NameFieldLength = 255;

        [DynamicField(DestinationProviderFields.Name, DestinationProviderFieldGuids.Name, FieldTypes.FixedLengthText, 255)]
        public string Name
        {
            get
            {
                return GetField<string>(new System.Guid(DestinationProviderFieldGuids.Name));
            }

            set
            {
                SetField<string>(new System.Guid(DestinationProviderFieldGuids.Name), value);
            }
        }

        private static System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> _fieldMetadata;

        public override System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> FieldMetadata
        {
            get
            {
                if (!(_fieldMetadata == null))
                    return _fieldMetadata;
                _fieldMetadata = GetFieldMetadata(typeof(DestinationProvider));
                return _fieldMetadata;
            }
        }
    }

    [DynamicObject(ObjectTypes.JobHistory, ObjectTypes.Workspace, "", ObjectTypeGuids.JobHistory)]
    public partial class JobHistory : BaseRdo
    {
        [DynamicField(JobHistoryFields.Documents, JobHistoryFieldGuids.Documents, FieldTypes.MultipleObject)]
        public int[] Documents
        {
            get
            {
                return GetField<int[]>(new System.Guid(JobHistoryFieldGuids.Documents));
            }

            set
            {
                SetField<int[]>(new System.Guid(JobHistoryFieldGuids.Documents), value);
            }
        }

        [DynamicField(JobHistoryFields.IntegrationPoint, JobHistoryFieldGuids.IntegrationPoint, FieldTypes.MultipleObject)]
        public int[] IntegrationPoint
        {
            get
            {
                return GetField<int[]>(new System.Guid(JobHistoryFieldGuids.IntegrationPoint));
            }

            set
            {
                SetField<int[]>(new System.Guid(JobHistoryFieldGuids.IntegrationPoint), value);
            }
        }

        [DynamicField(JobHistoryFields.JobStatus, JobHistoryFieldGuids.JobStatus, FieldTypes.SingleChoice)]
        public ChoiceRef JobStatus
        {
            get
            {
                return GetField<ChoiceRef>(new System.Guid(JobHistoryFieldGuids.JobStatus));
            }

            set
            {
                SetField<ChoiceRef>(new System.Guid(JobHistoryFieldGuids.JobStatus), value);
            }
        }

        [DynamicField(JobHistoryFields.ItemsTransferred, JobHistoryFieldGuids.ItemsTransferred, FieldTypes.WholeNumber)]
        public int? ItemsTransferred
        {
            get
            {
                return GetField<int?>(new System.Guid(JobHistoryFieldGuids.ItemsTransferred));
            }

            set
            {
                SetField<int?>(new System.Guid(JobHistoryFieldGuids.ItemsTransferred), value);
            }
        }

        [DynamicField(JobHistoryFields.ItemsWithErrors, JobHistoryFieldGuids.ItemsWithErrors, FieldTypes.WholeNumber)]
        public int? ItemsWithErrors
        {
            get
            {
                return GetField<int?>(new System.Guid(JobHistoryFieldGuids.ItemsWithErrors));
            }

            set
            {
                SetField<int?>(new System.Guid(JobHistoryFieldGuids.ItemsWithErrors), value);
            }
        }

        [DynamicField(JobHistoryFields.StartTimeUTC, JobHistoryFieldGuids.StartTimeUTC, FieldTypes.Date)]
        public DateTime? StartTimeUTC
        {
            get
            {
                return GetField<DateTime?>(new System.Guid(JobHistoryFieldGuids.StartTimeUTC));
            }

            set
            {
                SetField<DateTime?>(new System.Guid(JobHistoryFieldGuids.StartTimeUTC), value);
            }
        }

        [DynamicField(JobHistoryFields.EndTimeUTC, JobHistoryFieldGuids.EndTimeUTC, FieldTypes.Date)]
        public DateTime? EndTimeUTC
        {
            get
            {
                return GetField<DateTime?>(new System.Guid(JobHistoryFieldGuids.EndTimeUTC));
            }

            set
            {
                SetField<DateTime?>(new System.Guid(JobHistoryFieldGuids.EndTimeUTC), value);
            }
        }

        public const int BatchInstanceFieldLength = 50;

        [DynamicField(JobHistoryFields.BatchInstance, JobHistoryFieldGuids.BatchInstance, FieldTypes.FixedLengthText, 50)]
        public string BatchInstance
        {
            get
            {
                return GetField<string>(new System.Guid(JobHistoryFieldGuids.BatchInstance));
            }

            set
            {
                SetField<string>(new System.Guid(JobHistoryFieldGuids.BatchInstance), value);
            }
        }

        public const int DestinationWorkspaceFieldLength = 400;

        [DynamicField(JobHistoryFields.DestinationWorkspace, JobHistoryFieldGuids.DestinationWorkspace, FieldTypes.FixedLengthText, 400)]
        public string DestinationWorkspace
        {
            get
            {
                return GetField<string>(new System.Guid(JobHistoryFieldGuids.DestinationWorkspace));
            }

            set
            {
                SetField<string>(new System.Guid(JobHistoryFieldGuids.DestinationWorkspace), value);
            }
        }

        [DynamicField(JobHistoryFields.TotalItems, JobHistoryFieldGuids.TotalItems, FieldTypes.WholeNumber)]
        public long? TotalItems
        {
            get
            {
                return GetField<long?>(new System.Guid(JobHistoryFieldGuids.TotalItems));
            }

            set
            {
                SetField<long?>(new System.Guid(JobHistoryFieldGuids.TotalItems), value);
            }
        }

        [DynamicField(JobHistoryFields.DestinationWorkspaceInformation, JobHistoryFieldGuids.DestinationWorkspaceInformation, FieldTypes.MultipleObject)]
        public int[] DestinationWorkspaceInformation
        {
            get
            {
                return GetField<int[]>(new System.Guid(JobHistoryFieldGuids.DestinationWorkspaceInformation));
            }

            set
            {
                SetField<int[]>(new System.Guid(JobHistoryFieldGuids.DestinationWorkspaceInformation), value);
            }
        }

        [DynamicField(JobHistoryFields.JobType, JobHistoryFieldGuids.JobType, FieldTypes.SingleChoice)]
        public ChoiceRef JobType
        {
            get
            {
                return GetField<ChoiceRef>(new System.Guid(JobHistoryFieldGuids.JobType));
            }

            set
            {
                SetField<ChoiceRef>(new System.Guid(JobHistoryFieldGuids.JobType), value);
            }
        }

        public const int DestinationInstanceFieldLength = 400;

        [DynamicField(JobHistoryFields.DestinationInstance, JobHistoryFieldGuids.DestinationInstance, FieldTypes.FixedLengthText, 400)]
        public string DestinationInstance
        {
            get
            {
                return GetField<string>(new System.Guid(JobHistoryFieldGuids.DestinationInstance));
            }

            set
            {
                SetField<string>(new System.Guid(JobHistoryFieldGuids.DestinationInstance), value);
            }
        }

        public const int FilesSizeFieldLength = 20;

        [DynamicField(JobHistoryFields.FilesSize, JobHistoryFieldGuids.FilesSize, FieldTypes.FixedLengthText, 20)]
        public string FilesSize
        {
            get
            {
                return GetField<string>(new System.Guid(JobHistoryFieldGuids.FilesSize));
            }

            set
            {
                SetField<string>(new System.Guid(JobHistoryFieldGuids.FilesSize), value);
            }
        }

        public const int OverwriteFieldLength = 25;

        [DynamicField(JobHistoryFields.Overwrite, JobHistoryFieldGuids.Overwrite, FieldTypes.FixedLengthText, 25)]
        public string Overwrite
        {
            get
            {
                return GetField<string>(new System.Guid(JobHistoryFieldGuids.Overwrite));
            }

            set
            {
                SetField<string>(new System.Guid(JobHistoryFieldGuids.Overwrite), value);
            }
        }

        public const int JobIDFieldLength = 128;

        [DynamicField(JobHistoryFields.JobID, JobHistoryFieldGuids.JobID, FieldTypes.FixedLengthText, 128)]
        public string JobID
        {
            get
            {
                return GetField<string>(new System.Guid(JobHistoryFieldGuids.JobID));
            }

            set
            {
                SetField<string>(new System.Guid(JobHistoryFieldGuids.JobID), value);
            }
        }

        public const int NameFieldLength = 255;

        [DynamicField(JobHistoryFields.Name, JobHistoryFieldGuids.Name, FieldTypes.FixedLengthText, 255)]
        public string Name
        {
            get
            {
                return GetField<string>(new System.Guid(JobHistoryFieldGuids.Name));
            }

            set
            {
                SetField<string>(new System.Guid(JobHistoryFieldGuids.Name), value);
            }
        }

        [DynamicField(JobHistoryFields.ItemsRead, JobHistoryFieldGuids.ItemsRead, FieldTypes.WholeNumber)]
        public int? ItemsRead
        {
            get
            {
                return GetField<int?>(new System.Guid(JobHistoryFieldGuids.ItemsRead));
            }

            set
            {
                SetField<int?>(new System.Guid(JobHistoryFieldGuids.ItemsRead), value);
            }
        }

        private static System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> _fieldMetadata;

        public override System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> FieldMetadata
        {
            get
            {
                if (!(_fieldMetadata == null))
                    return _fieldMetadata;
                _fieldMetadata = GetFieldMetadata(typeof(JobHistory));
                return _fieldMetadata;
            }
        }
    }

    [DynamicObject(ObjectTypes.JobHistoryError, ObjectTypes.JobHistory, "", ObjectTypeGuids.JobHistoryError)]
    public partial class JobHistoryError : BaseRdo
    {
        public const int SourceUniqueIDFieldLength = 255;

        [DynamicField(JobHistoryErrorFields.SourceUniqueID, JobHistoryErrorFieldGuids.SourceUniqueID, FieldTypes.FixedLengthText, 255)]
        public string SourceUniqueID
        {
            get
            {
                return GetField<string>(new System.Guid(JobHistoryErrorFieldGuids.SourceUniqueID));
            }

            set
            {
                SetField<string>(new System.Guid(JobHistoryErrorFieldGuids.SourceUniqueID), value);
            }
        }

        [DynamicField(JobHistoryErrorFields.Error, JobHistoryErrorFieldGuids.Error, FieldTypes.LongText)]
        public string Error
        {
            get
            {
                return GetField<string>(new System.Guid(JobHistoryErrorFieldGuids.Error));
            }

            set
            {
                SetField<string>(new System.Guid(JobHistoryErrorFieldGuids.Error), value);
            }
        }

        [DynamicField(JobHistoryErrorFields.TimestampUTC, JobHistoryErrorFieldGuids.TimestampUTC, FieldTypes.Date)]
        public DateTime? TimestampUTC
        {
            get
            {
                return GetField<DateTime?>(new System.Guid(JobHistoryErrorFieldGuids.TimestampUTC));
            }

            set
            {
                SetField<DateTime?>(new System.Guid(JobHistoryErrorFieldGuids.TimestampUTC), value);
            }
        }

        [DynamicField(JobHistoryErrorFields.ErrorType, JobHistoryErrorFieldGuids.ErrorType, FieldTypes.SingleChoice)]
        public ChoiceRef ErrorType
        {
            get
            {
                return GetField<ChoiceRef>(new System.Guid(JobHistoryErrorFieldGuids.ErrorType));
            }

            set
            {
                SetField<ChoiceRef>(new System.Guid(JobHistoryErrorFieldGuids.ErrorType), value);
            }
        }

        [DynamicField(JobHistoryErrorFields.StackTrace, JobHistoryErrorFieldGuids.StackTrace, FieldTypes.LongText)]
        public string StackTrace
        {
            get
            {
                return GetField<string>(new System.Guid(JobHistoryErrorFieldGuids.StackTrace));
            }

            set
            {
                SetField<string>(new System.Guid(JobHistoryErrorFieldGuids.StackTrace), value);
            }
        }

        [DynamicField(JobHistoryErrorFields.ErrorStatus, JobHistoryErrorFieldGuids.ErrorStatus, FieldTypes.SingleChoice)]
        public ChoiceRef ErrorStatus
        {
            get
            {
                return GetField<ChoiceRef>(new System.Guid(JobHistoryErrorFieldGuids.ErrorStatus));
            }

            set
            {
                SetField<ChoiceRef>(new System.Guid(JobHistoryErrorFieldGuids.ErrorStatus), value);
            }
        }

        public const int NameFieldLength = 255;

        [DynamicField(JobHistoryErrorFields.Name, JobHistoryErrorFieldGuids.Name, FieldTypes.FixedLengthText, 255)]
        public string Name
        {
            get
            {
                return GetField<string>(new System.Guid(JobHistoryErrorFieldGuids.Name));
            }

            set
            {
                SetField<string>(new System.Guid(JobHistoryErrorFieldGuids.Name), value);
            }
        }

        [DynamicField(JobHistoryErrorFields.JobHistory, JobHistoryErrorFieldGuids.JobHistory, FieldTypes.SingleObject)]
        public int? JobHistory
        {
            get
            {
                return GetField<int?>(new System.Guid(JobHistoryErrorFieldGuids.JobHistory));
            }

            set
            {
                SetField<int?>(new System.Guid(JobHistoryErrorFieldGuids.JobHistory), value);
            }
        }

        private static System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> _fieldMetadata;

        public override System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> FieldMetadata
        {
            get
            {
                if (!(_fieldMetadata == null))
                    return _fieldMetadata;
                _fieldMetadata = GetFieldMetadata(typeof(JobHistoryError));
                return _fieldMetadata;
            }
        }
    }

    [DynamicObject(ObjectTypes.DestinationWorkspace, ObjectTypes.Workspace, "", ObjectTypeGuids.DestinationWorkspace)]
    public partial class DestinationWorkspace : BaseRdo
    {
        [DynamicField(DestinationWorkspaceFields.Documents, DestinationWorkspaceFieldGuids.Documents, FieldTypes.MultipleObject)]
        public int[] Documents
        {
            get
            {
                return GetField<int[]>(new System.Guid(DestinationWorkspaceFieldGuids.Documents));
            }

            set
            {
                SetField<int[]>(new System.Guid(DestinationWorkspaceFieldGuids.Documents), value);
            }
        }

        [DynamicField(DestinationWorkspaceFields.JobHistory, DestinationWorkspaceFieldGuids.JobHistory, FieldTypes.MultipleObject)]
        public int[] JobHistory
        {
            get
            {
                return GetField<int[]>(new System.Guid(DestinationWorkspaceFieldGuids.JobHistory));
            }

            set
            {
                SetField<int[]>(new System.Guid(DestinationWorkspaceFieldGuids.JobHistory), value);
            }
        }

        [DynamicField(DestinationWorkspaceFields.DestinationWorkspaceArtifactID, DestinationWorkspaceFieldGuids.DestinationWorkspaceArtifactID, FieldTypes.WholeNumber)]
        public int? DestinationWorkspaceArtifactID
        {
            get
            {
                return GetField<int?>(new System.Guid(DestinationWorkspaceFieldGuids.DestinationWorkspaceArtifactID));
            }

            set
            {
                SetField<int?>(new System.Guid(DestinationWorkspaceFieldGuids.DestinationWorkspaceArtifactID), value);
            }
        }

        public const int DestinationWorkspaceNameFieldLength = 400;

        [DynamicField(DestinationWorkspaceFields.DestinationWorkspaceName, DestinationWorkspaceFieldGuids.DestinationWorkspaceName, FieldTypes.FixedLengthText, 400)]
        public string DestinationWorkspaceName
        {
            get
            {
                return GetField<string>(new System.Guid(DestinationWorkspaceFieldGuids.DestinationWorkspaceName));
            }

            set
            {
                SetField<string>(new System.Guid(DestinationWorkspaceFieldGuids.DestinationWorkspaceName), value);
            }
        }

        public const int DestinationInstanceNameFieldLength = 400;

        [DynamicField(DestinationWorkspaceFields.DestinationInstanceName, DestinationWorkspaceFieldGuids.DestinationInstanceName, FieldTypes.FixedLengthText, 400)]
        public string DestinationInstanceName
        {
            get
            {
                return GetField<string>(new System.Guid(DestinationWorkspaceFieldGuids.DestinationInstanceName));
            }

            set
            {
                SetField<string>(new System.Guid(DestinationWorkspaceFieldGuids.DestinationInstanceName), value);
            }
        }

        [DynamicField(DestinationWorkspaceFields.DestinationInstanceArtifactID, DestinationWorkspaceFieldGuids.DestinationInstanceArtifactID, FieldTypes.WholeNumber)]
        public int? DestinationInstanceArtifactID
        {
            get
            {
                return GetField<int?>(new System.Guid(DestinationWorkspaceFieldGuids.DestinationInstanceArtifactID));
            }

            set
            {
                SetField<int?>(new System.Guid(DestinationWorkspaceFieldGuids.DestinationInstanceArtifactID), value);
            }
        }

        public const int NameFieldLength = 255;

        [DynamicField(DestinationWorkspaceFields.Name, DestinationWorkspaceFieldGuids.Name, FieldTypes.FixedLengthText, 255)]
        public string Name
        {
            get
            {
                return GetField<string>(new System.Guid(DestinationWorkspaceFieldGuids.Name));
            }

            set
            {
                SetField<string>(new System.Guid(DestinationWorkspaceFieldGuids.Name), value);
            }
        }

        private static System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> _fieldMetadata;

        public override System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> FieldMetadata
        {
            get
            {
                if (!(_fieldMetadata == null))
                    return _fieldMetadata;
                _fieldMetadata = GetFieldMetadata(typeof(DestinationWorkspace));
                return _fieldMetadata;
            }
        }
    }

    [DynamicObject(ObjectTypes.IntegrationPointType, ObjectTypes.Workspace, "", ObjectTypeGuids.IntegrationPointType)]
    public partial class IntegrationPointType : BaseRdo
    {
        public const int ApplicationIdentifierFieldLength = 50;

        [DynamicField(IntegrationPointTypeFields.ApplicationIdentifier, IntegrationPointTypeFieldGuids.ApplicationIdentifier, FieldTypes.FixedLengthText, 50)]
        public string ApplicationIdentifier
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointTypeFieldGuids.ApplicationIdentifier));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointTypeFieldGuids.ApplicationIdentifier), value);
            }
        }

        public const int IdentifierFieldLength = 40;

        [DynamicField(IntegrationPointTypeFields.Identifier, IntegrationPointTypeFieldGuids.Identifier, FieldTypes.FixedLengthText, 40)]
        public string Identifier
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointTypeFieldGuids.Identifier));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointTypeFieldGuids.Identifier), value);
            }
        }

        public const int NameFieldLength = 255;

        [DynamicField(IntegrationPointTypeFields.Name, IntegrationPointTypeFieldGuids.Name, FieldTypes.FixedLengthText, 255)]
        public string Name
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointTypeFieldGuids.Name));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointTypeFieldGuids.Name), value);
            }
        }

        private static System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> _fieldMetadata;

        public override System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> FieldMetadata
        {
            get
            {
                if (!(_fieldMetadata == null))
                    return _fieldMetadata;
                _fieldMetadata = GetFieldMetadata(typeof(IntegrationPointType));
                return _fieldMetadata;
            }
        }
    }

    [DynamicObject(ObjectTypes.IntegrationPointProfile, ObjectTypes.Workspace, "", ObjectTypeGuids.IntegrationPointProfile)]
    public partial class IntegrationPointProfile : BaseRdo
    {
        [DynamicField(IntegrationPointProfileFields.DestinationConfiguration, IntegrationPointProfileFieldGuids.DestinationConfiguration, FieldTypes.LongText)]
        public string DestinationConfiguration
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointProfileFieldGuids.DestinationConfiguration));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointProfileFieldGuids.DestinationConfiguration), value);
            }
        }

        [DynamicField(IntegrationPointProfileFields.DestinationProvider, IntegrationPointProfileFieldGuids.DestinationProvider, FieldTypes.SingleObject)]
        public int? DestinationProvider
        {
            get
            {
                return GetField<int?>(new System.Guid(IntegrationPointProfileFieldGuids.DestinationProvider));
            }

            set
            {
                SetField<int?>(new System.Guid(IntegrationPointProfileFieldGuids.DestinationProvider), value);
            }
        }

        [DynamicField(IntegrationPointProfileFields.EmailNotificationRecipients, IntegrationPointProfileFieldGuids.EmailNotificationRecipients, FieldTypes.LongText)]
        public string EmailNotificationRecipients
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointProfileFieldGuids.EmailNotificationRecipients));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointProfileFieldGuids.EmailNotificationRecipients), value);
            }
        }

        [DynamicField(IntegrationPointProfileFields.EnableScheduler, IntegrationPointProfileFieldGuids.EnableScheduler, FieldTypes.YesNo)]
        public bool? EnableScheduler
        {
            get
            {
                return GetField<bool?>(new System.Guid(IntegrationPointProfileFieldGuids.EnableScheduler));
            }

            set
            {
                SetField<bool?>(new System.Guid(IntegrationPointProfileFieldGuids.EnableScheduler), value);
            }
        }

        [DynamicField(IntegrationPointProfileFields.FieldMappings, IntegrationPointProfileFieldGuids.FieldMappings, FieldTypes.LongText)]
        public string FieldMappings
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointProfileFieldGuids.FieldMappings));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointProfileFieldGuids.FieldMappings), value);
            }
        }

        [DynamicField(IntegrationPointProfileFields.LogErrors, IntegrationPointProfileFieldGuids.LogErrors, FieldTypes.YesNo)]
        public bool? LogErrors
        {
            get
            {
                return GetField<bool?>(new System.Guid(IntegrationPointProfileFieldGuids.LogErrors));
            }

            set
            {
                SetField<bool?>(new System.Guid(IntegrationPointProfileFieldGuids.LogErrors), value);
            }
        }

        [DynamicField(IntegrationPointProfileFields.NextScheduledRuntimeUTC, IntegrationPointProfileFieldGuids.NextScheduledRuntimeUTC, FieldTypes.Date)]
        public DateTime? NextScheduledRuntimeUTC
        {
            get
            {
                return GetField<DateTime?>(new System.Guid(IntegrationPointProfileFieldGuids.NextScheduledRuntimeUTC));
            }

            set
            {
                SetField<DateTime?>(new System.Guid(IntegrationPointProfileFieldGuids.NextScheduledRuntimeUTC), value);
            }
        }

        [DynamicField(IntegrationPointProfileFields.OverwriteFields, IntegrationPointProfileFieldGuids.OverwriteFields, FieldTypes.SingleChoice)]
        public ChoiceRef OverwriteFields
        {
            get
            {
                return GetField<ChoiceRef>(new System.Guid(IntegrationPointProfileFieldGuids.OverwriteFields));
            }

            set
            {
                SetField<ChoiceRef>(new System.Guid(IntegrationPointProfileFieldGuids.OverwriteFields), value);
            }
        }

        [DynamicField(IntegrationPointProfileFields.ScheduleRule, IntegrationPointProfileFieldGuids.ScheduleRule, FieldTypes.LongText)]
        public string ScheduleRule
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointProfileFieldGuids.ScheduleRule));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointProfileFieldGuids.ScheduleRule), value);
            }
        }

        [DynamicField(IntegrationPointProfileFields.SourceConfiguration, IntegrationPointProfileFieldGuids.SourceConfiguration, FieldTypes.LongText)]
        public string SourceConfiguration
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointProfileFieldGuids.SourceConfiguration));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointProfileFieldGuids.SourceConfiguration), value);
            }
        }

        [DynamicField(IntegrationPointProfileFields.SourceProvider, IntegrationPointProfileFieldGuids.SourceProvider, FieldTypes.SingleObject)]
        public int? SourceProvider
        {
            get
            {
                return GetField<int?>(new System.Guid(IntegrationPointProfileFieldGuids.SourceProvider));
            }

            set
            {
                SetField<int?>(new System.Guid(IntegrationPointProfileFieldGuids.SourceProvider), value);
            }
        }

        [DynamicField(IntegrationPointProfileFields.Type, IntegrationPointProfileFieldGuids.Type, FieldTypes.SingleObject)]
        public int? Type
        {
            get
            {
                return GetField<int?>(new System.Guid(IntegrationPointProfileFieldGuids.Type));
            }

            set
            {
                SetField<int?>(new System.Guid(IntegrationPointProfileFieldGuids.Type), value);
            }
        }

        [DynamicField(IntegrationPointProfileFields.PromoteEligible, IntegrationPointProfileFieldGuids.PromoteEligible, FieldTypes.YesNo)]
        public bool? PromoteEligible
        {
            get
            {
                return GetField<bool?>(new System.Guid(IntegrationPointProfileFieldGuids.PromoteEligible));
            }

            set
            {
                SetField<bool?>(new System.Guid(IntegrationPointProfileFieldGuids.PromoteEligible), value);
            }
        }

        public const int NameFieldLength = 255;

        [DynamicField(IntegrationPointProfileFields.Name, IntegrationPointProfileFieldGuids.Name, FieldTypes.FixedLengthText, 255)]
        public string Name
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointProfileFieldGuids.Name));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointProfileFieldGuids.Name), value);
            }
        }

        [DynamicField(IntegrationPointProfileFields.CalculationState, IntegrationPointProfileFieldGuids.CalculationState, FieldTypes.LongText)]
        public string CalculationState
        {
            get
            {
                return GetField<string>(new System.Guid(IntegrationPointProfileFieldGuids.CalculationState));
            }

            set
            {
                SetField<string>(new System.Guid(IntegrationPointProfileFieldGuids.CalculationState), value);
            }
        }

        private static System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> _fieldMetadata;

        public override System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> FieldMetadata
        {
            get
            {
                if (!(_fieldMetadata == null))
                    return _fieldMetadata;
                _fieldMetadata = GetFieldMetadata(typeof(IntegrationPointProfile));
                return _fieldMetadata;
            }
        }
    }
}
