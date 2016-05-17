using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using kCura.IntegrationPoints.Data.Extensions;
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

		/// <summary>
		/// To be used internally by unit tests only
		/// </summary>
		internal JobHistoryErrorRepository(IHelper helper, int workspaceArtifactId)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public List<int> RetreiveJobHistoryErrorArtifactIds(int jobHistoryArtifactId, Relativity.Client.Choice errorType)
		{
			QueryResultSet<RDO> results = null;
			var query = new Query<RDO>();

			try
			{
				query.ArtifactTypeGuid = new Guid(ObjectTypeGuids.JobHistoryError);
				var jobHistoryCondition = new WholeNumberCondition(new Guid(JobHistoryErrorDTO.FieldGuids.JobHistory), NumericConditionEnum.EqualTo, jobHistoryArtifactId);
				var errorTypeCondition = new SingleChoiceCondition(new Guid(JobHistoryErrorDTO.FieldGuids.ErrorType), SingleChoiceConditionEnum.AnyOfThese, errorType.ArtifactGuids);
				query.Condition = new CompositeCondition(jobHistoryCondition, CompositeConditionEnum.And, errorTypeCondition);
				query.Fields = new List<FieldValue>
				{
					new FieldValue(Guid.Parse(JobHistoryErrorDTO.FieldGuids.ArtifactId))
				};

				using (
					IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
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

			return results.Results.Select(result => result.Artifact.ArtifactID).ToList();
		}

		public JobHistoryErrorDTO.UpdateStatusType DetermineUpdateStatusType(Relativity.Client.Choice jobType, bool hasJobLevelErrors, bool hasItemLevelErrors)
		{
			JobHistoryErrorDTO.UpdateStatusType updateStatusType = new JobHistoryErrorDTO.UpdateStatusType();

			if (jobType.Name == JobTypeChoices.JobHistoryRetryErrors.Name)
			{
				updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;
			}
			else
			{
				updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RunNow;
			}

			if (hasJobLevelErrors && hasItemLevelErrors)
			{
				updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem;
			}
			else if (hasJobLevelErrors)
			{
				updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly;
			}
			else if (hasItemLevelErrors)
			{
				updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly;
			}
			else
			{
				updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.None;
			}

			return updateStatusType;
		}

		public void CreateErrorListTempTables(List<int> jobLevelErrors, List<int> itemLevelErrors, JobHistoryErrorDTO.UpdateStatusType updateStatusType, string uniqueJobId)
		{
			if (updateStatusType.JobType == JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors)
			{
				switch (updateStatusType.ErrorTypes)
				{
					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem:
						CreateErrorListTempTable(jobLevelErrors, Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START, uniqueJobId);
						CreateErrorListTempTable(jobLevelErrors, Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_COMPLETE, uniqueJobId);
						CreateErrorListTempTable(itemLevelErrors, Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START, uniqueJobId);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
						CreateErrorListTempTable(jobLevelErrors, Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START, uniqueJobId);
						CreateErrorListTempTable(jobLevelErrors, Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_COMPLETE, uniqueJobId);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
						CreateErrorListTempTable(itemLevelErrors, Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START, uniqueJobId);
						//ToDo: Second CreateErrorListTempTable needed when logic to split item level errors between those being retried and those no longer included is written
						CreateErrorListTempTable(itemLevelErrors, Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_COMPLETE, uniqueJobId);
						break;
				}
			}
			else
			{
				switch (updateStatusType.ErrorTypes)
				{
					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem:
						CreateErrorListTempTable(jobLevelErrors, Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START, uniqueJobId);
						CreateErrorListTempTable(itemLevelErrors, Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START, uniqueJobId);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly:
						CreateErrorListTempTable(jobLevelErrors, Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START, uniqueJobId);
						break;

					case JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly:
						CreateErrorListTempTable(itemLevelErrors, Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START, uniqueJobId);
						break;
				}
			}
		}

		private void CreateErrorListTempTable(List<int> errors, string tablePrefix, string uniqueJobId)
		{
			try
			{
				ITempDocTableHelper tempDocTableHelper = new TempDocTableHelper(_helper, uniqueJobId);
				tempDocTableHelper.AddArtifactIdsIntoTempTable(errors, tablePrefix);
			}
			catch (Exception ex)
			{
				throw new Exception(JobHistoryErrorErrors.JOB_HISTORY_ERROR_TEMP_TABLE_CREATION_FAILURE, ex);
			}
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
				Name = $"Temporary Retry Errors Search - {integrationPointArtifactId} - {jobHistoryArtifactId}",
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
	}
}