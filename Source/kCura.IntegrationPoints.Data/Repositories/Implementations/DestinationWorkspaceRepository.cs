using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Domain;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Core;
using Relativity.Data;
using Artifact = kCura.Relativity.Client.DTOs.Artifact;
using ArtifactType = Relativity.Query.ArtifactType;
using Field = Relativity.Core.DTO.Field;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class DestinationWorkspaceRepository : RelativityMassEditBase, IDestinationWorkspaceRepository
	{
		private readonly IRSAPIService _rsapiService;
		private readonly IHelper _helper;
		private readonly int _sourceWorkspaceArtifactId;
		private const string _DESTINATION_WORKSPACE_JOB_HISTORY_LINK = "20A24C4E-55E8-4FC2-ABBE-F75C07FAD91B";

		public DestinationWorkspaceRepository(IHelper helper, int sourceWorkspaceArtifactId, IRSAPIService rsapiService)
		{
			_helper = helper;
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_rsapiService = rsapiService;
		}

		public DestinationWorkspace Query(int targetWorkspaceArtifactId, int? federatedInstanceArtifactId)
		{
			var query = new Query<RDO>();
			query.ArtifactTypeGuid = new Guid(ObjectTypeGuids.DestinationWorkspace);
			query.Fields = FieldValue.AllFields;

			var workspaceCondition = new ObjectCondition(new Guid(DestinationWorkspaceFieldGuids.DestinationWorkspaceArtifactID), ObjectConditionEnum.EqualTo, targetWorkspaceArtifactId);

			Condition federatedInstanceCondition;
			if (federatedInstanceArtifactId.HasValue)
			{
				federatedInstanceCondition = new ObjectCondition(new Guid(DestinationWorkspaceFieldGuids.DestinationInstanceArtifactID), ObjectConditionEnum.EqualTo,
					federatedInstanceArtifactId.Value);
			}
			else
			{
				federatedInstanceCondition = new NotCondition(new ObjectCondition(new Guid(DestinationWorkspaceFieldGuids.DestinationInstanceArtifactID), ObjectConditionEnum.IsSet));
				query.Condition = workspaceCondition;
			}

			query.Condition = new CompositeCondition(workspaceCondition, CompositeConditionEnum.And, federatedInstanceCondition);

			try
			{
				var rdos = _rsapiService.DestinationWorkspaceLibrary.Query(query);

				if (rdos.Count == 0)
				{
					return null;
				}

				return rdos[0];
			}
			catch (Exception e)
			{
				throw new Exception(RSAPIErrors.QUERY_DEST_WORKSPACE_ERROR, e);
			}
		}

		public DestinationWorkspace Create(int targetWorkspaceArtifactId, string targetWorkspaceName, int? federatedInstanceArtifactId, string federatedInstanceName)
		{
			string instanceName = Utils.GetFormatForWorkspaceOrJobDisplay(targetWorkspaceName, targetWorkspaceArtifactId);

			var destinationWorkspace = new DestinationWorkspace
			{
				DestinationWorkspaceArtifactID = targetWorkspaceArtifactId,
				DestinationWorkspaceName = targetWorkspaceName,
				DestinationInstanceName = federatedInstanceName,
				DestinationInstanceArtifactID = federatedInstanceArtifactId,
				Name = instanceName
			};


			try
			{
				var artifactId = _rsapiService.DestinationWorkspaceLibrary.Create(destinationWorkspace);
				destinationWorkspace.ArtifactId = artifactId;
				return destinationWorkspace;
			}
			catch (Exception e)
			{
				throw new Exception(RSAPIErrors.CREATE_DEST_WORKSPACE_ERROR, e);
			}
		}

		public void Update(DestinationWorkspace destinationWorkspace)
		{
			string instanceName = Utils.GetFormatForWorkspaceOrJobDisplay(destinationWorkspace.DestinationWorkspaceName,
				destinationWorkspace.DestinationWorkspaceArtifactID.Value);
			destinationWorkspace.Name = instanceName;

			try
			{
				_rsapiService.DestinationWorkspaceLibrary.Update(destinationWorkspace);
			}
			catch (Exception e)
			{
				throw new Exception($"{RSAPIErrors.UPDATE_DEST_WORKSPACE_ERROR}: Unable to retrieve Destination Workspace instance", e);
			}
		}


		public void LinkDestinationWorkspaceToJobHistory(int destinationWorkspaceInstanceId, int jobHistoryInstanceId)
		{
			RDO jobHistoryObject = new RDO(jobHistoryInstanceId);
			jobHistoryObject.ArtifactTypeGuids.Add(new Guid(ObjectTypeGuids.JobHistory));

			FieldValueList<Artifact> objectToLink = new FieldValueList<Artifact>();
			objectToLink.Add(new Artifact(destinationWorkspaceInstanceId));
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

		public void TagDocsWithDestinationWorkspaceAndJobHistory(ClaimsPrincipal claimsPrincipal, int numberOfDocs, int destinationWorkspaceInstanceId, int jobHistoryInstanceId,
			string tableName, int sourceWorkspaceId)
		{
			ArtifactType artifactType = new ArtifactType(global::Relativity.ArtifactType.Document);

			if (numberOfDocs <= 0)
			{
				return;
			}

			BaseServiceContext baseService = claimsPrincipal.GetUnversionContext(sourceWorkspaceId);

			Field destinationWorkspaceField = GetFieldToEdit(baseService, DocumentMultiObjectFields.DESTINATION_WORKSPACE_FIELD);
			Field jobHistoryField = GetFieldToEdit(baseService, DocumentMultiObjectFields.JOB_HISTORY_FIELD);

			List<MassEditObject> massEditObjects = new List<MassEditObject>
			{
				new MassEditObject
				{
					FieldGuid = DocumentMultiObjectFields.DESTINATION_WORKSPACE_FIELD,
					FieldToUpdate = destinationWorkspaceField,
					ObjectToLinkTo = destinationWorkspaceInstanceId
				},
				new MassEditObject
				{
					FieldGuid = DocumentMultiObjectFields.JOB_HISTORY_FIELD,
					FieldToUpdate = jobHistoryField,
					ObjectToLinkTo = jobHistoryInstanceId
				}
			};

			try
			{
				TagFieldsWithRdo(baseService, massEditObjects, numberOfDocs, artifactType, tableName);
			}
			catch (Exception e)
			{
				throw new Exception(MassEditErrors.SOURCE_OBJECT_MASS_EDIT_FAILURE, e);
			}
		}

		private Field GetFieldToEdit(BaseServiceContext baseServiceContext, string fieldGuid)
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

			return new Field(baseServiceContext, fieldRows[0]);
		}
	}
}