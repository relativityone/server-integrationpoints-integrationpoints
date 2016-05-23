using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Core;
using Relativity.Data;
using Relativity.Services.Field;
using Relativity.Services.Search;
using Relativity.Services.User;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class JobHistoryErrorRepository : RelativityMassEditBase, IJobHistoryErrorRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;
		private readonly IGenericLibrary<JobHistoryError> _jobHistoryErrorLibrary;
		private readonly IDtoTransformer<JobHistoryErrorDTO, JobHistoryError> _dtoTransformer;

		/// <summary>
		/// Internal due to Factory and Unit Tests
		/// </summary>
		internal JobHistoryErrorRepository(IHelper helper, 
			IGenericLibrary<JobHistoryError> jobHistoryErrorLibrary,
			IDtoTransformer<JobHistoryErrorDTO, JobHistoryError> dtoTransformer,
			int workspaceArtifactId)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
			_jobHistoryErrorLibrary = jobHistoryErrorLibrary;
			_dtoTransformer = dtoTransformer;
		}

		public List<int> RetrieveJobHistoryErrorArtifactIds(int jobHistoryArtifactId, Relativity.Client.Choice errorType)
		{
			var fields = new List<FieldValue>
			{
				new FieldValue(Guid.Parse(JobHistoryErrorDTO.FieldGuids.ArtifactId))
			};

			QueryResultSet<RDO> results = RetrieveJobHistoryErrorData(jobHistoryArtifactId, errorType, fields);
			
			return results.Results.Select(result => result.Artifact.ArtifactID).ToList();
		}

		public Dictionary<int, string> RetrieveJobHistoryErrorIdsAndSourceUniqueIds(int jobHistoryArtifactId, Relativity.Client.Choice errorType)
		{
			var fields = new List<FieldValue>
			{
				new FieldValue(Guid.Parse(JobHistoryErrorDTO.FieldGuids.ArtifactId)),
				new FieldValue(Guid.Parse(JobHistoryErrorDTO.FieldGuids.SourceUniqueID))
			};

			QueryResultSet<RDO> results = RetrieveJobHistoryErrorData(jobHistoryArtifactId, errorType, fields);

			Dictionary<int, string> artifactIdsAndSourceUniqueIds = new Dictionary<int, string>();

			foreach (var result in results.Results)
			{
				artifactIdsAndSourceUniqueIds.Add(result.Artifact.ArtifactID, result.Artifact.Fields[0].Value.ToString());
			}
			
			return artifactIdsAndSourceUniqueIds;
		}

		private QueryResultSet<RDO> RetrieveJobHistoryErrorData(int jobHistoryArtifactId, Relativity.Client.Choice errorType, List<FieldValue> fields)
		{
			QueryResultSet<RDO> results = null;
			var query = new Query<RDO>();

			query.ArtifactTypeGuid = new Guid(ObjectTypeGuids.JobHistoryError);
			var jobHistoryCondition = new WholeNumberCondition(new Guid(JobHistoryErrorDTO.FieldGuids.JobHistory), NumericConditionEnum.EqualTo, jobHistoryArtifactId);
			var errorTypeCondition = new SingleChoiceCondition(new Guid(JobHistoryErrorDTO.FieldGuids.ErrorType), SingleChoiceConditionEnum.AnyOfThese, errorType.ArtifactGuids);
			query.Condition = new CompositeCondition(jobHistoryCondition, CompositeConditionEnum.And, errorTypeCondition);
			query.Fields = fields;

			try
			{
				using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
				{
					rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
					results = rsapiClient.Repositories.RDO.Query(query);
				}
			}
			catch (Exception ex)
			{
				throw new Exception(System.String.Format(JobHistoryErrorErrors.JOB_HISTORY_ERROR_RETRIEVE_FAILURE, jobHistoryArtifactId), ex);
			}

			if (!results.Success)
			{
				throw new Exception(System.String.Format(JobHistoryErrorErrors.JOB_HISTORY_ERROR_RETRIEVE_NO_RESULTS, jobHistoryArtifactId, results.Message));
			}

			return results;
		}


		public void UpdateErrorStatuses(ClaimsPrincipal claimsPrincipal, int numberOfErrors, int jobHistoryErrorTypeId, int sourceWorkspaceId, int errorStatusArtifactId, string tableName)
		{
			if (numberOfErrors <= 0)
			{
				return;
			}

			BaseServiceContext baseService = claimsPrincipal.GetUnversionContext(sourceWorkspaceId);

			Guid[] guids = { new Guid(JobHistoryErrorFieldGuids.ErrorStatus) };
			DataRowCollection fieldRows;
			try
			{
				fieldRows = FieldQuery.RetrieveAllByGuids(baseService.ChicagoContext.DBContext, guids).Table.Rows;
			}
			catch (Exception ex)
			{
				throw new Exception(MassEditErrors.JOB_HISTORY_ERROR_STATUS_QUERY_ERROR, ex);
			}

			if (fieldRows.Count == 0)				
			{
				throw new Exception(MassEditErrors.JOB_HISTORY_ERROR_STATUS_EXISTENCE_ERROR);
			}

			global::Relativity.Query.ArtifactType jobHistoryErrorArtifactType = new global::Relativity.Query.ArtifactType(jobHistoryErrorTypeId, JobHistoryErrorDTO.TableName);

			global::Relativity.Core.DTO.Field singleChoiceField = new global::Relativity.Core.DTO.Field(baseService, fieldRows[0]);

			try
			{
				base.UpdateSingleChoiceField(baseService, singleChoiceField, numberOfErrors, jobHistoryErrorArtifactType, errorStatusArtifactId, tableName);
			}
			catch (Exception e)
			{
				throw new Exception(MassEditErrors.JOB_HISTORY_ERROR_MASS_EDIT_FAILURE, e);
			}

		}

		public int CreateItemLevelErrorsSavedSearch(int workspaceArtifactId, int integrationPointArtifactId, int savedSearchArtifactId, int jobHistoryArtifactId, int userArtifactId)
		{
			//Check for all documents that are part of the current saved search
			FieldRef savedSearchFieldRef = new FieldRef("(Saved Search)");
			Criteria savedSearchCriteria = new Criteria
			{
				Condition = new CriteriaCondition(savedSearchFieldRef, CriteriaConditionEnum.In, savedSearchArtifactId),
				BooleanOperator = BooleanOperatorEnum.And
			};

			//Check that the documents have not been tagged with the last Job History Object (meaning the job finished for them)
			FieldRef jobHistoryFieldRef = new FieldRef(JobHistoryErrorFields.JobHistory);
			Criteria jobHistoryArtifactIdCriteria = new Criteria
			{
				Condition = new CriteriaCondition(jobHistoryFieldRef, CriteriaConditionEnum.AnyOfThese, new[] { jobHistoryArtifactId }) { NotOperator = true }
			};
			CriteriaCollection jobHistoryObjectCriteriaCollection = new CriteriaCollection
			{
				Conditions = new List<CriteriaBase>(1) { jobHistoryArtifactIdCriteria }
			};
			Criteria jobHistoryCriteria = new Criteria
			{
				Condition = new CriteriaCondition(jobHistoryFieldRef, CriteriaConditionEnum.In, jobHistoryObjectCriteriaCollection)
			};

			CriteriaCollection searchCondition = new CriteriaCollection
			{
				Conditions = new List<CriteriaBase>(2) { savedSearchCriteria, jobHistoryCriteria }
			};

			KeywordSearch itemLevelSearch = new KeywordSearch
			{
				Owner = new UserRef(userArtifactId),
				Name = $"{Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_SAVED_SEARCH_NAME} - {integrationPointArtifactId} - {jobHistoryArtifactId}",
				ArtifactTypeID = (int)Relativity.Client.ArtifactType.Document,
				SearchCriteria = searchCondition
			};

			using (IKeywordSearchManager searchManager = _helper.GetServicesManager().CreateProxy<IKeywordSearchManager>(ExecutionIdentity.System))
			{
				SearchResultViewFields fields = searchManager.GetFieldsForSearchResultViewAsync(workspaceArtifactId, (int)Relativity.Client.ArtifactType.Document)
					.ConfigureAwait(false).GetAwaiter().GetResult();

				FieldRef field = fields.FieldsNotIncluded.First(x => x.Name == "Artifact ID");
				itemLevelSearch.Fields = new List<FieldRef>(1) { field };
				
				int itemLevelSearchArtifactId =
					searchManager.CreateSingleAsync(workspaceArtifactId, itemLevelSearch)
						.ConfigureAwait(false).GetAwaiter().GetResult();

				return itemLevelSearchArtifactId;
			}
		}

		public void DeleteItemLevelErrorsSavedSearch(int workspaceArtifactId, int searchArtifactId, int retryAttempts)
		{
			using (IKeywordSearchManager searchManager = _helper.GetServicesManager().CreateProxy<IKeywordSearchManager>(ExecutionIdentity.System))
			{
				var task = searchManager.DeleteSingleAsync(workspaceArtifactId, searchArtifactId);
				task.ConfigureAwait(false).GetAwaiter().GetResult();

				if (task.IsFaulted && retryAttempts < 3)
				{
					DeleteItemLevelErrorsSavedSearch(workspaceArtifactId, searchArtifactId, retryAttempts + 1);
				}
			}
		}

		public List<JobHistoryErrorDTO> Read(IEnumerable<int> artifactIds)
		{
			List<JobHistoryError> jobHistories = _jobHistoryErrorLibrary.Read(artifactIds);
			return _dtoTransformer.ConvertToDto(jobHistories);
		}
	}
}