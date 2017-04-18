using System;
using System.Collections.Generic;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Artifact = kCura.Relativity.Client.DTOs.Artifact;
using Field = kCura.Relativity.Client.DTOs.Field;

namespace kCura.IntegrationPoints.Domain.Models
{
	public class SourceJobDTO
	{
		public static readonly Guid ObjectTypeGuid = new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");
		public int SourceWorkspaceArtifactId { get; set; }
		public int ArtifactId { get; set; }
		public int ArtifactTypeId { get; set; }
		public string Name { get; set; }
		public int JobHistoryArtifactId { get; set; }
		public string JobHistoryName { get; set; }

		public RDO ToRdo()
		{
			var fields = new List<FieldValue>
			{
				new FieldValue(Constants.SOURCEJOB_NAME_FIELD_NAME, Name),
				new FieldValue(Constants.SOURCEJOB_JOBHISTORYID_FIELD_NAME, JobHistoryArtifactId),
				new FieldValue(Constants.SOURCEJOB_JOBHISTORYNAME_FIELD_NAME, JobHistoryName)
			};

			return new RDO
			{
				ParentArtifact = new Artifact(SourceWorkspaceArtifactId),
				ArtifactTypeID = ArtifactTypeId,
				Fields = fields
			};
		}

		public static class Fields
		{
			public static readonly Guid JobHistoryIdFieldGuid = new Guid("2bf54e79-7f75-4a51-a99a-e4d68f40a231");
			public static readonly Guid JobHistoryNameFieldGuid = new Guid("0b8fcebf-4149-4f1b-a8bc-d88ff5917169");
			public static readonly Guid JobHistoryFieldOnDocumentGuid = new Guid("7cc3faaf-cbb8-4315-a79f-3aa882f1997f");

			public static IDictionary<Guid, Field> GetFieldsDefinition(ObjectType objectType)
			{
				var fieldsDefinition = FieldsDefinition;
				foreach (var keyValuePair in fieldsDefinition)
				{
					keyValuePair.Value.ObjectType = objectType;
				}
				return fieldsDefinition;
			}

			private static IDictionary<Guid, Field> FieldsDefinition => new Dictionary<Guid, Field>
			{
				{
					JobHistoryIdFieldGuid, new Field
					{
						Name = Constants.SOURCEJOB_JOBHISTORYID_FIELD_NAME,
						Guids = new List<Guid> {JobHistoryIdFieldGuid},
						FieldTypeID = FieldType.WholeNumber,
						IsRequired = true,
						Linked = false,
						OpenToAssociations = false,
						AllowSortTally = false,
						AllowGroupBy = false,
						AllowPivot = false,
						Width = "100",
						Wrapping = false
					}
				},
				{
					JobHistoryNameFieldGuid, new Field
					{
						Name = Constants.SOURCEJOB_JOBHISTORYNAME_FIELD_NAME,
						Guids = new List<Guid> {JobHistoryNameFieldGuid},
						FieldTypeID = FieldType.FixedLengthText,
						IsRequired = true,
						IncludeInTextIndex = false,
						Linked = false,
						AllowHTML = false,
						AllowSortTally = false,
						AllowGroupBy = false,
						AllowPivot = false,
						OpenToAssociations = false,
						Width = "100",
						Wrapping = false,
						Unicode = false,
						Length = 255
					}
				}
			};
		}
	}
}