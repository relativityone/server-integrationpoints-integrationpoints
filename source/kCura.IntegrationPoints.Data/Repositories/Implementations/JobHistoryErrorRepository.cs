using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.API;
using Relativity.Core;
using Relativity.Data;
using Relativity.Services.Field;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class JobHistoryErrorRepository : KeplerServiceBase, IJobHistoryErrorRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;
		private readonly IGenericLibrary<JobHistoryError> _jobHistoryErrorLibrary;
		private readonly IDtoTransformer<JobHistoryErrorDTO, JobHistoryError> _dtoTransformer;

		/// <summary>
		/// Internal due to Factory and Unit Tests
		/// </summary>
		internal JobHistoryErrorRepository(IHelper helper,
			IObjectQueryManagerAdaptor objectQueryManagerAdaptor,
			IGenericLibrary<JobHistoryError> jobHistoryErrorLibrary,
			IDtoTransformer<JobHistoryErrorDTO, JobHistoryError> dtoTransformer,
			int workspaceArtifactId)
			: base(objectQueryManagerAdaptor)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
			_jobHistoryErrorLibrary = jobHistoryErrorLibrary;
			_dtoTransformer = dtoTransformer;
		}

		public IList<int> RetrieveJobHistoryErrorArtifactIds(int jobHistoryArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values errorType)
		{
			IEnumerable<JobHistoryErrorDTO> results = RetrieveJobHistoryErrorData(jobHistoryArtifactId, errorType);

			return results.Select(result => result.ArtifactId).ToList();
		}

		public IDictionary<int, string> RetrieveJobHistoryErrorIdsAndSourceUniqueIds(int jobHistoryArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values errorType)
		{

			IEnumerable<JobHistoryErrorDTO> results = RetrieveJobHistoryErrorData(jobHistoryArtifactId, errorType);

			Dictionary<int, string> artifactIdsAndSourceUniqueIds = new Dictionary<int, string>();

			foreach (var result in results)
			{
				artifactIdsAndSourceUniqueIds.Add(result.ArtifactId, result.SourceUniqueID);
			}
			
			return artifactIdsAndSourceUniqueIds;
		}

		private IEnumerable<JobHistoryErrorDTO> RetrieveJobHistoryErrorData(int jobHistoryArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values errorType)
		{
			IEnumerable<JobHistoryErrorDTO> jobHistoryErrors = new List<JobHistoryErrorDTO>();
			var jobHistoryCondition = $"'{JobHistoryErrorDTO.FieldNames.JobHistory}' == {jobHistoryArtifactId}";
			
			var query = new global::Relativity.Services.ObjectQuery.Query()
			{
				Fields = JobHistoryErrorDTO.FieldNames.FieldNamesList.ToArray(),
				Condition = jobHistoryCondition,
				TruncateTextFields = true
			};

			try
			{
				ArtifactDTO[] results = this.RetrieveAllArtifactsAsync(query).GetResultsWithoutContextSync();
				
				if (results.Length > 0)
				{
					JobHistoryErrorTransformer jobHistoryErrorTransformer = new JobHistoryErrorTransformer(_helper, _workspaceArtifactId);
					jobHistoryErrors = jobHistoryErrorTransformer.ConvertArtifactDtoToDto(results).FindAll(x => x.ErrorType == errorType);
				}
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format(JobHistoryErrorErrors.JOB_HISTORY_ERROR_RETRIEVE_FAILURE, jobHistoryArtifactId), ex);
			}
			
			return jobHistoryErrors;
		}


		public void UpdateErrorStatuses(ClaimsPrincipal claimsPrincipal, int numberOfErrors, int jobHistoryErrorTypeId, int errorStatusArtifactId, string tableName)
		{
			if (numberOfErrors <= 0)
			{
				return;
			}

			BaseServiceContext baseService = claimsPrincipal.GetUnversionContext(_workspaceArtifactId);

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
				JobHistoryErrorMassEditRepository jobHistoryErrorMassEditRepository = new JobHistoryErrorMassEditRepository();
				jobHistoryErrorMassEditRepository.UpdateSingleChoiceField(baseService, singleChoiceField, numberOfErrors, jobHistoryErrorArtifactType, errorStatusArtifactId, tableName);
			}
			catch (Exception e)
			{
				throw new Exception(MassEditErrors.JOB_HISTORY_ERROR_MASS_EDIT_FAILURE, e);
			}

		}

		public int CreateItemLevelErrorsSavedSearch(int integrationPointArtifactId, int savedSearchArtifactId, int jobHistoryArtifactId)
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
				Condition = new CriteriaCondition(jobHistoryFieldRef, CriteriaConditionEnum.AnyOfThese, new[] { jobHistoryArtifactId })
			};
			CriteriaCollection jobHistoryObjectCriteriaCollection = new CriteriaCollection
			{
				Conditions = new List<CriteriaBase>(1) { jobHistoryArtifactIdCriteria }
			};
			Criteria jobHistoryCriteria = new Criteria
			{
				Condition = new CriteriaCondition(jobHistoryFieldRef, CriteriaConditionEnum.In, jobHistoryObjectCriteriaCollection) { NotOperator = true }
			};

			CriteriaCollection searchCondition = new CriteriaCollection
			{
				Conditions = new List<CriteriaBase>(2) { savedSearchCriteria, jobHistoryCriteria }
			};

			KeywordSearch itemLevelSearch = new KeywordSearch
			{
				Name = $"{Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_SAVED_SEARCH_NAME} - {integrationPointArtifactId} - {jobHistoryArtifactId}",
				ArtifactTypeID = (int)Relativity.Client.ArtifactType.Document,
				SearchCriteria = searchCondition
			};

			using (IKeywordSearchManager searchManager = _helper.GetServicesManager().CreateProxy<IKeywordSearchManager>(ExecutionIdentity.System))
			{
				SearchResultViewFields fields = searchManager.GetFieldsForSearchResultViewAsync(_workspaceArtifactId, (int)Relativity.Client.ArtifactType.Document)
					.GetResultsWithoutContextSync();

				FieldRef field = fields.FieldsNotIncluded.First(x => x.Name == "Control Number");
				itemLevelSearch.Fields = new List<FieldRef>(1) { field };

				int itemLevelSearchArtifactId = searchManager.CreateSingleAsync(_workspaceArtifactId, itemLevelSearch).GetResultsWithoutContextSync();

				return itemLevelSearchArtifactId;
			}
		}

		public void DeleteItemLevelErrorsSavedSearch(int searchArtifactId, int retryAttempts)
		{
			using (IKeywordSearchManager searchManager = _helper.GetServicesManager().CreateProxy<IKeywordSearchManager>(ExecutionIdentity.System))
			{
				var task = searchManager.DeleteSingleAsync(_workspaceArtifactId, searchArtifactId);
				task.ConfigureAwait(false).GetAwaiter().GetResult();

				if (task.IsFaulted && retryAttempts < 3)
				{
					DeleteItemLevelErrorsSavedSearch(searchArtifactId, retryAttempts + 1);
				}
			}
		}

		public IList<JobHistoryErrorDTO> Read(IEnumerable<int> artifactIds)
		{
			List<JobHistoryError> jobHistoryErrors = _jobHistoryErrorLibrary.Read(artifactIds);
			return _dtoTransformer.ConvertToDto(jobHistoryErrors);
		}

		private class JobHistoryErrorMassEditRepository : RelativityMassEditBase
		{
			public new void UpdateSingleChoiceField(BaseServiceContext baseService, global::Relativity.Core.DTO.Field singleChoiceField, int numberOfErrors,
				global::Relativity.Query.ArtifactType jobHistoryErrorArtifactType, int errorStatusArtifactId, string tableName)
			{
				base.UpdateSingleChoiceField(baseService, singleChoiceField, numberOfErrors, jobHistoryErrorArtifactType, errorStatusArtifactId, tableName);
			}
		}
	}
}