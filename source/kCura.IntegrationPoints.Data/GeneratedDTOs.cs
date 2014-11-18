using System;
using System.Collections.Generic;  
using System.Text;
using kCura.Relativity.Client;
using kCura.IntegrationPoints.Data.Attributes;

namespace kCura.IntegrationPoints.Data
{
 
	[DynamicObject(ObjectTypes.IntegrationPoint, ObjectTypes.Workspace, "", ObjectTypeGuids.IntegrationPoint)]
	public partial class IntegrationPoint : BaseRdo
	{
		[DynamicField(IntegrationPointFields.NextScheduledRuntime, IntegrationPointFieldGuids.NextScheduledRuntime, FieldTypes.Date)]
		public DateTime? NextScheduledRuntime
		{
			get
			{
				return GetField<DateTime?>(new System.Guid(IntegrationPointFieldGuids.NextScheduledRuntime));
			}
			set
			{
				SetField<DateTime?>(new System.Guid(IntegrationPointFieldGuids.NextScheduledRuntime), value);
			}
		}
		[DynamicField(IntegrationPointFields.LastRuntime, IntegrationPointFieldGuids.LastRuntime, FieldTypes.Date)]
		public DateTime? LastRuntime
		{
			get
			{
				return GetField<DateTime?>(new System.Guid(IntegrationPointFieldGuids.LastRuntime));
			}
			set
			{
				SetField<DateTime?>(new System.Guid(IntegrationPointFieldGuids.LastRuntime), value);
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
		[DynamicField(IntegrationPointFields.Frequency, IntegrationPointFieldGuids.Frequency, FieldTypes.SingleChoice)]
		public Choice Frequency
		{
			get
			{
				return GetField<Choice>(new System.Guid(IntegrationPointFieldGuids.Frequency));
			}
			set
			{
				SetField<Choice>(new System.Guid(IntegrationPointFieldGuids.Frequency), value);
			}
		}
		[DynamicField(IntegrationPointFields.Reoccur, IntegrationPointFieldGuids.Reoccur, FieldTypes.WholeNumber)]
		public int? Reoccur
		{
			get
			{
				return GetField<int?>(new System.Guid(IntegrationPointFieldGuids.Reoccur));
			}
			set
			{
				SetField<int?>(new System.Guid(IntegrationPointFieldGuids.Reoccur), value);
			}
		}
		[DynamicField(IntegrationPointFields.SendOn, IntegrationPointFieldGuids.SendOn, FieldTypes.LongText)]
		public string SendOn
		{
			get
			{
				return GetField<string>(new System.Guid(IntegrationPointFieldGuids.SendOn));
			}
			set
			{
				SetField<string>(new System.Guid(IntegrationPointFieldGuids.SendOn), value);
			}
		}
		[DynamicField(IntegrationPointFields.StartDate, IntegrationPointFieldGuids.StartDate, FieldTypes.Date)]
		public DateTime? StartDate
		{
			get
			{
				return GetField<DateTime?>(new System.Guid(IntegrationPointFieldGuids.StartDate));
			}
			set
			{
				SetField<DateTime?>(new System.Guid(IntegrationPointFieldGuids.StartDate), value);
			}
		}
		[DynamicField(IntegrationPointFields.EndDate, IntegrationPointFieldGuids.EndDate, FieldTypes.Date)]
		public DateTime? EndDate
		{
			get
			{
				return GetField<DateTime?>(new System.Guid(IntegrationPointFieldGuids.EndDate));
			}
			set
			{
				SetField<DateTime?>(new System.Guid(IntegrationPointFieldGuids.EndDate), value);
			}
		}
		public const int ScheduledTimeFieldLength = 10;
		[DynamicField(IntegrationPointFields.ScheduledTime, IntegrationPointFieldGuids.ScheduledTime, FieldTypes.FixedLengthText, 10)]
		public string ScheduledTime
		{
			get
			{
				return GetField<string>(new System.Guid(IntegrationPointFieldGuids.ScheduledTime));
			}
			set
			{
				SetField<string>(new System.Guid(IntegrationPointFieldGuids.ScheduledTime), value);
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

}