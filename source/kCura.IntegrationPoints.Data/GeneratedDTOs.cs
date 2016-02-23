﻿using System;
using kCura.Relativity.Client;
using kCura.IntegrationPoints.Data.Attributes;

namespace kCura.IntegrationPoints.Data
{
 
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
		public Choice OverwriteFields
		{
			get
			{
				return GetField<Choice>(new System.Guid(IntegrationPointFieldGuids.OverwriteFields));
			}
			set
			{
				SetField<Choice>(new System.Guid(IntegrationPointFieldGuids.OverwriteFields), value);
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
		private static DynamicObjectAttribute _objectMetadata;
		public override DynamicObjectAttribute ObjectMetadata
		{
			get
			{
				if (!(_objectMetadata == null))
					return _objectMetadata;
				_objectMetadata = GetObjectMetadata(typeof(IntegrationPoint));
				return _objectMetadata;
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
		private static DynamicObjectAttribute _objectMetadata;
		public override DynamicObjectAttribute ObjectMetadata
		{
			get
			{
				if (!(_objectMetadata == null))
					return _objectMetadata;
				_objectMetadata = GetObjectMetadata(typeof(SourceProvider));
				return _objectMetadata;
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
		private static DynamicObjectAttribute _objectMetadata;
		public override DynamicObjectAttribute ObjectMetadata
		{
			get
			{
				if (!(_objectMetadata == null))
					return _objectMetadata;
				_objectMetadata = GetObjectMetadata(typeof(DestinationProvider));
				return _objectMetadata;
			}
		}
	}
 
	[DynamicObject(ObjectTypes.JobHistory, ObjectTypes.Workspace, "", ObjectTypeGuids.JobHistory)]
	public partial class JobHistory : BaseRdo
	{
		[DynamicField(JobHistoryFields.JobStatus, JobHistoryFieldGuids.JobStatus, FieldTypes.SingleChoice)]
		public Choice JobStatus
		{
			get
			{
				return GetField<Choice>(new System.Guid(JobHistoryFieldGuids.JobStatus));
			}
			set
			{
				SetField<Choice>(new System.Guid(JobHistoryFieldGuids.JobStatus), value);
			}
		}
		[DynamicField(JobHistoryFields.RecordsImported, JobHistoryFieldGuids.RecordsImported, FieldTypes.WholeNumber)]
		public int? RecordsImported
		{
			get
			{
				return GetField<int?>(new System.Guid(JobHistoryFieldGuids.RecordsImported));
			}
			set
			{
				SetField<int?>(new System.Guid(JobHistoryFieldGuids.RecordsImported), value);
			}
		}
		[DynamicField(JobHistoryFields.RecordsWithErrors, JobHistoryFieldGuids.RecordsWithErrors, FieldTypes.WholeNumber)]
		public int? RecordsWithErrors
		{
			get
			{
				return GetField<int?>(new System.Guid(JobHistoryFieldGuids.RecordsWithErrors));
			}
			set
			{
				SetField<int?>(new System.Guid(JobHistoryFieldGuids.RecordsWithErrors), value);
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
		private static DynamicObjectAttribute _objectMetadata;
		public override DynamicObjectAttribute ObjectMetadata
		{
			get
			{
				if (!(_objectMetadata == null))
					return _objectMetadata;
				_objectMetadata = GetObjectMetadata(typeof(JobHistory));
				return _objectMetadata;
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
		public Choice ErrorType
		{
			get
			{
				return GetField<Choice>(new System.Guid(JobHistoryErrorFieldGuids.ErrorType));
			}
			set
			{
				SetField<Choice>(new System.Guid(JobHistoryErrorFieldGuids.ErrorType), value);
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
		private static DynamicObjectAttribute _objectMetadata;
		public override DynamicObjectAttribute ObjectMetadata
		{
			get
			{
				if (!(_objectMetadata == null))
					return _objectMetadata;
				_objectMetadata = GetObjectMetadata(typeof(JobHistoryError));
				return _objectMetadata;
			}
		}
	}

}