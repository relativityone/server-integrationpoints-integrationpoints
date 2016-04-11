using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
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
			var objectType = new ObjectType(TargetWorkspaceJobHistoryDTO.ObjectTypeGuid) { Fields = FieldValue.AllFields };
			ResultSet<ObjectType> resultSet = _rsapiClient.Repositories.ObjectType.Read(new[] { objectType });

			int? objectTypeArtifactId = null;
			if (resultSet.Success && resultSet.Results.Any())
			{
				objectTypeArtifactId = resultSet.Results.First().Artifact.DescriptorArtifactTypeID;
			}

			return objectTypeArtifactId;
		}

		public int CreateObjectType(int sourceWorkspaceArtifactTypeId)
		{
			var objectType = new ObjectType(TargetWorkspaceJobHistoryDTO.ObjectTypeGuid)
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

			return resultSet.Results.First().Artifact.ArtifactID;
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
				int rdoArtifactId = 0;
				ResultSet<RDO> existingRdo = _rsapiClient.Repositories.RDO.Read(rdo);
				if (!existingRdo.Success || existingRdo.Results.Count == 0)
				{
					rdoArtifactId = _rsapiClient.Repositories.RDO.CreateSingle(rdo);
				}
				else
				{
					rdoArtifactId = existingRdo.Results[0].Artifact.ArtifactID;
				}

				return rdoArtifactId;
			}
			catch (Exception e)
			{
				throw new Exception("Unable to create new instance of Job History", e);
			}
		}

		public bool ObjectTypeFieldsExist(int jobHistoryArtifactTypeId)
		{
			string[] fieldNames = new string[] { Contracts.Constants.JOBHISTORY_JOBHISTORYID_FIELD_NAME, Contracts.Constants.JOBHISTORY_JOBHISTORYNAME_FIELD_NAME };

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

		public IDictionary<Guid, int> CreateObjectTypeFields(int jobHistoryArtifactTypeId, IEnumerable<Guid> fieldGuids)
		{
			var objectType = new ObjectType() { DescriptorArtifactTypeID = jobHistoryArtifactTypeId };

			var jobHistoryFields = new List<kCura.Relativity.Client.DTOs.Field>()
			{
				new kCura.Relativity.Client.DTOs.Field()
				{
					Name = Contracts.Constants.JOBHISTORY_JOBHISTORYID_FIELD_NAME,
					Guids = new List<Guid>() { TargetWorkspaceJobHistoryDTO.Fields.JobHistoryIdFieldGuid },
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
					Guids = new List<Guid>() { TargetWorkspaceJobHistoryDTO.Fields.JobHistoryNameFieldGuid },
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

			kCura.Relativity.Client.DTOs.Field[] fieldsToCreate =
				jobHistoryFields.Where(x => fieldGuids.Contains(x.Guids.First())).ToArray();

			WriteResultSet<kCura.Relativity.Client.DTOs.Field> fieldWriteResultSet = _rsapiClient.Repositories.Field.Create(fieldsToCreate);
			if (!fieldWriteResultSet.Success)
			{
				throw new Exception("Unable to create fields for the Source Workspace object type: " + fieldWriteResultSet.Message);
			}

			int[] newFieldIds = fieldWriteResultSet.Results.Select(x => x.Artifact.ArtifactID).ToArray();

			ResultSet<kCura.Relativity.Client.DTOs.Field> newFieldResultSet = _rsapiClient.Repositories.Field.Read(newFieldIds);

			if (!newFieldResultSet.Success)
			{
				_rsapiClient.Repositories.Field.Delete(fieldsToCreate);
				throw new Exception("Unable to create fields for the Source Workspace object type: Failed to retrieve after creation: " + newFieldResultSet.Message);
			}

			IDictionary<Guid, int> guidToIdDictionary = newFieldResultSet.Results.ToDictionary(
				x =>
				{
					switch (x.Artifact.Name)
					{
						case Contracts.Constants.JOBHISTORY_JOBHISTORYID_FIELD_NAME:
							return TargetWorkspaceJobHistoryDTO.Fields.JobHistoryIdFieldGuid;

						case Contracts.Constants.JOBHISTORY_JOBHISTORYNAME_FIELD_NAME:
							return TargetWorkspaceJobHistoryDTO.Fields.JobHistoryNameFieldGuid;

						default:
							throw new Exception("Unexpected fields returned");
					}
				},
				y => y.Artifact.ArtifactID);

			return guidToIdDictionary;
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
					Width = "100",
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