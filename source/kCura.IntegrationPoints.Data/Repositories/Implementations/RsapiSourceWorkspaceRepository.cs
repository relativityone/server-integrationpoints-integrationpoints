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

		public int? RetrieveObjectTypeDescriptorArtifactTypeId()
		{
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

		public int CreateObjectType()
		{
			var objectType = new ObjectType(SourceWorkspaceDTO.ObjectTypeGuid)
			{
				Name = Contracts.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME,
				ParentArtifactTypeID = (int) ArtifactType.Case,
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
			int descriptorArtifactTypeId = this.RetrieveObjectTypeDescriptorArtifactTypeId().Value;

			return descriptorArtifactTypeId;
		}

		public bool ObjectTypeFieldsExist(int sourceWorkspaceObjectTypeId)
		{
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
			IDictionary<string, int> fieldNameToIdDictionary =
				resultSet.Results
					.ToDictionary(x => x.Artifact.Name, y => y.Artifact.ArtifactID);
			
			// Validate that all fields exist
			return fieldNames.All(expectedFieldName => fieldNameToIdDictionary.ContainsKey(expectedFieldName));
		}

		public void CreateObjectTypeFields(int sourceWorkspaceObjectTypeId)
		{
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
				}
			};

			WriteResultSet<kCura.Relativity.Client.DTOs.Field> fieldWriteResultSet = _rsapiClient.Repositories.Field.Create(sourceWorkspaceFields);
			if (!fieldWriteResultSet.Success)
			{
				throw new Exception("Unable to create fields for the Source Workspace object type: " + fieldWriteResultSet.Message);
			}
		}

		public int CreateSourceWorkspaceFieldOnDocument(int sourceWorkspaceObjectTypeId)
		{
			var documentObjectType = new ObjectType() { DescriptorArtifactTypeID = (int) ArtifactType.Document };
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

		public bool SourceWorkspaceFieldExistsOnDocument(int sourceWorkspaceObjectTypeId)
		{
			var criteria = new TextCondition(FieldFieldNames.Name, TextConditionEnum.EqualTo, "Source Workspace");
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

		public SourceWorkspaceDTO RetrieveForSourceWorkspaceId(int sourceWorkspaceArtifactId, int sourceWorkspaceArtifactTypeId)
		{
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

		public int? RetrieveTabArtifactId(int sourceWorkspaceArtifactTypeId)
		{
			// Get the tab
			var tabNameCondition = new TextCondition(FieldFieldNames.Name, TextConditionEnum.EqualTo, Contracts.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME);
			var objectTypeCondition = new WholeNumberCondition(FieldFieldNames.ObjectType, NumericConditionEnum.EqualTo, sourceWorkspaceArtifactTypeId);
			var compositeCondition = new CompositeCondition(tabNameCondition, CompositeConditionEnum.And, objectTypeCondition);

			var tabQuery = new Query<Tab>()
			{
				Fields = FieldValue.AllFields,
				Condition = compositeCondition
			};

			QueryResultSet<Tab> resultSet = _rsapiClient.Repositories.Tab.Query(tabQuery);

			if (!resultSet.Success)
			{
				throw new Exception("Unable to retrieve the Source Workspace tab: " + resultSet.Message);
			}

			Result<Tab> tab = resultSet.Results.FirstOrDefault();

			return tab?.Artifact.ArtifactID;
		}

		public void DeleteTab(int tabArtifactId)
		{
			var artifactRequest = new List<ArtifactRequest>()
			{
				new ArtifactRequest((int)ArtifactType.Tab, tabArtifactId)
			};

			ResultSet resultSet = _rsapiClient.Delete(_rsapiClient.APIOptions, artifactRequest);

			if (!resultSet.Success)
			{
				throw new Exception("Unable to delete Source Workspace tab: " + resultSet.Message);
			}
		}
	}
}