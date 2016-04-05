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
			var criteria = new TextCondition(ObjectTypeFieldNames.Name, TextConditionEnum.EqualTo, Contracts.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME);

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
				Name = Contracts.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME,
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
				throw new Exception("Unable to create new Source Workspace object type: " + resultSet.Message);	
			}

			// We have to do this because the Descriptor Artifact Type Id isn't returned in the WriteResultSet :( -- biedrzycki: April 4th, 2016
			int descriptorArtifactTypeId = this.RetrieveObjectTypeDescriptorArtifactTypeId(workspaceArtifactId).Value;

			return descriptorArtifactTypeId;
		}

		public bool ObjectTypeFieldExist(int workspaceArtifactId, int sourceWorkspaceObjectTypeId)
		{
			_rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

			string[] fieldNames = new string[] { Contracts.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME, Contracts.Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME };
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
					.ToDictionary(x => x.Artifact.Name, y => y.Artifact.ArtifactID);
			
			// Validate that all fields exist
			return fieldNames.All(expectedFieldName => fieldNametoIdDictionary.ContainsKey(expectedFieldName));
		}

		public void CreateObjectTypeFields(int workspaceArtifactId, int sourceWorkspaceObjectTypeId)
		{
			_rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
			var objectType = new ObjectType() { DescriptorArtifactTypeID = sourceWorkspaceObjectTypeId };

			var sourceWorkspaceFields = new List<kCura.Relativity.Client.DTOs.Field>()
			{
				new kCura.Relativity.Client.DTOs.Field()
				{
					Name = Contracts.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME,
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
					Name = Contracts.Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME,
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
					Name = Contracts.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME,
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

		public SourceWorkspaceDTO RetrieveForSourceWorkspaceId(int workspaceArtifactId, int sourceWorkspaceArtifactId, int sourceWorkspaceArtifactTypeId)
		{
			_rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

			
			var condition = new WholeNumberCondition(Contracts.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME, NumericConditionEnum.EqualTo, sourceWorkspaceArtifactId);
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
			var sourceWorkspaceDto = new SourceWorkspaceDTO() { ArtifactId = rdo.Artifact.ArtifactID };

			foreach (FieldValue fieldValue in rdo.Artifact.Fields)
			{
				if (fieldValue.Name == Contracts.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME)
				{
					sourceWorkspaceDto.SourceWorkspaceArtifactId = fieldValue.ValueAsWholeNumber.Value;
				}
				else if (fieldValue.Name == Contracts.Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME)
				{
					sourceWorkspaceDto.SourceWorkspaceName = fieldValue.ValueAsFixedLengthText;
				}
				else if (fieldValue.Name == Contracts.Constants.SOURCEWORKSPACE_NAME_FIELD_NAME)
				{
					sourceWorkspaceDto.Name = fieldValue.ValueAsFixedLengthText;
				}
			}

			return sourceWorkspaceDto;
		}

		public int Create(int workspaceArtifactId, int sourceWorkspaceArtifactTypeId, SourceWorkspaceDTO sourceWorkspaceDto)
		{
			var fields = new List<FieldValue>()
			{
				new FieldValue(Contracts.Constants.SOURCEWORKSPACE_NAME_FIELD_NAME, sourceWorkspaceDto.Name),
				new FieldValue(Contracts.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME, sourceWorkspaceDto.SourceWorkspaceArtifactId),
				new FieldValue(Contracts.Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME, sourceWorkspaceDto.SourceWorkspaceName)
			};
			var rdo = new RDO()
			{
				ArtifactTypeID = sourceWorkspaceArtifactTypeId,
				Fields = fields
			};

			try
			{
				int rdoArtifactId = _rsapiClient.Repositories.RDO.CreateSingle(rdo);

				return rdoArtifactId;
			}
			catch
			{
				throw new Exception("Unable to create new instance of Source Workspace");
			}
		}

		public void Update(int workspaceArtifactId, SourceWorkspaceDTO sourceWorkspaceDto)
		{
			_rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
			var fields = new List<FieldValue>()
			{
				new FieldValue(Contracts.Constants.SOURCEWORKSPACE_NAME_FIELD_NAME, sourceWorkspaceDto.Name, true),
				new FieldValue(Contracts.Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME, sourceWorkspaceDto.SourceWorkspaceName, true),
			};
			var rdo = new RDO()
			{
				ArtifactTypeID = workspaceArtifactId,
				Fields = fields
			};

			try
			{
				_rsapiClient.Repositories.RDO.UpdateSingle(rdo);
			}
			catch
			{
				throw new Exception("Unable to update Source Workspace instance");
			}
		}
	}
}