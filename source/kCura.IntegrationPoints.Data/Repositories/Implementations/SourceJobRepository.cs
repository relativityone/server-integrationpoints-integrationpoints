﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SourceJobRepository : ISourceJobRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;

		public SourceJobRepository(IHelper helper, int workspaceArtifactId)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public int CreateObjectType(int sourceWorkspaceArtifactTypeId)
		{
			var objectType = new ObjectType(SourceJobDTO.ObjectTypeGuid)
			{
				Name = Contracts.Constants.SPECIAL_SOURCEJOB_FIELD_NAME,
				ParentArtifactTypeID = sourceWorkspaceArtifactTypeId,
				CopyInstancesOnParentCopy = false,
				CopyInstancesOnWorkspaceCreation = false,
				SnapshotAuditingEnabledOnDelete = false,
				Pivot = true,
				Sampling = false,
				PersistentLists = false,
			};

			WriteResultSet<ObjectType> resultSet = null;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				try
				{
					resultSet = rsapiClient.Repositories.ObjectType.Create(new[] {objectType});
				}
				catch (Exception e)
				{
					throw new Exception($"Unable to create new Source Job object type: {resultSet?.Message}", e);
				}
			}

			if (!resultSet.Success || !resultSet.Results.Any())
			{
				throw new Exception($"Unable to create new Source Job object type: {resultSet.Message}");
			}

			return resultSet.Results.First().Artifact.ArtifactID;
		}

		public int Create(int sourceJobArtifactTypeId, SourceJobDTO sourceJobDto)
		{
			var fields = new List<FieldValue>()
			{
				new FieldValue(Contracts.Constants.SOURCEJOB_NAME_FIELD_NAME, sourceJobDto.Name),
				new FieldValue(Contracts.Constants.SOURCEJOB_JOBHISTORYID_FIELD_NAME, sourceJobDto.JobHistoryArtifactId),
				new FieldValue(Contracts.Constants.SOURCEJOB_JOBHISTORYNAME_FIELD_NAME, sourceJobDto.JobHistoryName)
			};

			var parentArtifact = new kCura.Relativity.Client.DTOs.Artifact(sourceJobDto.SourceWorkspaceArtifactId);
			var rdo = new RDO()
			{
				ParentArtifact = parentArtifact,
				ArtifactTypeID = sourceJobArtifactTypeId,
				Fields = fields
			};

			int rdoArtifactId;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				try
				{
					rdoArtifactId = rsapiClient.Repositories.RDO.CreateSingle(rdo);
				}
				catch (Exception e)
				{
					throw new Exception($"Unable to create new instance of Source Job: {e.Message}", e);
				}
			}

			return rdoArtifactId;
		}

		public IDictionary<Guid, int> CreateObjectTypeFields(int sourceJobArtifactTypeId, IEnumerable<Guid> fieldGuids)
		{
			var objectType = new ObjectType() { DescriptorArtifactTypeID = sourceJobArtifactTypeId };

			var jobHistoryFields = new List<kCura.Relativity.Client.DTOs.Field>()
			{
				new kCura.Relativity.Client.DTOs.Field()
				{
					Name = Contracts.Constants.SOURCEJOB_JOBHISTORYID_FIELD_NAME,
					Guids = new List<Guid>() { SourceJobDTO.Fields.JobHistoryIdFieldGuid },
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
					Name = Contracts.Constants.SOURCEJOB_JOBHISTORYNAME_FIELD_NAME,
					Guids = new List<Guid>() { SourceJobDTO.Fields.JobHistoryNameFieldGuid },
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

			ResultSet<kCura.Relativity.Client.DTOs.Field> newFieldResultSet = null;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				WriteResultSet<kCura.Relativity.Client.DTOs.Field> fieldWriteResultSet = null;

				try
				{
					rsapiClient.Repositories.Field.Create(fieldsToCreate);
				}
				catch (Exception e)
				{
					throw new Exception($"Unable to create fields for the Source Job object type:  {e.Message}", e);					
				}

				if (!fieldWriteResultSet.Success)
				{
					throw new Exception($"Unable to create fields for the Source Job object type:  {fieldWriteResultSet.Message}");
				}

				int[] newFieldIds = fieldWriteResultSet.Results.Select(x => x.Artifact.ArtifactID).ToArray();

				newFieldResultSet = rsapiClient.Repositories.Field.Read(newFieldIds);

				if (!newFieldResultSet.Success)
				{
					try
					{
						rsapiClient.Repositories.Field.Delete(fieldsToCreate);
					}
					catch (Exception e)
					{
						throw new Exception($"Unable to create fields for the Source Job object type: Failed to retrieve field after creation: Unable to delete field upon failure{newFieldResultSet.Message}", e);
					}

					throw new Exception("Unable to create fields for the Source Job object type: Failed to retrieve after creation: " +
					                    newFieldResultSet.Message);
				}
			}

			IDictionary<Guid, int> guidToIdDictionary = newFieldResultSet.Results.ToDictionary(
				x =>
				{
					switch (x.Artifact.Name)
					{
						case Contracts.Constants.SOURCEJOB_JOBHISTORYID_FIELD_NAME:
							return SourceJobDTO.Fields.JobHistoryIdFieldGuid;

						case Contracts.Constants.SOURCEJOB_JOBHISTORYNAME_FIELD_NAME:
							return SourceJobDTO.Fields.JobHistoryNameFieldGuid;

						default:
							throw new Exception("Unexpected fields returned");
					}
				},
				y => y.Artifact.ArtifactID);

			return guidToIdDictionary;
		}

		public int CreateSourceJobFieldOnDocument(int sourceJobArtifactTypeId)
		{
			var documentObjectType = new ObjectType() { DescriptorArtifactTypeID = 10 };
			var jobHistoryObjectType = new ObjectType() { DescriptorArtifactTypeID = sourceJobArtifactTypeId };
			var fields = new List<kCura.Relativity.Client.DTOs.Field>()
			{
				new kCura.Relativity.Client.DTOs.Field()
				{
					Name = Contracts.Constants.SPECIAL_SOURCEJOB_FIELD_NAME,
					FieldTypeID = kCura.Relativity.Client.FieldType.MultipleObject,
					ObjectType = documentObjectType,
					AssociativeObjectType = jobHistoryObjectType,
					AllowGroupBy = false,
					AllowPivot = false,
					AvailableInFieldTree = false,
					IsRequired = false,
					Width = "100",
				}
			};

			WriteResultSet<kCura.Relativity.Client.DTOs.Field> resultSet = null;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				try
				{
					resultSet = rsapiClient.Repositories.Field.Create(fields);
				}
				catch (Exception e)
				{
					throw new Exception($"Unable to create Source Job field on Document: {e.Message}", e);
				}
			}

			Result<kCura.Relativity.Client.DTOs.Field> field = resultSet.Results.FirstOrDefault();
			if (!resultSet.Success || field == null)
			{
				throw new Exception($"Unable to create Source Job field on Document: {resultSet.Message}");
			}

			int newFieldArtifactId = field.Artifact.ArtifactID;

			return newFieldArtifactId;
		}
	}
}