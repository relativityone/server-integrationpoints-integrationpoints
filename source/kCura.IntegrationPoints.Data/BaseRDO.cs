using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Attributes;
using kCura.Relativity.Client.DTOs;
using Choice = kCura.Relativity.Client.Choice;

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

		protected BaseRdo() { }


		public virtual T GetField<T>(Guid fieldGuid)
		{
			var value = Rdo[fieldGuid].Value;
			var choice = value as Relativity.Client.DTOs.Choice;
			var artifact = value as Artifact;
			if (choice != null)
			{
				value = new Choice(choice.ArtifactID, choice.Name);
			}
			if (artifact != null)
			{
				value = artifact.ArtifactID;
			}

			return (T)value;
		}

		public string GetFieldName(Guid fieldGuid)
		{
			return this.FieldMetadata.Single(x => x.Value.FieldGuid == fieldGuid).Value.FieldName;
		}

		public virtual void SetField<T>(Guid fieldName, T fieldValue, Boolean markAsUpdated = true)
		{
			object value = fieldValue;
			var choice = fieldValue as Choice;
			if (choice != null)
			{
				var choiceDto = new Relativity.Client.DTOs.Choice(choice.ArtifactID);
				choiceDto.Name = choice.Name;
				value = choiceDto;
			}
			if (!Rdo.Fields.Any(x => x.Guids.Contains(fieldName)))
			{
				Rdo.Fields.Add(new FieldValue(fieldName, value));
			}
			else
			{
				Rdo[fieldName].Value = value;
			}

		}

		public static Dictionary<Guid, DynamicFieldAttribute> GetFieldMetadata(Type t)
		{
			return (from pi in t.GetProperties()
							select pi.GetCustomAttributes(typeof(DynamicFieldAttribute), true)
								into attributes
								where attributes.Any()
								select (DynamicFieldAttribute)attributes.First()).ToDictionary(attribute => attribute.FieldGuid);
		}

		public static DynamicObjectAttribute GetObjectMetadata(Type t)
		{
			return (DynamicObjectAttribute)t.GetCustomAttributes(typeof(DynamicObjectAttribute), false).First();
		}

		public int ArtifactId
		{
			get
			{
				return this.Rdo.ArtifactID;
			}
			set
			{
				//this is the shittiest hack ever
				var newRdo = new RDO(value);
				newRdo.ArtifactTypeGuids.AddRange(this.Rdo.ArtifactTypeGuids);
				newRdo.Fields.AddRange(this.Rdo.Fields);
				this.Rdo = newRdo;
			}
		}

		public int? ParentArtifactId { get; set; }
		public abstract Dictionary<Guid, DynamicFieldAttribute> FieldMetadata { get; }
		public abstract DynamicObjectAttribute ObjectMetadata { get; }
	}
}
