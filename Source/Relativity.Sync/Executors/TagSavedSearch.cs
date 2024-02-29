using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Field;
using Relativity.Services.Search;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Executors
{
    internal sealed class TagSavedSearch : ITagSavedSearch
    {
        private readonly Guid _jobHistoryFieldOnDocumentGuid = new Guid("7cc3faaf-cbb8-4315-a79f-3aa882f1997f");
        private readonly Guid _fileIconGuid = new Guid("861295b5-5b1d-4830-89e7-77e0a7ef1c30");
        private readonly Guid _controlNumberGuid = new Guid("2a3f1212-c8ca-4fa9-ad6b-f76c97f05438");

        private readonly IDestinationServiceFactoryForUser _destinationServiceFactoryForUser;
        private readonly IAPILog _syncLog;

        public TagSavedSearch(IDestinationServiceFactoryForUser destinationServiceFactoryForUser, IAPILog syncLog)
        {
            _destinationServiceFactoryForUser = destinationServiceFactoryForUser;
            _syncLog = syncLog;
        }

        public async Task<int> CreateTagSavedSearchAsync(IDestinationWorkspaceSavedSearchCreationConfiguration configuration, int savedSearchFolderArtifactId, CancellationToken token)
        {
            int destinationWorkspaceArtifactId = configuration.DestinationWorkspaceArtifactId;

            try
            {
                using (var keywordSearchManager = await _destinationServiceFactoryForUser.CreateProxyAsync<IKeywordSearchManager>().ConfigureAwait(false))
                {
                    KeywordSearch keywordSearchDto = CreateKeywordSearchForTagging(configuration, savedSearchFolderArtifactId);
                    int keywordSearchId = await keywordSearchManager.CreateSingleAsync(destinationWorkspaceArtifactId, keywordSearchDto).ConfigureAwait(false);

                    _syncLog.LogInformation("Created tagging keyword search: {keywordSearchId}", keywordSearchId);
                    return keywordSearchId;
                }
            }
            catch (Exception e)
            {
                _syncLog.LogError(e, "Failed to create Saved Search for promoted documents in destination workspace {workspaceArtifactId}.", destinationWorkspaceArtifactId);
                throw new SyncException($"Failed to create Saved Search for promoted documents in destination workspace {destinationWorkspaceArtifactId}.", e);
            }
        }

        private KeywordSearch CreateKeywordSearchForTagging(IDestinationWorkspaceSavedSearchCreationConfiguration configuration, int savedSearchFolderId)
        {
            CriteriaCollection criteria = CreateWorkspaceAndJobCriteria(configuration);

            string truncatedSourceJobTagName = StringExtensions.LimitLength(configuration.GetSourceJobTagName());

            var keywordSearchDto = new KeywordSearch
            {
                Name = truncatedSourceJobTagName,
                ArtifactTypeID = (int)ArtifactType.Document,
                SearchCriteria = criteria,
                SearchContainer = new SearchContainerRef(savedSearchFolderId),
                Fields = new List<FieldRef>
                {
                    new FieldRef(new List<Guid> { _fileIconGuid }),
                    new FieldRef("Edit"),
                    new FieldRef(new List<Guid> { _controlNumberGuid })
                }
            };
            return keywordSearchDto;
        }

        private CriteriaCollection CreateWorkspaceAndJobCriteria(IDestinationWorkspaceSavedSearchCreationConfiguration configuration)
        {
            CriteriaBase sourceJobCriteria = CreateConditionForMultiObject(_jobHistoryFieldOnDocumentGuid, configuration.SourceJobTagArtifactId);

            var conditionCollection = new CriteriaCollection();
            conditionCollection.Conditions.Add(sourceJobCriteria);
            return conditionCollection;
        }

        private static CriteriaBase CreateConditionForMultiObject(Guid fieldGuid, int fieldArtifactId)
        {
            var fieldIdentifier = new FieldRef(new List<Guid> { fieldGuid });

            // Create main condition
            var criteria = new Criteria
            {
                Condition = new CriteriaCondition(fieldIdentifier, CriteriaConditionEnum.AllOfThese, new[] { fieldArtifactId }),
                BooleanOperator = BooleanOperatorEnum.And
            };

            // Aggregate condition with CriteriaCollection
            var criteriaCollection = new CriteriaCollection();
            criteriaCollection.Conditions.Add(criteria);

            // Multi-objects require condition to be aggregated into additional CriteriaCondition
            var parentCriteria = new Criteria
            {
                Condition = new CriteriaCondition(fieldIdentifier, CriteriaConditionEnum.In, criteriaCollection)
            };
            return parentCriteria;
        }
    }
}
