using System;
using System.Collections.Generic;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class DestinationWorkspaceRepository : IDestinationWorkspaceRepository
	{
		private readonly IRSAPIClient _client;
		private readonly int _destinationWorkspaceId;
		private readonly IWorkspaceRepository _workspaceRepository;

		public DestinationWorkspaceRepository(IRSAPIClient client, IWorkspaceRepository workspaceRepository, int destinationWorkspaceId)
		{
			_client = client;
			_destinationWorkspaceId = destinationWorkspaceId;
			_workspaceRepository = workspaceRepository;
		}

		public int QueryDestinationWorkspaceRdoInstance()
		{
			 Query<RDO> query = new Query<RDO>();
			 query.ArtifactTypeGuid = new Guid(DestinationWorkspaceObject.OBJECT_TYPE_GUID);
			 query.Condition = new ObjectCondition(new Guid(DestinationWorkspaceObject.DESTINATION_WORKSPACE_ARTIFACT_ID), ObjectConditionEnum.EqualTo, _destinationWorkspaceId);

			try
			{
				ResultSet<RDO> results = _client.Repositories.RDO.Query(query);

				if (results.Results.Count > 0)
				{
					return results.Results[0].Artifact.ArtifactID;
				}

				return -1;
			}
			catch (Exception e)
			{
				throw new Exception(DestinationWorkspaceErrors.QUERY_ERROR, e);
			}
		}

		public int CreateDestinationWorkspaceRdoInstance(List<int> documentIds)
		{
			string destinationWorkspaceName = _workspaceRepository.Retrieve(_destinationWorkspaceId).Name;
			string instanceName = destinationWorkspaceName + " - " + _destinationWorkspaceId; 

			RDO destinationWorkspaceObject = new RDO();

			var objectsToLink = new FieldValueList<Relativity.Client.DTOs.Artifact>();
			foreach (int docId in documentIds)
			{
				objectsToLink.Add(new Relativity.Client.DTOs.Artifact(docId));
			}

			destinationWorkspaceObject.ArtifactTypeGuids.Add(new Guid(DestinationWorkspaceObject.OBJECT_TYPE_GUID));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceObject.DESTINATION_WORKSPACE_ARTIFACT_ID), _destinationWorkspaceId));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceObject.DESTINATION_WORKSPACE_NAME), destinationWorkspaceName));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceObject.DESTINATION_WORKSPACE_INSTANCE_NAME), instanceName));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceObject.DESTINATION_WORKSPACE_DOCUMENTS), objectsToLink));

			WriteResultSet<RDO> results;
			try
			{
				results = _client.Repositories.RDO.Create(destinationWorkspaceObject);
			}
			catch (Exception e)
			{
				throw new Exception(DestinationWorkspaceErrors.CREATE_ERROR, e);
			}

			if (!results.Success || results.Results.Count == 0)
			{
				throw new Exception(DestinationWorkspaceErrors.CREATE_ERROR);
			}

			return results.Results[0].Artifact.ArtifactID;
		}

		public void UpdateDestinationWorkspaceRdoInstance(List<int> documentIds, int destinationWorkspaceArtifactId, ref FieldValueList<Relativity.Client.DTOs.Artifact> existingMultiObjectLinks, bool initialReadDone = false)
		{
			RDO destinationWorkspaceObject = new RDO(destinationWorkspaceArtifactId);
			destinationWorkspaceObject.ArtifactTypeGuids.Add(new Guid(DestinationWorkspaceObject.OBJECT_TYPE_GUID));
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceObject.DESTINATION_WORKSPACE_DOCUMENTS)));

			//read existing MO Field Values
			if (!initialReadDone)
			{
				existingMultiObjectLinks = ReadExistingMultiObjectLinks(destinationWorkspaceObject);
			}
			
			foreach (int docId in documentIds)
			{
				existingMultiObjectLinks.Add(new Relativity.Client.DTOs.Artifact(docId));
			}

			//update MO Field Values
			destinationWorkspaceObject.Fields.Add(new FieldValue(new Guid(DestinationWorkspaceObject.DESTINATION_WORKSPACE_DOCUMENTS), existingMultiObjectLinks));

			WriteResultSet<RDO> results;
			try
			{
				results = _client.Repositories.RDO.Update(destinationWorkspaceObject);
			}
			catch (Exception e)
			{
				throw new Exception(DestinationWorkspaceErrors.UPDATE_ERROR, e);
			}

			if (!results.Success)
			{
				throw new Exception(DestinationWorkspaceErrors.UPDATE_ERROR);
			}
		}

		internal FieldValueList<Relativity.Client.DTOs.Artifact> ReadExistingMultiObjectLinks(RDO destinationWorkspaceObject)
		{
			ResultSet<RDO> dwResults = _client.Repositories.RDO.Read(destinationWorkspaceObject);
			RDO resultsObject = dwResults.Results[0].Artifact;

			FieldValueList<Relativity.Client.DTOs.Artifact> existingMultiObjectLinks = resultsObject[new Guid(DestinationWorkspaceObject.DESTINATION_WORKSPACE_DOCUMENTS)].GetValueAsMultipleObject<kCura.Relativity.Client.DTOs.Artifact>();

			return existingMultiObjectLinks;
		}
	}
}
