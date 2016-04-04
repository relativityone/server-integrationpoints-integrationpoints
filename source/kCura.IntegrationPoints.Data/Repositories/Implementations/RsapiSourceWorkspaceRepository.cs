using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using FieldType = kCura.Relativity.Client.FieldType;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RsapiSourceWorkspaceRepository : ISourceWorkspaceRepository
	{
		private readonly IRSAPIClient _rsapiClient;

		public RsapiSourceWorkspaceRepository(IRSAPIClient _rsapiClient)
		{
			this._rsapiClient = _rsapiClient;
		}

		public int? RetrieveObjectTypeDescriptorArtifactTypeId(int workspaceArtifactId)
		{
			_rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
			var criteria = new TextCondition(ObjectTypeFieldNames.Name, TextConditionEnum.EqualTo, "Source Workspace");

			Query<ObjectType> query = new Query<ObjectType>
			{
				Condition = criteria,
				Fields = FieldValue.AllFields
			};

			QueryResultSet<ObjectType> resultSet = _rsapiClient.Repositories.ObjectType.Query(query);

			int? objectTypeArtifactId = null;
			if (resultSet.Success && resultSet.Results.Any())
			{
				objectTypeArtifactId = resultSet.Results.First().Artifact.DescriptorArtifactTypeID;
			}

			return objectTypeArtifactId;
		}

		public int CreateObjectType(int workspaceArtifactId)
		{
			_rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
			var objectType = new ObjectType(SourceWorkspaceDTO.ObjectTypeGuid)
			{
				Name = "Source Workspace",
				ParentArtifactTypeID = 8,
				CopyInstancesOnParentCopy = false,
				CopyInstancesOnWorkspaceCreation = false,
				SnapshotAuditingEnabledOnDelete = false,
				Pivot = true,
				Sampling = false,
				PersistentLists = false,
			};

			WriteResultSet<ObjectType> resultSet = _rsapiClient.Repositories.ObjectType.Create(new [] { objectType });

			if (!resultSet.Success || !resultSet.Results.Any())
			{
				throw new Exception("Unable to create new Source Workspce object type: " + resultSet.Message);	
			}

			// WTF HAVE I DONE
			int descriptorArtifactTypeId = this.RetrieveObjectTypeDescriptorArtifactTypeId(workspaceArtifactId).Value;

			return descriptorArtifactTypeId;
		}

		public IDictionary<string, int> GetObjectTypeFieldArtifactIds(int workspaceArtifactId, int sourceWorkspaceObjectTypeId)
		{
			_rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

			string[] fieldNames = new string[] {"Source Workspace Case Id", "Source Workspace Case Name"};
			var criteria = new TextCondition(FieldFieldNames.Name, TextConditionEnum.In, fieldNames);
			var query = new Query<kCura.Relativity.Client.DTOs.Field> 
			{
				Fields = FieldValue.AllFields,
				Condition = criteria
				//ArtifactTypeID = sourceWorkspaceObjectTypeId this doesn't work
			};

			QueryResultSet<kCura.Relativity.Client.DTOs.Field> resultSet = _rsapiClient.Repositories.Field.Query(query);

			if (!resultSet.Success)
			{
				throw new Exception("Unable to retrieve Source Workspace fields: " + resultSet.Message);
			}

			// TODO: this cannot stay here...
			IDictionary<string, int> fieldNametoIdDictionary =
				resultSet.Results
					//.Where(x => x.Artifact.ArtifactTypeID == sourceWorkspaceObjectTypeId)
					.ToDictionary(x => x.Artifact.Name, y => y.Artifact.ArtifactID);
			
			// Validate that all fields exist
			foreach (string expectedFieldName in fieldNames)
			{
				if (!fieldNametoIdDictionary.ContainsKey(expectedFieldName))
				{
					throw new Exception(String.Format("Source Workspace is missing the \"{0}\" field", expectedFieldName));
				}
			}

			return fieldNametoIdDictionary;
		}

		public IDictionary<string, int> CreateObjectTypeFields(int workspaceArtifactId, int sourceWorkspaceObjectTypeId)
		{
			_rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
			var objectType = new ObjectType() { DescriptorArtifactTypeID = sourceWorkspaceObjectTypeId };

			var sourceWorkspaceFields = new List<kCura.Relativity.Client.DTOs.Field>()
			{
				new kCura.Relativity.Client.DTOs.Field()
				{
					Name = "Source Workspace Case Id",
					FieldTypeID = kCura.Relativity.Client.FieldType.WholeNumber,
					ObjectType = objectType,
					IsRequired = true,
					Linked =  false,
					OpenToAssociations = false,
					AllowSortTally = false,
					AllowGroupBy = false,
					AllowPivot = false,
					Width = "100",
					Wrapping = false
				},
				new kCura.Relativity.Client.DTOs.Field()
				{
					Name = "Source Workspace Case Name",
					FieldTypeID = kCura.Relativity.Client.FieldType.FixedLengthText,
					ObjectType = objectType,
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
					Unicode = false, // TODO: check this
					Length = 255 // TODO: check this
				},

				// TODO: add job history field here
			};

			WriteResultSet<kCura.Relativity.Client.DTOs.Field> fieldWriteResultSet = _rsapiClient.Repositories.Field.Create(sourceWorkspaceFields);
			if (!fieldWriteResultSet.Success)
			{
				throw new Exception("Unable to create fields for the Source Workspace object type: " + fieldWriteResultSet.Message);
			}

			IDictionary<string, int> fieldNameToArtifactIdDictionary = this.GetObjectTypeFieldArtifactIds(workspaceArtifactId, sourceWorkspaceObjectTypeId);

			return fieldNameToArtifactIdDictionary;
		}

		public int CreateSourceWorkspaceFieldOnDocument(int workspaceArtifactId, int sourceWorkspaceObjectTypeId)
		{
			_rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
			var documentObjectType = new ObjectType() { DescriptorArtifactTypeID = 10 };
			var sourceWorkspaceObjectType = new ObjectType() { DescriptorArtifactTypeID = sourceWorkspaceObjectTypeId };
			var fields = new List<kCura.Relativity.Client.DTOs.Field>()
			{
				new kCura.Relativity.Client.DTOs.Field()
				{
					Name = "Source Workspace",
					FieldTypeID = FieldType.MultipleObject,
					ObjectType = documentObjectType,
					AssociativeObjectType = sourceWorkspaceObjectType,
					AllowGroupBy = false,
					AllowPivot = false,
					AvailableInFieldTree = false,
					IsRequired = false,
					Width = "100"
				}
			};

			WriteResultSet<kCura.Relativity.Client.DTOs.Field> resultSet = _rsapiClient.Repositories.Field.Create(fields);

			var field = resultSet.Results.FirstOrDefault();
			if (!resultSet.Success || field == null)
			{
				throw new Exception("Unable to create Source Workspace field on Document: " + resultSet.Message);
			}

			int newFieldArtifactId = field.Artifact.ArtifactID;

			return newFieldArtifactId;
		}

		public int GetSourceWorkspaceFieldOnDocument(int workspaceArtifactId, int sourceWorkspaceObjectTypeId)
		{
			_rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
			var criteria = new TextCondition(FieldFieldNames.Name, TextConditionEnum.EqualTo, "Source Workspace");
			var query = new Query<kCura.Relativity.Client.DTOs.Field>
			{
				Fields = FieldValue.AllFields,
				Condition = criteria,
			};

			QueryResultSet<kCura.Relativity.Client.DTOs.Field> resultSet = _rsapiClient.Repositories.Field.Query(query);

			var field = resultSet.Results.FirstOrDefault();
			if (!resultSet.Success || field == null)
			{
				throw new Exception("Unable to retrieve Document fields: " + resultSet.Message);
			}

			return field.Artifact.ArtifactID;
		}

		public SourceWorkspaceDTO RetrieveForSourceWorkspaceId(int workspaceArtifactId, int sourceWorkspaceArtifactId, int sourceWorkspaceArtifactTypeId, IDictionary<string, int> fieldNameToIdDictionary)
		{
			_rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
			List<FieldValue> fields = fieldNameToIdDictionary.Values.Select(x => new FieldValue(x)).ToList();
			var condition = new WholeNumberCondition()
			{
				ArtifactID = fieldNameToIdDictionary["Source Workspace Case Id"],
				Operator = NumericConditionEnum.EqualTo,
				Value = new List<int> { sourceWorkspaceArtifactId }
			};

			var query = new Query<RDO>()
			{
				ArtifactTypeID = sourceWorkspaceArtifactTypeId,
				Fields  = FieldValue.AllFields,
				Condition = condition
			};
			QueryResultSet<RDO> resultSet = _rsapiClient.Repositories.RDO.Query(query);

			if (!resultSet.Success || !resultSet.Results.Any())
			{
				return null;
			}

			var rdo = resultSet.Results.First();
			var sourceWorkspaceDto = new SourceWorkspaceDTO()
			{
			};
//			sourceWorkspaceDto.ArtifactId = rdo.Artifact.ArtifactID,
//			sourceWorkspaceDto.Name = rdo.Artifact.Fields.Where(x => x.)
			//resultSet.Results.First().Artifact

			return null;
		}

		public int Create(int workspsaceArtifactId, int sourceWorkspaceArtifactTypeId, SourceWorkspaceDTO sourceWorkspaceDto, IDictionary<string, int> fieldNameToIdDictionary)
		{
			var fields = new List<FieldValue>()
			{
				new FieldValue() { ArtifactID = fieldNameToIdDictionary["Source Workspace Case Id"], Value = sourceWorkspaceDto.SourceWorkspaceArtifactId},
				new FieldValue() { ArtifactID = fieldNameToIdDictionary["Source Workspace Case Name"], Value = sourceWorkspaceDto.Name}
			};
			var rdo = new RDO()
			{
				ArtifactTypeID = sourceWorkspaceArtifactTypeId,
				Fields = fields
			};

			int rdoArtifactId = _rsapiClient.Repositories.RDO.CreateSingle(rdo);

			return rdoArtifactId;
		}
	}
}