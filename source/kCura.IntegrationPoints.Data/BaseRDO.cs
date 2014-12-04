﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Attributes;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data
{
	public abstract class BaseRdo : IBaseRdo
	{
		private static System.Text.UnicodeEncoding _enc = new System.Text.UnicodeEncoding();

		private RDO _rdo;

		internal RDO Rdo
		{
			get
			{
				if (_rdo == null)
				{
					_rdo = new RDO();
					_rdo.ArtifactTypeGuids.Add(Guid.Parse(ObjectMetadata.ArtifactTypeGuid));
				}
				return _rdo;
			}
			set { _rdo = value; }
		}

		protected BaseRdo(){}

		public virtual T GetField<T>(Guid fieldGuid)
		{
			return (T) Rdo[fieldGuid].Value;
		}

		public string GetFieldName(Guid fieldGuid)
		{
			return this.FieldMetadata.Single(x => x.Value.FieldGuid == fieldGuid).Value.FieldName;
		}

		public virtual void SetField<T>(Guid fieldName, T fieldValue, Boolean markAsUpdated = true)
		{
			if (!Rdo.Fields.Contains(new FieldValue(fieldName)))
			{
				Rdo.Fields.Add(new FieldValue(fieldName, fieldValue));
			}
			else
			{
				Rdo[fieldName].Value = fieldValue;	
			}
			
		}

		public static Dictionary<Guid, DynamicFieldAttribute> GetFieldMetadata(Type t)
		{
			return (from pi in t.GetProperties() 
							select pi.GetCustomAttributes(typeof (DynamicFieldAttribute), true)
							into attributes where attributes.Any()
							select (DynamicFieldAttribute) attributes.First()).ToDictionary(attribute => attribute.FieldGuid);
		}

		public static DynamicObjectAttribute GetObjectMetadata(Type t)
		{
			return (DynamicObjectAttribute)t.GetCustomAttributes(typeof(DynamicObjectAttribute), false).First();
		}

		public int ArtifactId { get; set; }
		public int? ParentArtifactId { get; set; }
		public abstract Dictionary<Guid, DynamicFieldAttribute> FieldMetadata { get; }
		public abstract DynamicObjectAttribute ObjectMetadata { get; }
	}
}
