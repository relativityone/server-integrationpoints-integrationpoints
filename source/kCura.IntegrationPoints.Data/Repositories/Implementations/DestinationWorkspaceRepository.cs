using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Domain;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Core;
using Relativity.Data;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class DestinationWorkspaceRepository : RelativityMassEditBase, IDestinationWorkspaceRepository
	{
		private readonly IHelper _helper;
		private readonly int _sourceWorkspaceArtifactId;
		private const string _DESTINATION_WORKSPACE_JOB_HISTORY_LINK = "20A24C4E-55E8-4FC2-ABBE-F75C07FAD91B";

		public DestinationWorkspaceRepository(IHelper helper, int sourceWorkspaceArtifactId)
		{
			_helper = helper;
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
		}

		public DestinationWorkspaceDTO Query(int targetWorkspaceArtifactId)
		{
			var query = new Query<RDO>();
			query.ArtifactTypeGuid = new Guid(DestinationWorkspaceDTO.Fields.OBJECT_TYPE_GUID);
			query.Condition = new ObjectCondition(new Guid(DestinationWorkspaceDTO.Fields.DESTINATION_WORKSPACE_ARTIFACT_ID), ObjectConditionEnum.EqualTo, targetWorkspaceArtifactId);
			query.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceDTO.Fields.DESTINATION_WORKSPACE_NAME)));

			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _sourceWorkspaceArtifactId;

				ResultSet<RDO> results = null;
				try
				{
					 results = rsapiClient.Repositories.RDO.Query(query);
				}
				catch (Exception e)
				{
					throw new Exception(RSAPIErrors.QUERY_DEST_WORKSPACE_ERROR, e);
				}

				if (results.Success && results.Results.Count > 0)
				{
					DestinationWorkspaceDTO destinationWorkspace = new DestinationWorkspaceDTO()
					{
						ArtifactId = results.Results[0].Artifact.ArtifactID,
						WorkspaceArtifactId = targetWorkspaceArtifactId,
						WorkspaceName = results.Results[0].Artifact.Fields[0].Value.ToString(),
					};

					return destinationWorkspace;
				}
			}

			return null;
		}

		public DestinationWorkspaceDTO Create(int targetWorkspaceArtifactId, string targetWorkspaceName)
		{
			string instanceName = Utils.GetFormatForWorkspaceOrJobDisplay(targetWorkspaceName, targetWorkspaceArtifactId);

			RDO destinationWorkspaceObject = new RDO();

			destinationWorkspaceObject.ArtifactTypeGuids.Add(new Guid(DestinationWorkspaceDTO.Fields.OBJECT_TYPE_GUID));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceDTO.Fields.DESTINATION_WORKSPACE_ARTIFACT_ID), targetWorkspaceArtifactId));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceDTO.Fields.DESTINATION_WORKSPACE_NAME), targetWorkspaceName));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceDTO.Fields.DESTINATION_WORKSPACE_INSTANCE_NAME), instanceName));

			WriteResultSet<RDO> results;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _sourceWorkspaceArtifactId;

				try
				{
					results = rsapiClient.Repositories.RDO.Create(destinationWorkspaceObject);
				}
				catch (Exception e)
				{
					throw new Exception(RSAPIErrors.CREATE_DEST_WORKSPACE_ERROR, e);
				}
			}

			if (results.Success && results.Results.Count > 0)
			{
				return new DestinationWorkspaceDTO()
				{
					ArtifactId = results.Results[0].Artifact.ArtifactID,
					WorkspaceArtifactId = targetWorkspaceArtifactId,
					WorkspaceName = targetWorkspaceName,
				};
			}

			throw new Exception(RSAPIErrors.CREATE_DEST_WORKSPACE_ERROR);
		}

		public void Update(DestinationWorkspaceDTO destinationWorkspace)
		{
			int workspaceId = destinationWorkspace.WorkspaceArtifactId;
			string workspaceName = destinationWorkspace.WorkspaceName;
			string instanceName = Utils.GetFormatForWorkspaceOrJobDisplay(workspaceName, workspaceId);

			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _sourceWorkspaceArtifactId;

				RDO destinationWorkspaceObject = null;
				try
				{
					destinationWorkspaceObject = rsapiClient.Repositories.RDO.ReadSingle(destinationWorkspace.ArtifactId);
				}
				catch (Exception e)
				{
					throw new Exception($"{RSAPIErrors.UPDATE_DEST_WORKSPACE_ERROR}: Unable to retrieve Destination Workspace instance", e);
				}

				destinationWorkspaceObject.ArtifactTypeGuids.Add(new Guid(DestinationWorkspaceDTO.Fields.OBJECT_TYPE_GUID));
				destinationWorkspaceObject.Fields.Add(
					new FieldValue(new Guid(DestinationWorkspaceDTO.Fields.DESTINATION_WORKSPACE_ARTIFACT_ID), workspaceId));
				destinationWorkspaceObject.Fields.Add(
					new FieldValue(new Guid(DestinationWorkspaceDTO.Fields.DESTINATION_WORKSPACE_NAME), workspaceName));
				destinationWorkspaceObject.Fields.Add(
					new FieldValue(new Guid(DestinationWorkspaceDTO.Fields.DESTINATION_WORKSPACE_INSTANCE_NAME), instanceName));

				WriteResultSet<RDO> results;
				try
				{
					results = rsapiClient.Repositories.RDO.Update(destinationWorkspaceObject);
				}
				catch (Exception e)
				{
					throw new Exception(RSAPIErrors.UPDATE_DEST_WORKSPACE_ERROR, e);
				}

				if (!results.Success)
				{
					throw new Exception(RSAPIErrors.UPDATE_DEST_WORKSPACE_ERROR);
				}
			}
		}

		public void LinkDestinationWorkspaceToJobHistory(int destinationWorkspaceInstanceId, int jobHistoryInstanceId)
		{
			RDO jobHistoryObject = new RDO(jobHistoryInstanceId);
			jobHistoryObject.ArtifactTypeGuids.Add(new Guid(ObjectTypeGuids.JobHistory));

			FieldValueList<Relativity.Client.DTOs.Artifact> objectToLink = new FieldValueList<Relativity.Client.DTOs.Artifact>();
			objectToLink.Add(new Relativity.Client.DTOs.Artifact(destinationWorkspaceInstanceId));
			jobHistoryObject.Fields.Add(new FieldValue(new Guid(_DESTINATION_WORKSPACE_JOB_HISTORY_LINK), objectToLink));

			WriteResultSet<RDO> results;
			try
			{
				using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
				{
					rsapiClient.APIOptions.WorkspaceID = _sourceWorkspaceArtifactId;

					results = rsapiClient.Repositories.RDO.Update(jobHistoryObject);
				}
			}
			catch (Exception e)
			{
				throw new Exception(RSAPIErrors.LINK_OBJECT_INSTANCE_ERROR, e);
			}

			if (!results.Success)
			{
				throw new Exception(RSAPIErrors.LINK_OBJECT_INSTANCE_ERROR);
			}
		}

		public void TagDocsWithDestinationWorkspaceAndJobHistory(ClaimsPrincipal claimsPrincipal, int numberOfDocs, int destinationWorkspaceInstanceId, int jobHistoryInstanceId, string tableName, int sourceWorkspaceId)
		{
			global::Relativity.Query.ArtifactType artifactType = new global::Relativity.Query.ArtifactType(global::Relativity.ArtifactType.Document);

			if (numberOfDocs <= 0)
			{
				return;
			}

			BaseServiceContext baseService = claimsPrincipal.GetUnversionContext(sourceWorkspaceId);

			global::Relativity.Core.DTO.Field destinationWorkspaceField = GetFieldToEdit(baseService, DocumentMultiObjectFields.DESTINATION_WORKSPACE_FIELD);
			global::Relativity.Core.DTO.Field jobHistoryField = GetFieldToEdit(baseService, DocumentMultiObjectFields.JOB_HISTORY_FIELD);

			List<MassEditObject> massEditObjects = new List<MassEditObject>()
			{
				new MassEditObject()
				{
					FieldGuid = DocumentMultiObjectFields.DESTINATION_WORKSPACE_FIELD,
					FieldToUpdate = destinationWorkspaceField,
					ObjectToLinkTo = destinationWorkspaceInstanceId
				},
				new MassEditObject()
				{
					FieldGuid = DocumentMultiObjectFields.JOB_HISTORY_FIELD,
					FieldToUpdate = jobHistoryField,
					ObjectToLinkTo = jobHistoryInstanceId
				}
			};

			try
			{
				base.TagFieldsWithRdo(baseService, massEditObjects, numberOfDocs, artifactType, tableName);
			}
			catch (Exception e)
			{
				throw new Exception(MassEditErrors.SOURCE_OBJECT_MASS_EDIT_FAILURE, e);
			}
		}

		private global::Relativity.Core.DTO.Field GetFieldToEdit(BaseServiceContext baseServiceContext, string fieldGuid)
		{
			Guid[] guids =
			{
				new Guid(fieldGuid)
			};

			DataRowCollection fieldRows;
			try
			{
				fieldRows = FieldQuery.RetrieveAllByGuids(baseServiceContext.ChicagoContext.DBContext, guids).Table.Rows;
			}
			catch (Exception ex)
			{
				throw new Exception(MassEditErrors.SOURCE_OBJECT_MO_QUERY_ERROR, ex);
			}

			if (fieldRows.Count == 0)
			{
				throw new Exception(MassEditErrors.SOURCE_OBJECT_MO_EXISTENCE_ERROR);
			}

			return new global::Relativity.Core.DTO.Field(baseServiceContext, fieldRows[0]);
		}
	}
}