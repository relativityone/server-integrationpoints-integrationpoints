using System;
using System.Data;
using System.Security.Claims;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Authentication;
using Relativity.Data;
using Field = Relativity.Core.DTO.Field;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class DestinationWorkspaceRepository : RelativityMassEditBase, IDestinationWorkspaceRepository
	{
		private readonly IHelper _helper;
		private readonly int _targetWorkspaceArtifactId;
		private readonly IWorkspaceRepository _workspaceRepository;
		private readonly int _sourceWorkspaceArtifactId;
		private const string _DESTINATION_WORKSPACE_JOB_HISTORY_LINK = "20A24C4E-55E8-4FC2-ABBE-F75C07FAD91B";

		public DestinationWorkspaceRepository(IHelper helper, IWorkspaceRepository workspaceRepository, int sourceWorkspaceArtifactId, int targetWorkspaceArtifactId)
		{
			_helper = helper;
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_targetWorkspaceArtifactId = targetWorkspaceArtifactId;
			_workspaceRepository = workspaceRepository;
		}

		public int? QueryDestinationWorkspaceRdoInstance()
		{
			var query = new Query<RDO>();
			query.ArtifactTypeGuid = new Guid(DestinationWorkspaceObject.OBJECT_TYPE_GUID);
			query.Condition = new ObjectCondition(new Guid(DestinationWorkspaceObject.DESTINATION_WORKSPACE_ARTIFACT_ID), ObjectConditionEnum.EqualTo, _targetWorkspaceArtifactId);

			try
			{
				using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
				{
					rsapiClient.APIOptions.WorkspaceID = _sourceWorkspaceArtifactId;

					ResultSet<RDO> results = rsapiClient.Repositories.RDO.Query(query);

					if (results.Success && results.Results.Count > 0)
					{
						return results.Results[0].Artifact.ArtifactID;
					}
				}

				return -1;
			}
			catch (Exception e)
			{
				throw new Exception(RSAPIErrors.QUERY_DEST_WORKSPACE_ERROR, e);
			}
		}

		public int? CreateDestinationWorkspaceRdoInstance()
		{
			string destinationWorkspaceName = _workspaceRepository.Retrieve(_targetWorkspaceArtifactId).Name;
			string instanceName = $"{destinationWorkspaceName} - {_targetWorkspaceArtifactId}";

			RDO destinationWorkspaceObject = new RDO();

			destinationWorkspaceObject.ArtifactTypeGuids.Add(new Guid(DestinationWorkspaceObject.OBJECT_TYPE_GUID));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceObject.DESTINATION_WORKSPACE_ARTIFACT_ID), _targetWorkspaceArtifactId));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceObject.DESTINATION_WORKSPACE_NAME), destinationWorkspaceName));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceObject.DESTINATION_WORKSPACE_INSTANCE_NAME), instanceName));

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
				return results.Results[0].Artifact.ArtifactID;
			}

			throw new Exception(RSAPIErrors.CREATE_DEST_WORKSPACE_ERROR);
		}

		public void LinkDestinationWorkspaceToJobHistory(int? destinationWorkspaceInstanceId, int jobHistoryInstanceId)
		{
			RDO jobHistoryObject = new RDO(jobHistoryInstanceId);
			jobHistoryObject.ArtifactTypeGuids.Add(new Guid(ObjectTypeGuids.JobHistory));

			FieldValueList<Relativity.Client.DTOs.Artifact> objectToLink = new FieldValueList<Relativity.Client.DTOs.Artifact>();
			objectToLink.Add(new Relativity.Client.DTOs.Artifact(destinationWorkspaceInstanceId ?? default(int)));
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

		public void TagDocsWithDestinationWorkspace(int numberOfDocs, int? destinationWorkspaceInstanceId, string tableSuffix, int sourceWorkspaceId)
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

			Field multiObjectField = new Field(baseService, fieldRows[0]);
			string fullTableName = $"{Constants.TEMPORARY_DOC_TABLE_DEST_WS}_{tableSuffix}";

			try
			{
				base.TagDocumentsWithRdo(baseService, multiObjectField, numberOfDocs, destinationWorkspaceInstanceId.Value, fullTableName);
			}
			catch (Exception e)
			{
				throw new Exception(MassEditErrors.DEST_WORKSPACE_MASS_EDIT_FAILURE, e);
			}
		}
	}
}