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

}