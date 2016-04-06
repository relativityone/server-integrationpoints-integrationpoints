using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Artifact = kCura.Relativity.Client.DTOs.Artifact;
using FieldType = kCura.Relativity.Client.FieldType;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class TargetWorkspaceJobHistoryRepository : ITargetWorkspaceJobHistoryRepository
	{
		private readonly IRSAPIClient _rsapiClient;

		public TargetWorkspaceJobHistoryRepository(IRSAPIClient rsapiClient)
		{
			_rsapiClient = rsapiClient;
		}

		public int? RetrieveObjectTypeDescriptorArtifactTypeId()
		{
			var criteria = new TextCondition(ObjectTypeFieldNames.Name, TextConditionEnum.EqualTo, Contracts.Constants.SPECIAL_JOBHISTORY_FIELD_NAME);

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

		public int CreateObjectType(int sourceWorkspaceArtifactTypeId)
		{
			var objectType = new ObjectType(SourceWorkspaceDTO.ObjectTypeGuid)
			{
				Name = Contracts.Constants.SPECIAL_JOBHISTORY_FIELD_NAME,
				ParentArtifactTypeID = sourceWorkspaceArtifactTypeId,
				CopyInstancesOnParentCopy = false,
				CopyInstancesOnWorkspaceCreation = false,
				SnapshotAuditingEnabledOnDelete = false,
				Pivot = true,
				Sampling = false,
				PersistentLists = false,
			};

			WriteResultSet<ObjectType> resultSet = _rsapiClient.Repositories.ObjectType.Create(new[] { objectType });

			if (!resultSet.Success || !resultSet.Results.Any())
			{
				throw new Exception("Unable to create new Job History object type: " + resultSet.Message);
			}

			// We have to do this because the Descriptor Artifact Type Id isn't returned in the WriteResultSet :( -- biedrzycki: April 4th, 2016
			int descriptorArtifactTypeId = this.RetrieveObjectTypeDescriptorArtifactTypeId().Value;

			return descriptorArtifactTypeId;
		}

		public int Create(int jobHistoryArtifactTypeId,
			TargetWorkspaceJobHistoryDTO targetWorkspaceJobHistoryDto)
		{
			var fields = new List<FieldValue>()
			{
				new FieldValue(Contracts.Constants.JOBHISTORY_NAME_FIELD_NAME, targetWorkspaceJobHistoryDto.Name),
				new FieldValue(Contracts.Constants.JOBHISTORY_JOBHISTORYID_FIELD_NAME, targetWorkspaceJobHistoryDto.JobHistoryArtifactId),
				new FieldValue(Contracts.Constants.JOBHISTORY_JOBHISTORYNAME_FIELD_NAME, targetWorkspaceJobHistoryDto.JobHistoryName)
			};

			var parentArtifact = new kCura.Relativity.Client.DTOs.Artifact(targetWorkspaceJobHistoryDto.SourceWorkspaceArtifactId);
			var rdo = new RDO()
			{
				ParentArtifact = parentArtifact,
				ArtifactTypeID = jobHistoryArtifactTypeId,
				Fields = fields
			};

			try
			{
				int rdoArtifactId = _rsapiClient.Repositories.RDO.CreateSingle(rdo);

				return rdoArtifactId;
			}
			catch (Exception e)
			{
				throw new Exception("Unable to create new instance of Job History", e);
			}
		}

		public bool ObjectTypeFieldsExist(int jobHistoryArtifactTypeId)
		{

			string[] fieldNames = new string[]
			{Contracts.Constants.JOBHISTORY_JOBHISTORYID_FIELD_NAME, Contracts.Constants.JOBHISTORY_JOBHISTORYNAME_FIELD_NAME};

			var criteria = new TextCondition(FieldFieldNames.Name, TextConditionEnum.In, fieldNames);
			var query = new Query<kCura.Relativity.Client.DTOs.Field>
			{
				Fields = FieldValue.AllFields,
				Condition = criteria
				//ArtifactTypeID = jobHistoryArtifactTypeId this doesn't work
			};

			QueryResultSet<kCura.Relativity.Client.DTOs.Field> resultSet = _rsapiClient.Repositories.Field.Query(query);

			if (!resultSet.Success)
			{
				throw new Exception("Unable to retrieve Job History fields: " + resultSet.Message);
			}

			// TODO: this cannot stay here...
			IDictionary<string, int> fieldNametoIdDictionary =
				resultSet.Results
					.ToDictionary(x => x.Artifact.Name, y => y.Artifact.ArtifactID);

			// Validate that all fields exist
			return fieldNames.All(expectedFieldName => fieldNametoIdDictionary.ContainsKey(expectedFieldName));
		}

		public void CreateObjectTypeFields(int jobHistoryArtifactTypeId)
		{
			var objectType = new ObjectType() { DescriptorArtifactTypeID = jobHistoryArtifactTypeId };

			var sourceWorkspaceFields = new List<kCura.Relativity.Client.DTOs.Field>()
			{
				new kCura.Relativity.Client.DTOs.Field()
				{
					Name = Contracts.Constants.JOBHISTORY_JOBHISTORYID_FIELD_NAME,
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
					Name = Contracts.Constants.JOBHISTORY_JOBHISTORYNAME_FIELD_NAME,
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
				}
			};

			WriteResultSet<kCura.Relativity.Client.DTOs.Field> fieldWriteResultSet = _rsapiClient.Repositories.Field.Create(sourceWorkspaceFields);
			if (!fieldWriteResultSet.Success)
			{
				throw new Exception("Unable to create fields for the Source Workspace object type: " + fieldWriteResultSet.Message);
			}
		}

		public int CreateJobHistoryFieldOnDocument(int jobHistoryArtifactTypeId)
		{
			var documentObjectType = new ObjectType() { DescriptorArtifactTypeID = 10 };
			var jobHistoryObjectType = new ObjectType() { DescriptorArtifactTypeID = jobHistoryArtifactTypeId };
			var fields = new List<kCura.Relativity.Client.DTOs.Field>()
			{
				new kCura.Relativity.Client.DTOs.Field()
				{
					Name = Contracts.Constants.SPECIAL_JOBHISTORY_FIELD_NAME,
					FieldTypeID = FieldType.MultipleObject,
					ObjectType = documentObjectType,
					AssociativeObjectType = jobHistoryObjectType,
					AllowGroupBy = false,
					AllowPivot = false,
					AvailableInFieldTree = false,
					IsRequired = false,
					Width = "100"
				}
			};

			WriteResultSet<kCura.Relativity.Client.DTOs.Field> resultSet = _rsapiClient.Repositories.Field.Create(fields);

			Result<kCura.Relativity.Client.DTOs.Field> field = resultSet.Results.FirstOrDefault();
			if (!resultSet.Success || field == null)
			{
				throw new Exception("Unable to create Job History field on Document: " + resultSet.Message);
			}

			int newFieldArtifactId = field.Artifact.ArtifactID;

			return newFieldArtifactId;
		}

		public bool JobHistoryFieldExistsOnDocument(int jobHistoryArtifactTypeId)
		{
			var criteria = new TextCondition(FieldFieldNames.Name, TextConditionEnum.EqualTo, Contracts.Constants.SPECIAL_JOBHISTORY_FIELD_NAME);
			var query = new Query<kCura.Relativity.Client.DTOs.Field>
			{
				Fields = FieldValue.AllFields,
				Condition = criteria,
			};

			QueryResultSet<kCura.Relativity.Client.DTOs.Field> resultSet = _rsapiClient.Repositories.Field.Query(query);

			Result<kCura.Relativity.Client.DTOs.Field> field = resultSet.Results.FirstOrDefault();
			if (!resultSet.Success)
			{
				throw new Exception("Unable to retrieve Document fields: " + resultSet.Message);
			}

			bool fieldExists = field != null;

			return fieldExists;
		}
	}
}