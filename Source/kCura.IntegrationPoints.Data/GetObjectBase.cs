using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data
{
	public abstract class GetObjectBase
	{
		private readonly Type _objectType;
		public GetObjectBase(Type objectType)
		{
			_objectType = objectType;
		}

		protected List<FieldValue> GetFields()
		{
			return (from field in (BaseRdo.GetFieldMetadata(_objectType).Values).ToList()
							select new FieldValue(field.FieldGuid)).ToList();
		}
	}
}
