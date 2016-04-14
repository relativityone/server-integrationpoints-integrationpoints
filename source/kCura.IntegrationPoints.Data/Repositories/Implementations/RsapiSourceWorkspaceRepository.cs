using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Core;
using FieldType = kCura.Relativity.Client.FieldType;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RsapiSourceWorkspaceRepository : ISourceWorkspaceRepository
	{
		private readonly IRSAPIClient _rsapiClient;

		public RsapiSourceWorkspaceRepository(
			IRSAPIClient rsapiClient)
		{
			this._rsapiClient = rsapiClient;
		}

		public int CreateObjectType()
		{
			var objectType = new ObjectType(SourceWorkspaceDTO.ObjectTypeGuid)
			{
				Name = Contracts.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME,
				ParentArtifactTypeID = (int)ArtifactType.Case,
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
				throw new Exception("Unable to create new Source Workspace object type: " + resultSet.Message);
			}

			return resultSet.Results.First().Artifact.ArtifactID;
		}

		public IDictionary<Guid, int> CreateObjectTypeFields(int sourceWorkspaceObjectTypeId, IEnumerable<Guid> fieldGuids)
		{
			var objectType = new ObjectType() { DescriptorArtifactTypeID = sourceWorkspaceObjectTypeId };

			var sourceWorkspaceFields = new List<kCura.Relativity.Client.DTOs.Field>()
			{
				new kCura.Relativity.Client.DTOs.Field()
				{
					Name = Contracts.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME,
					Guids = new List<Guid>() { SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid },
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
					Guids = new List<Guid>() { SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid },
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
				sourceWorkspaceFields.Where(x => fieldGuids.Contains(x.Guids.First())).ToArray();

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
						case Contracts.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME:
							return SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid;

						case Contracts.Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME:
							return SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid;

						default:
							throw new Exception("Unexpected fields returned");
					}
				},
				y => y.Artifact.ArtifactID);

			return guidToIdDictionary;
		}

		public int CreateSourceWorkspaceFieldOnDocument(int sourceWorkspaceObjectTypeId)
		{
			var documentObjectType = new ObjectType() { DescriptorArtifactTypeID = (int)ArtifactType.Document };
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
			Result<kCura.Relativity.Client.DTOs.Field> field = resultSet.Results.FirstOrDefault();
			if (!resultSet.Success || field == null)
			{
				throw new Exception("Unable to create Source Workspace field on Document: " + resultSet.Message);
			}

			int newFieldArtifactId = field.Artifact.ArtifactID;

			return newFieldArtifactId;
		}

		public SourceWorkspaceDTO RetrieveForSourceWorkspaceId(int sourceWorkspaceArtifactId)
		{
			var condition = new WholeNumberCondition(Contracts.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME, NumericConditionEnum.EqualTo, sourceWorkspaceArtifactId);
			var query = new Query<RDO>()
			{
				ArtifactTypeGuid = SourceWorkspaceDTO.ObjectTypeGuid,
				Fields = FieldValue.AllFields,
				Condition = condition
			};
			QueryResultSet<RDO> resultSet = _rsapiClient.Repositories.RDO.Query(query);

			if (!resultSet.Success || !resultSet.Results.Any())
			{
				return null;
			}

			Result<RDO> rdo = resultSet.Results.First();
			var sourceWorkspaceDto = new SourceWorkspaceDTO() { ArtifactId = rdo.Artifact.ArtifactID };

			foreach (FieldValue fieldValue in rdo.Artifact.Fields)
			{
				if (fieldValue.Name == Contracts.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME)
				{
					sourceWorkspaceDto.SourceCaseArtifactId = fieldValue.ValueAsWholeNumber.Value;
				}
				else if (fieldValue.Name == Contracts.Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME)
				{
					sourceWorkspaceDto.SourceCaseName = fieldValue.ValueAsFixedLengthText;
				}
				else if (fieldValue.Name == Contracts.Constants.SOURCEWORKSPACE_NAME_FIELD_NAME)
				{
					sourceWorkspaceDto.Name = fieldValue.ValueAsFixedLengthText;
				}
			}

			return sourceWorkspaceDto;
		}

		public int Create(int sourceWorkspaceArtifactTypeId, SourceWorkspaceDTO sourceWorkspaceDto)
		{
			var fields = new List<FieldValue>()
			{
				new FieldValue(Contracts.Constants.SOURCEWORKSPACE_NAME_FIELD_NAME, sourceWorkspaceDto.Name),
				new FieldValue(Contracts.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME, sourceWorkspaceDto.SourceCaseArtifactId),
				new FieldValue(Contracts.Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME, sourceWorkspaceDto.SourceCaseName)
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

		public void Update(SourceWorkspaceDTO sourceWorkspaceDto)
		{
			var fields = new List<FieldValue>()
			{
				new FieldValue(Contracts.Constants.SOURCEWORKSPACE_NAME_FIELD_NAME, sourceWorkspaceDto.Name, true),
				new FieldValue(Contracts.Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME, sourceWorkspaceDto.SourceCaseName, true),
			};

			var rdo = new RDO(sourceWorkspaceDto.ArtifactTypeId, sourceWorkspaceDto.ArtifactId)
			{
				Fields = fields
			};

			try
			{
				_rsapiClient.Repositories.RDO.UpdateSingle(rdo);
			}
			catch (Exception e)
			{
				throw new Exception("Unable to update Source Workspace instance", e);
			}
		}
	}
}