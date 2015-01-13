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
		[DynamicField(IntegrationPointFields.Overwrite, IntegrationPointFieldGuids.Overwrite, FieldTypes.SingleChoice)]
		public Choice Overwrite
		{
			get
			{
				return GetField<Choice>(new System.Guid(IntegrationPointFieldGuids.Overwrite));
			}
			set
			{
				SetField<Choice>(new System.Guid(IntegrationPointFieldGuids.Overwrite), value);
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
		[DynamicField(SourceProviderFields.LibLocation, SourceProviderFieldGuids.LibLocation, FieldTypes.File)]
		public int? LibLocation
		{
			get
			{
				return GetField<int?>(new System.Guid(SourceProviderFieldGuids.LibLocation));
			}
			set
			{
				SetField<int?>(new System.Guid(SourceProviderFieldGuids.LibLocation), value);
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

}