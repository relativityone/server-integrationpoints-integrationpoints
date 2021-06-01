using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public abstract class RdoTestBase
	{
		public ArtifactTest Artifact { get; }

		public int ArtifactId => Artifact.ArtifactId;

		public int ParenObjectArtifactId { get; set; }

		protected RdoTestBase(string artifactTypeName, int? artifactId = null)
		{
			Artifact = new ArtifactTest
			{
				ArtifactId = artifactId ?? ArtifactProvider.NextId(),
				ArtifactType = artifactTypeName
			};
		}

		public abstract RelativityObject ToRelativityObject();

		public void LoadRelativityObject<T>(RelativityObject relativityObject) where T : RdoTestBase
		{
			var type = typeof(T);
			Dictionary<string, PropertyInfo> propertiesDictionary = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanWrite)
				.ToDictionary(x => x.Name, x => x);

			Artifact.ArtifactId = relativityObject.ArtifactID;
			
			foreach (FieldValuePair fieldValuePair in relativityObject.FieldValues.Where(x => propertiesDictionary.ContainsKey(x.Field.Name.Replace(" ", ""))))
			{
				try
				{
					var prop = propertiesDictionary[fieldValuePair.Field.Name.Replace(" ", "")];
					
					prop.SetValue(this, Sanitize(fieldValuePair.Value));
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
					Guids = choiceRef.Guid.HasValue ? new List<Guid>{choiceRef.Guid.Value} : new List<Guid>()
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
	}
}