using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Core;
using Relativity.Data;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;
using FieldRef = Relativity.Services.Field.FieldRef;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class JobHistoryErrorRepository : MarshalByRefObject, IJobHistoryErrorRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;
		private readonly IRelativityObjectManager _objectManager;

		/// <summary>
		/// Internal due to Factory and Unit Tests
		/// </summary>
		internal JobHistoryErrorRepository(IHelper helper, IRelativityObjectManagerFactory objectManagerFactory, int workspaceArtifactId)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
			_objectManager = objectManagerFactory.CreateRelativityObjectManager(workspaceArtifactId);
		}

		public ICollection<int> RetrieveJobHistoryErrorArtifactIds(int jobHistoryArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values errorType)
		{
			ICollection<JobHistoryError> results = RetrieveJobHistoryErrorData(jobHistoryArtifactId, errorType);

			return results.Select(result => result.ArtifactId).ToList();
		}

		public IDictionary<int, string> RetrieveJobHistoryErrorIdsAndSourceUniqueIds(int jobHistoryArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values errorType)
		{
			ICollection<JobHistoryError> results = RetrieveJobHistoryErrorData(jobHistoryArtifactId, errorType);

			Dictionary<int, string> artifactIdsAndSourceUniqueIds = new Dictionary<int, string>();

			foreach (var result in results)
			{
				artifactIdsAndSourceUniqueIds.Add(result.ArtifactId, result.SourceUniqueID);
			}

			return artifactIdsAndSourceUniqueIds;
		}

		private ICollection<JobHistoryError> RetrieveJobHistoryErrorData(int jobHistoryArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values errorType)
		{
			Guid expectedChoiceGuid = GetChoiceGuidForErrorType(errorType);
			string jobHistoryCondition = $"'{JobHistoryErrorFields.JobHistory}' == {jobHistoryArtifactId} AND '{JobHistoryErrorFields.ErrorType}' == CHOICE {expectedChoiceGuid}";

			var query = new QueryRequest
			{
				Condition = jobHistoryCondition,
			};

			try
			{
				return _objectManager.Query<JobHistoryError>(query);
			}
			catch (Exception ex)
			{
				throw new IntegrationPointsException(string.Format(JobHistoryErrorErrors.JOB_HISTORY_ERROR_RETRIEVE_FAILURE, jobHistoryArtifactId), ex)
				{
					ExceptionSource = IntegrationPointsExceptionSource.KEPLER
				};
			}
		}

		private Guid GetChoiceGuidForErrorType(JobHistoryErrorDTO.Choices.ErrorType.Values errorType)
		{
			switch (errorType)
			{
					case JobHistoryErrorDTO.Choices.ErrorType.Values.Item:
						return ErrorTypeChoices.JobHistoryErrorItem.Guids.First();

					case JobHistoryErrorDTO.Choices.ErrorType.Values.Job:
						return ErrorTypeChoices.JobHistoryErrorJob.Guids.First();
				default:
					throw new InvalidOperationException($"Guid for requested error type doesn't exist. Error type: {errorType}");
			}
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
				jobHistoryErrorMassEditRepository.UpdateErrorStatuses(baseService, singleChoiceField, numberOfErrors, jobHistoryErrorArtifactType, errorStatusArtifactId, tableName);
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
				Name = $"{Constants.TEMPORARY_JOB_HISTORY_ERROR_SAVED_SEARCH_NAME} - {integrationPointArtifactId} - {jobHistoryArtifactId}",
				ArtifactTypeID = (int)Relativity.Client.ArtifactType.Document,
				SearchCriteria = searchCondition
			};

			using (IKeywordSearchManager searchManager = _helper.GetServicesManager().CreateProxy<IKeywordSearchManager>(ExecutionIdentity.System))
			{
				SearchResultViewFields fields = searchManager.GetFieldsForSearchResultViewAsync(_workspaceArtifactId, (int)Relativity.Client.ArtifactType.Document)
					.GetResultsWithoutContextSync();

				FieldRef field = fields.FieldsNotIncluded.First(x => x.Name == "Artifact ID");
				itemLevelSearch.Fields = new List<FieldRef>(1) { field };

				int itemLevelSearchArtifactId = searchManager.CreateSingleAsync(_workspaceArtifactId, itemLevelSearch).GetResultsWithoutContextSync();

				return itemLevelSearchArtifactId;
			}
		}

		public void DeleteItemLevelErrorsSavedSearch(int searchArtifactId)
		{
			for (int i = 0; i < 3; i++)
			{
				try
				{
					using (IKeywordSearchManager searchManager = _helper.GetServicesManager().CreateProxy<IKeywordSearchManager>(ExecutionIdentity.System))
					{
						var task = searchManager.DeleteSingleAsync(_workspaceArtifactId, searchArtifactId);
						task.ConfigureAwait(false).GetAwaiter().GetResult();
						return;
					}
				}
				catch
				{
					// ignored
				}
			}
		}

		public IList<JobHistoryError> Read(IEnumerable<int> artifactIds)
		{
			if (artifactIds == null)
			{
				return new List<JobHistoryError>();
			}
			List<JobHistoryError> jobHistoryErrors = _objectManager.Query<JobHistoryError>(new QueryRequest()
			{
				Condition = $"'{ArtifactQueryFieldNames.ArtifactID}' in [{string.Join(",", artifactIds)}]"
			});
			return jobHistoryErrors;
		}

		private class JobHistoryErrorMassEditRepository : RelativityMassEditBase
		{
			public void UpdateErrorStatuses(BaseServiceContext baseService, global::Relativity.Core.DTO.Field singleChoiceField, int numberOfErrors,
				global::Relativity.Query.ArtifactType jobHistoryErrorArtifactType, int errorStatusArtifactId, string tableName)
			{
				base.UpdateSingleChoiceField(baseService, singleChoiceField, numberOfErrors, jobHistoryErrorArtifactType, errorStatusArtifactId, tableName);
			}
		}
	}
}