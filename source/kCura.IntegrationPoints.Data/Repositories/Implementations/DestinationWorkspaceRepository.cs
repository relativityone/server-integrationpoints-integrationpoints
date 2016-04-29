﻿using System;
using System.Data;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Authentication;
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

			try
			{
				using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
				{
					rsapiClient.APIOptions.WorkspaceID = _sourceWorkspaceArtifactId;

					ResultSet<RDO> results = rsapiClient.Repositories.RDO.Query(query);

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
			catch (Exception e)
			{
				throw new Exception(RSAPIErrors.QUERY_DEST_WORKSPACE_ERROR, e);
			}
		}

		public DestinationWorkspaceDTO Create(int targetWorkspaceArtifactId, string destinationWorkspaceName)
		{
			string instanceName = Utils.GetFormatForWorkspaceOrJobDisplay(destinationWorkspaceName, targetWorkspaceArtifactId);

			RDO destinationWorkspaceObject = new RDO();

			destinationWorkspaceObject.ArtifactTypeGuids.Add(new Guid(DestinationWorkspaceDTO.Fields.OBJECT_TYPE_GUID));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceDTO.Fields.DESTINATION_WORKSPACE_ARTIFACT_ID), targetWorkspaceArtifactId));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceDTO.Fields.DESTINATION_WORKSPACE_NAME), destinationWorkspaceName));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceDTO.Fields.DESTINATION_WORKSPACE_INSTANCE_NAME), instanceName));

			WriteResultSet<RDO> results;
			try
			{
				using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
				{
					rsapiClient.APIOptions.WorkspaceID = _sourceWorkspaceArtifactId;

					results = rsapiClient.Repositories.RDO.Create(destinationWorkspaceObject);
				}
			}
			catch (Exception e)
			{
				throw new Exception(RSAPIErrors.CREATE_DEST_WORKSPACE_ERROR, e);
			}

			if (results.Success && results.Results.Count > 0)
			{
				return new DestinationWorkspaceDTO()
				{
					ArtifactId = results.Results[0].Artifact.ArtifactID,
					WorkspaceArtifactId = targetWorkspaceArtifactId,
					WorkspaceName = destinationWorkspaceName,
				};
			}

			throw new Exception(RSAPIErrors.CREATE_DEST_WORKSPACE_ERROR);
		}

		public void Update(DestinationWorkspaceDTO destinationWorkspace)
		{
			int workspaceId = destinationWorkspace.WorkspaceArtifactId;
			string workspaceName = destinationWorkspace.WorkspaceName;
			string instanceName = Utils.GetFormatForWorkspaceOrJobDisplay(workspaceName, workspaceId);

			RDO destinationWorkspaceObject = null;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _sourceWorkspaceArtifactId;

				destinationWorkspaceObject = rsapiClient.Repositories.RDO.ReadSingle(destinationWorkspace.ArtifactId);
			}

			destinationWorkspaceObject.ArtifactTypeGuids.Add(new Guid(DestinationWorkspaceDTO.Fields.OBJECT_TYPE_GUID));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceDTO.Fields.DESTINATION_WORKSPACE_ARTIFACT_ID), workspaceId));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceDTO.Fields.DESTINATION_WORKSPACE_NAME), workspaceName));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceDTO.Fields.DESTINATION_WORKSPACE_INSTANCE_NAME), instanceName));

			WriteResultSet<RDO> results;
			try
			{
				using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
				{
					rsapiClient.APIOptions.WorkspaceID = _sourceWorkspaceArtifactId;

					results = rsapiClient.Repositories.RDO.Update(destinationWorkspaceObject);
				}
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

		public void TagDocsWithDestinationWorkspace(int numberOfDocs, int destinationWorkspaceInstanceId, string tableSuffix, int sourceWorkspaceId)
		{
			if (numberOfDocs <= 0)
			{
				return;
			}

			BaseServiceContext baseService = ClaimsPrincipal.Current.GetServiceContextUnversionShortTerm(sourceWorkspaceId);

			Guid[] guids = { new Guid(DocumentMultiObjectFields.DESTINATION_WORKSPACE_FIELD) };
			DataRowCollection fieldRows;
			try
			{
				fieldRows = FieldQuery.RetrieveAllByGuids(baseService.ChicagoContext.DBContext, guids).Table.Rows;
			}
			catch (Exception ex)
			{
				throw new Exception(MassEditErrors.DEST_WORKSPACE_MO_QUERY_ERROR, ex);
			}

			if (fieldRows.Count == 0)
			{
				throw new Exception(MassEditErrors.DEST_WORKSPACE_MO_EXISTENCE_ERROR);
			}

			var multiObjectField = new global::Relativity.Core.DTO.Field(baseService, fieldRows[0]);
			string fullTableName = $"{Constants.TEMPORARY_DOC_TABLE_DEST_WS}_{tableSuffix}";

			try
			{
				base.TagDocumentsWithRdo(baseService, multiObjectField, numberOfDocs, destinationWorkspaceInstanceId, fullTableName);
			}
			catch (Exception e)
			{
				throw new Exception(MassEditErrors.DEST_WORKSPACE_MASS_EDIT_FAILURE, e);
			}
		}
	}
}