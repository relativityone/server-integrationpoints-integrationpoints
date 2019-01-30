using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Attributes;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Artifact = kCura.Relativity.Client.DTOs.Artifact;
using Choice = kCura.Relativity.Client.DTOs.Choice;

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

		public virtual bool HasField(Guid fieldGuid)
		{
			return _rdo.Fields.Any(x => x.Guids.Contains(fieldGuid));
		}

		public virtual T GetField<T>(Guid fieldGuid)
		{
			var value = Rdo[fieldGuid].Value;
			object v = ConvertForGet(FieldMetadata[fieldGuid].Type, value);
			return (T)v;
		}

		public string GetFieldName(Guid fieldGuid)
		{
			return this.FieldMetadata.Single(x => x.Value.FieldGuid == fieldGuid).Value.FieldName;
		}

		public virtual void SetField<T>(Guid fieldName, T fieldValue, bool markAsUpdated = true)
		{
			object value = ConvertValue(FieldMetadata[fieldName].Type, fieldValue);
			if (!Rdo.Fields.Any(x => x.Guids.Contains(fieldName)))
			{
				Rdo.Fields.Add(new FieldValue(fieldName, value));
			}
			else
			{
				Rdo[fieldName].Value = value;
			}
		}

		internal object ConvertForGet(string fieldType, object value)
		{
			switch (fieldType)
			{
				case FieldTypes.MultipleObject:
					if (value is IEnumerable<Artifact>)
					{
						return ((IEnumerable<Artifact>)value).Select(x => x.ArtifactID).ToArray();
					}
					return new int[]{};
				case FieldTypes.SingleObject:
					var a = value as Artifact;
					if (a != null)
					{
						return a.ArtifactID;
					}
					return value;
				case FieldTypes.SingleChoice:
					var choice = value as Relativity.Client.DTOs.Choice;
					if (choice != null)
					{
						return new Choice(choice.ArtifactID) {Name = choice.Name, Guids = choice.Guids };
					}
					return value;
				default:
					return value;
			}

		}

		internal object ConvertValue(string fieldType, object value)
		{
			if (value == null) return value;
			object newValue = null;

			switch (fieldType)
			{
				case FieldTypes.MultipleChoice:
					Choice[] choices = null;
					if (value is List<Choice>) choices = ((List<Choice>)value).ToArray();
					else if (value is object[]) choices = ((object[])value).Select(x => ((Choice)x)).ToArray();
					else if (value is Choice[]) choices = (Choice[])value;
					MultiChoiceFieldValueList multiChoices = new MultiChoiceFieldValueList();
					multiChoices.UpdateBehavior = MultiChoiceUpdateBehavior.Replace;
					if (choices != null)
					{
						foreach (var choice in choices)
						{
							var choiceDto = new Relativity.Client.DTOs.Choice(choice.Guids.First()) { Name = choice.Name };
							multiChoices.Add(choiceDto);
						}
						newValue = multiChoices;
					}
					break;
				case FieldTypes.SingleChoice:
					Choice singleChoice = null;
					if (value is Choice)
					{
						singleChoice = (Choice)value;
						
						if (singleChoice.ArtifactID > 0 || singleChoice.Guids.Any())
						{
							newValue = new Choice(singleChoice.ArtifactID) {Name = singleChoice.Name, Guids = singleChoice.Guids};
						}
						else
						{
							throw new Exception("Can not determine choice with no artifact id or guid.");
						}
					}
					break;
				case FieldTypes.SingleObject:
					if (value is int)
					{
						RDO obj = new RDO((int)value);
						newValue = obj;
					}
					break;
				case FieldTypes.MultipleObject:
					int[] multipleObjectIDs = null;
					if (value is int[])
					{
						multipleObjectIDs = ((int[])value);
						FieldValueList<RDO> objects = new FieldValueList<RDO>();
						foreach (var objectID in multipleObjectIDs)
						{
							objects.Add(new RDO(objectID));
						}
						newValue = objects;
					}
					break;
				default:
					newValue = value;
					break;
			}
			return newValue;
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

		public int? ParentArtifactId
		{
			get
			{
				if (this.Rdo.ParentArtifact != null)
				{
					return this.Rdo.ParentArtifact.ArtifactID;
				}
				return null;
			}
			set
			{
				if (value.HasValue)
				{
					this.Rdo.ParentArtifact = new kCura.Relativity.Client.DTOs.Artifact(value.Value);
				}
			}
		}
		public abstract Dictionary<Guid, DynamicFieldAttribute> FieldMetadata { get; }
		public abstract DynamicObjectAttribute ObjectMetadata { get; }
	}
}
