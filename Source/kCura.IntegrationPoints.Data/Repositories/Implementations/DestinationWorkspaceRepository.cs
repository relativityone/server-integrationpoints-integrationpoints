using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.Core;
using Relativity.Data;
using Relativity.Services.Objects.DataContracts;
using ArtifactType = Relativity.Query.ArtifactType;
using Field = Relativity.Core.DTO.Field;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class DestinationWorkspaceRepository : RelativityMassEditBase, IDestinationWorkspaceRepository
	{
		private readonly IRSAPIService _rsapiService;

		public DestinationWorkspaceRepository(IRSAPIService rsapiService)
		{
			_rsapiService = rsapiService;
		}

		public DestinationWorkspace Query(int targetWorkspaceArtifactId, int? federatedInstanceArtifactId)
		{
			string instanceCondition;
			if (federatedInstanceArtifactId.HasValue)
			{
				instanceCondition = $"'{DestinationWorkspaceFields.DestinationInstanceArtifactID}' == {federatedInstanceArtifactId}";
			}
			else
			{
				instanceCondition = $"(NOT '{DestinationWorkspaceFields.DestinationInstanceArtifactID}' ISSET)";
			}
			
			try
			{
				var rdos = _rsapiService.RelativityObjectManager.Query<DestinationWorkspace>(new QueryRequest()
				{
					Condition = $"'{DestinationWorkspaceFields.DestinationWorkspaceArtifactID}' == {targetWorkspaceArtifactId} AND {instanceCondition}",
					Fields = new List<FieldRef>
					{
						new FieldRef { Name = "ArtifactId" },
						new FieldRef { Guid = new Guid(DestinationWorkspaceFieldGuids.DestinationWorkspaceName)},
						new FieldRef { Guid = new Guid(DestinationWorkspaceFieldGuids.DestinationInstanceName) },
						new FieldRef { Guid = new Guid(DestinationWorkspaceFieldGuids.DestinationWorkspaceArtifactID) },
						new FieldRef { Guid = new Guid(DestinationWorkspaceFieldGuids.DestinationInstanceArtifactID) },
						new FieldRef { Guid = new Guid(DestinationWorkspaceFieldGuids.Name) }
					}
				});

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
			string instanceName = Utils.GetFormatForWorkspaceOrJobDisplay(federatedInstanceName, targetWorkspaceName, targetWorkspaceArtifactId);

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
				int artifactId = _rsapiService.RelativityObjectManager.Create(destinationWorkspace);
				destinationWorkspace.ArtifactId = artifactId;
				return destinationWorkspace;
			}
			catch (Exception e) when (!(e is IntegrationPointsException))
			{
				throw new Exception(RSAPIErrors.CREATE_DEST_WORKSPACE_ERROR, e);
			}
		}

		public void Update(DestinationWorkspace destinationWorkspace)
		{
			string instanceName = Utils.GetFormatForWorkspaceOrJobDisplay(destinationWorkspace.DestinationInstanceName, destinationWorkspace.DestinationWorkspaceName,
				destinationWorkspace.DestinationWorkspaceArtifactID);
			destinationWorkspace.Name = instanceName;

			try
			{
				_rsapiService.RelativityObjectManager.Update(destinationWorkspace);
			}
			catch (Exception e)
			{
				throw new Exception($"{RSAPIErrors.UPDATE_DEST_WORKSPACE_ERROR}: Unable to retrieve Destination Workspace instance", e);
			}
		}


		public void LinkDestinationWorkspaceToJobHistory(int destinationWorkspaceInstanceId, int jobHistoryInstanceId)
		{
			var destinationWorkspaceObjectValue = new RelativityObjectValue
			{
				ArtifactID = destinationWorkspaceInstanceId
			};
			var fieldsToUpdate = new List<FieldRefValuePair>
			{
				new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = new Guid(JobHistoryFieldGuids.DestinationWorkspaceInformation)
					},
					Value = new [] { destinationWorkspaceObjectValue }
				}
			};

			bool isUpdated;
			try
			{
				isUpdated = _rsapiService.RelativityObjectManager.Update(jobHistoryInstanceId, fieldsToUpdate);
			}
			catch (Exception e)
			{
				throw new IntegrationPointsException(RSAPIErrors.LINK_OBJECT_INSTANCE_ERROR, e);
			}

			if (!isUpdated)
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