using System;
using System.Collections.Generic;  
using System.Text;
using kCura.Relativity.Client;
using kCura.IntegrationPoints.Data.Attributes;

namespace kCura.IntegrationPoints.Data
{
 
	[DynamicObject(ObjectTypes.IntegrationPoints, ObjectTypes.Workspace, "", ObjectTypeGuids.IntegrationPoints)]
	public partial class IntegrationPoints : BaseRdo
	{
		[DynamicField(IntegrationPointsFields.ConnectionPath, IntegrationPointsFieldGuids.ConnectionPath, FieldTypes.LongText)]
		public string ConnectionPath
		{
			get
			{
				return GetField<string>(new System.Guid(IntegrationPointsFieldGuids.ConnectionPath));
			}
			set
			{
				SetField<string>(new System.Guid(IntegrationPointsFieldGuids.ConnectionPath), value);
			}
		}
		[DynamicField(IntegrationPointsFields.FilterString, IntegrationPointsFieldGuids.FilterString, FieldTypes.LongText)]
		public string FilterString
		{
			get
			{
				return GetField<string>(new System.Guid(IntegrationPointsFieldGuids.FilterString));
			}
			set
			{
				SetField<string>(new System.Guid(IntegrationPointsFieldGuids.FilterString), value);
			}
		}
		[DynamicField(IntegrationPointsFields.Authentication, IntegrationPointsFieldGuids.Authentication, FieldTypes.SingleChoice)]
		public Choice Authentication
		{
			get
			{
				return GetField<Choice>(new System.Guid(IntegrationPointsFieldGuids.Authentication));
			}
			set
			{
				SetField<Choice>(new System.Guid(IntegrationPointsFieldGuids.Authentication), value);
			}
		}
		public const int UserNameFieldLength = 255;
		[DynamicField(IntegrationPointsFields.UserName, IntegrationPointsFieldGuids.UserName, FieldTypes.FixedLengthText, 255)]
		public string UserName
		{
			get
			{
				return GetField<string>(new System.Guid(IntegrationPointsFieldGuids.UserName));
			}
			set
			{
				SetField<string>(new System.Guid(IntegrationPointsFieldGuids.UserName), value);
			}
		}
		public const int PasswordFieldLength = 255;
		[DynamicField(IntegrationPointsFields.Password, IntegrationPointsFieldGuids.Password, FieldTypes.FixedLengthText, 255)]
		public string Password
		{
			get
			{
				return GetField<string>(new System.Guid(IntegrationPointsFieldGuids.Password));
			}
			set
			{
				SetField<string>(new System.Guid(IntegrationPointsFieldGuids.Password), value);
			}
		}
		[DynamicField(IntegrationPointsFields.OverwriteFieldsOnImport, IntegrationPointsFieldGuids.OverwriteFieldsOnImport, FieldTypes.YesNo)]
		public bool? OverwriteFieldsOnImport
		{
			get
			{
				return GetField<bool?>(new System.Guid(IntegrationPointsFieldGuids.OverwriteFieldsOnImport));
			}
			set
			{
				SetField<bool?>(new System.Guid(IntegrationPointsFieldGuids.OverwriteFieldsOnImport), value);
			}
		}
		[DynamicField(IntegrationPointsFields.NextScheduledRuntime, IntegrationPointsFieldGuids.NextScheduledRuntime, FieldTypes.Date)]
		public DateTime? NextScheduledRuntime
		{
			get
			{
				return GetField<DateTime?>(new System.Guid(IntegrationPointsFieldGuids.NextScheduledRuntime));
			}
			set
			{
				SetField<DateTime?>(new System.Guid(IntegrationPointsFieldGuids.NextScheduledRuntime), value);
			}
		}
		[DynamicField(IntegrationPointsFields.LastRuntime, IntegrationPointsFieldGuids.LastRuntime, FieldTypes.Date)]
		public DateTime? LastRuntime
		{
			get
			{
				return GetField<DateTime?>(new System.Guid(IntegrationPointsFieldGuids.LastRuntime));
			}
			set
			{
				SetField<DateTime?>(new System.Guid(IntegrationPointsFieldGuids.LastRuntime), value);
			}
		}
		public const int NameFieldLength = 255;
		[DynamicField(IntegrationPointsFields.Name, IntegrationPointsFieldGuids.Name, FieldTypes.FixedLengthText, 255)]
		public string Name
		{
			get
			{
				return GetField<string>(new System.Guid(IntegrationPointsFieldGuids.Name));
			}
			set
			{
				SetField<string>(new System.Guid(IntegrationPointsFieldGuids.Name), value);
			}
		}
		private static System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> _fieldMetadata;
		public override System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> FieldMetadata
		{
			get
			{
				if (!(_fieldMetadata == null))
					return _fieldMetadata;
				_fieldMetadata = GetFieldMetadata(typeof(IntegrationPoints));
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
				_objectMetadata = GetObjectMetadata(typeof(IntegrationPoints));
				return _objectMetadata;
			}
		}
	}
 
	[DynamicObject(ObjectTypes.MappedFields, ObjectTypes.IntegrationPoints, "", ObjectTypeGuids.MappedFields)]
	public partial class MappedFields : BaseRdo
	{
		[DynamicField(MappedFieldsFields.WorkspaceField, MappedFieldsFieldGuids.WorkspaceField, FieldTypes.SingleObject)]
		public int? WorkspaceField
		{
			get
			{
				return GetField<int?>(new System.Guid(MappedFieldsFieldGuids.WorkspaceField));
			}
			set
			{
				SetField<int?>(new System.Guid(MappedFieldsFieldGuids.WorkspaceField), value);
			}
		}
		[DynamicField(MappedFieldsFields.SourceField, MappedFieldsFieldGuids.SourceField, FieldTypes.SingleObject)]
		public int? SourceField
		{
			get
			{
				return GetField<int?>(new System.Guid(MappedFieldsFieldGuids.SourceField));
			}
			set
			{
				SetField<int?>(new System.Guid(MappedFieldsFieldGuids.SourceField), value);
			}
		}
		public const int NameFieldLength = 255;
		[DynamicField(MappedFieldsFields.Name, MappedFieldsFieldGuids.Name, FieldTypes.FixedLengthText, 255)]
		public string Name
		{
			get
			{
				return GetField<string>(new System.Guid(MappedFieldsFieldGuids.Name));
			}
			set
			{
				SetField<string>(new System.Guid(MappedFieldsFieldGuids.Name), value);
			}
		}
		[DynamicField(MappedFieldsFields.IntegrationPoints, MappedFieldsFieldGuids.IntegrationPoints, FieldTypes.SingleObject)]
		public int? IntegrationPoints
		{
			get
			{
				return GetField<int?>(new System.Guid(MappedFieldsFieldGuids.IntegrationPoints));
			}
			set
			{
				SetField<int?>(new System.Guid(MappedFieldsFieldGuids.IntegrationPoints), value);
			}
		}
		private static System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> _fieldMetadata;
		public override System.Collections.Generic.Dictionary<Guid, DynamicFieldAttribute> FieldMetadata
		{
			get
			{
				if (!(_fieldMetadata == null))
					return _fieldMetadata;
				_fieldMetadata = GetFieldMetadata(typeof(MappedFields));
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
				_objectMetadata = GetObjectMetadata(typeof(MappedFields));
				return _objectMetadata;
			}
		}
	}

}