using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Services;
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

            return _objectManager.Query<JobHistoryError>(query);
        }

        private Guid GetChoiceGuidForErrorType(JobHistoryErrorDTO.Choices.ErrorType.Values errorType)
        {
            switch (errorType)
            {
                case JobHistoryErrorDTO.Choices.ErrorType.Values.Item:
                    return ErrorTypeChoices.JobHistoryErrorItemGuid;
                case JobHistoryErrorDTO.Choices.ErrorType.Values.Job:
                    return ErrorTypeChoices.JobHistoryErrorJobGuid;
                default:
                    throw new InvalidOperationException($"Guid for requested error type doesn't exist. Error type: {errorType}");
            }
        }

        public int CreateItemLevelErrorsSavedSearch(int integrationPointArtifactId, int savedSearchArtifactId, int jobHistoryArtifactId)
        {
            // Check for all documents that are part of the current saved search
            FieldRef savedSearchFieldRef = new FieldRef("(Saved Search)");
            Criteria savedSearchCriteria = new Criteria
            {
                Condition = new CriteriaCondition(savedSearchFieldRef, CriteriaConditionEnum.In, savedSearchArtifactId),
                BooleanOperator = BooleanOperatorEnum.And
            };

            // Check that the documents have not been tagged with the last Job History Object (meaning the job finished for them)
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
                ArtifactTypeID = (int)global::Relativity.ArtifactType.Document,
                SearchCriteria = searchCondition
            };

            using (IKeywordSearchManager searchManager = _helper.GetServicesManager().CreateProxy<IKeywordSearchManager>(ExecutionIdentity.System))
            {
                SearchResultViewFields fields = searchManager.GetFieldsForSearchResultViewAsync(_workspaceArtifactId, (int)global::Relativity.ArtifactType.Document)
                    .GetAwaiter().GetResult();

                FieldRef field = fields.FieldsNotIncluded.First(x => x.Name == "Artifact ID");
                itemLevelSearch.Fields = new List<FieldRef>(1) { field };

                int itemLevelSearchArtifactId = searchManager.CreateSingleAsync(_workspaceArtifactId, itemLevelSearch).GetAwaiter().GetResult();

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

        public Task<bool> MassUpdateAsync(IEnumerable<int> artifactIDsToUpdate, IEnumerable<FieldUpdateRequestDto> fieldsToUpdate)
        {
            IEnumerable<FieldRefValuePair> convertedFieldstoUpdate = fieldsToUpdate.Select(x => x.ToFieldRefValuePair());
            return _objectManager.MassUpdateAsync(artifactIDsToUpdate, convertedFieldstoUpdate, FieldUpdateBehavior.Merge);
        }
    }
}
