using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autofac;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public abstract class RdoTestBase
    {
	    public ArtifactTest Artifact { get; }

        public int ArtifactId => Artifact.ArtifactId;

        public int ParentObjectArtifactId { get; set; }

        protected RdoTestBase(string artifactTypeName, int? artifactId = null)
        {
            Artifact = new ArtifactTest
            {
                ArtifactId = artifactId ?? ArtifactProvider.NextId(),
                ArtifactType = artifactTypeName
            };

			Values = Guids.ToDictionary(g => g, g => (object)null);
        }

		public Dictionary<Guid, object> Values { get; }

		public abstract List<Guid> Guids { get; }

        public abstract RelativityObject ToRelativityObject();

        public void LoadRelativityObjectByName<T>(RelativityObject relativityObject)
        {
            LoadRelativityObjectByName(typeof(T), relativityObject);
        }

        public void LoadRelativityObjectByName(Type type, RelativityObject relativityObject)
        {
            string SanitizeFieldName(string name)
            {
                return name.Replace(" ", "").ToLowerInvariant();
            }
            
            if (!type.IsAssignableTo<RdoTestBase>())
            {
                Debugger.Break();
                throw new Exception($"Type {type.Name} is not assignable to {nameof(RdoTestBase)}");
            }

            Dictionary<string, PropertyInfo> propertiesDictionary = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanWrite)
                .ToDictionary(x => SanitizeFieldName(x.Name), x => x);

            ParentObjectArtifactId = relativityObject?.ParentObject?.ArtifactID ?? 0;
            Artifact.ArtifactId = relativityObject.ArtifactID;

            foreach (FieldValuePair fieldValuePair in relativityObject.FieldValues)
            {
                fieldValuePair.Field.Name = SanitizeFieldName(fieldValuePair.Field.Name);
            }

			foreach (FieldValuePair fieldValuePair in relativityObject.FieldValues
                .Where(x => propertiesDictionary.ContainsKey(x.Field.Name)))
            {
                try
                {
                    var prop = propertiesDictionary[fieldValuePair.Field.Name];

                    if (prop.GetValue(this) == null && fieldValuePair.Value == null)
                    {
                        prop.SetValue(this, null);
                        continue;
                    }

                    prop.SetValue(this, Sanitize(fieldValuePair.Value));
                }
                catch (Exception)
                {
                    Debugger.Break();
                    throw;
                }
            }
        }

        public void LoadRelativityObjectByGuid<T>(RelativityObject relativityObject) where T : RdoTestBase
        {
            LoadRelativityObjectByGuid(typeof(T), relativityObject);
        }

		public void LoadRelativityObjectByGuid(Type type, RelativityObject relativityObject)
        {
            ParentObjectArtifactId = relativityObject?.ParentObject?.ArtifactID ?? 0;
            Artifact.ArtifactId = relativityObject.ArtifactID;

			foreach (FieldValuePair fieldValuePair in relativityObject.FieldValues)
			{
				try
				{
					Guid propId = fieldValuePair.Field.Guids.Single();

					SetField(propId, fieldValuePair.Value);
				}
				catch (Exception)
				{
					Debugger.Break();
					throw;
				}
			}
		}

        protected object Sanitize(object value)
        {
            if (value is Relativity.Services.Objects.DataContracts.ChoiceRef choiceRef)
            {
                return new ChoiceRef
                {
                    ArtifactID = choiceRef.ArtifactID,
                    Guids = choiceRef.Guid.HasValue ? new List<Guid> {choiceRef.Guid.Value} : new List<Guid>()
                };
            }

            if (value is RelativityObjectValue objectValue)
            {
                return objectValue.ArtifactID;
            }

            if (value is RelativityObjectValue[] objectValues)
            {
                return objectValues.Select(x => x.ArtifactID).ToArray();
            }

            return value;
		}

		protected virtual void SetField(Guid guid, object value) => Values[guid] = Sanitize(value);

		protected object GetField(Guid guid)
		{
			object value = Values[guid];

			return Sanitize(value);
        }
    }
}