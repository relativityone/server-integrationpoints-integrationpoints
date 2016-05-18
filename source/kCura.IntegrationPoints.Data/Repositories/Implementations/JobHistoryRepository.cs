using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Core;
using Relativity.Data;
using Field = Relativity.Core.DTO.Field;
using ArtifactType = Relativity.Query.ArtifactType;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class JobHistoryRepository : RelativityMassEditBase, IJobHistoryRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;

		internal JobHistoryRepository(IHelper helper, int workspaceArtifactId)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public void TagDocsWithJobHistory(ClaimsPrincipal claimsPrincipal, int numberOfDocs, int jobHistoryInstanceArtifactId, int sourceWorkspaceId, string tableSuffix)
		{
			ArtifactType artifactType = new ArtifactType(global::Relativity.ArtifactType.Document);

			if (numberOfDocs <= 0)
			{
				return;
			}

			BaseServiceContext baseService = claimsPrincipal.GetUnversionContext(sourceWorkspaceId);

			Guid[] guids = { new Guid(DocumentMultiObjectFields.JOB_HISTORY_FIELD) };
			DataRowCollection fieldRows;
			try
			{
				fieldRows = FieldQuery.RetrieveAllByGuids(baseService.ChicagoContext.DBContext, guids).Table.Rows;
			}
			catch (Exception ex)
			{
				throw new Exception(MassEditErrors.JOB_HISTORY_MO_QUERY_ERROR, ex);
			}

			if (fieldRows.Count == 0)
			{
				throw new Exception(MassEditErrors.JOB_HISTORY_MO_EXISTENCE_ERROR);
			}

			Field multiObjectField = new Field(baseService, fieldRows[0]);
			string fullTableName = $"{Constants.TEMPORARY_DOC_TABLE_JOB_HIST}_{tableSuffix}";
			try
			{
				base.TagFieldsWithRdo(baseService, multiObjectField, numberOfDocs, artifactType, jobHistoryInstanceArtifactId, fullTableName);
			}
			catch (Exception e)
			{
				throw new Exception(MassEditErrors.JOB_HISTORY_MASS_EDIT_FAILURE, e);
			}
		}

		public List<int> GetLastTwoJobHistoryArtifactId(int integrationPointArtifactId)
		{
			var query = new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.JobHistory),
				Condition = new ObjectsCondition(new Guid(JobHistoryFieldGuids.IntegrationPoint), ObjectsConditionEnum.AnyOfThese, new List<int>(){integrationPointArtifactId}),
				Fields = new List<FieldValue>()
				{
					new FieldValue(new Guid(JobHistoryFieldGuids.IntegrationPoint))
				},
				Sorts = new List<Sort>()
				{
					new Sort()
					{
						Field = "ArtifactID",
						Direction = SortEnum.Descending
					}
				}
			};

			QueryResultSet<RDO> results = null;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
				results = rsapiClient.Repositories.RDO.Query(query, 2);
			}

			if (!results.Success)
			{
				throw new Exception($"Unable to retrieve Job Hisory: {results.Message}");
			}

			List<int> lastJobHistoryArtifactId = results.Results.Select(result => result.Artifact.ArtifactID).ToList();
			return lastJobHistoryArtifactId;
		}
	}
}