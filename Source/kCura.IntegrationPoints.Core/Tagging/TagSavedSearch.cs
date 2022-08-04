using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.API;
using Relativity.Services.Field;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Tagging
{
    public class TagSavedSearch : ITagSavedSearch
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IMultiObjectSavedSearchCondition _multiObjectSavedSearchCondition;
        private readonly IAPILog _logger;

        public TagSavedSearch(IRepositoryFactory repositoryFactory, IMultiObjectSavedSearchCondition multiObjectSavedSearchCondition, IHelper helper)
        {
            _repositoryFactory = repositoryFactory;
            _multiObjectSavedSearchCondition = multiObjectSavedSearchCondition;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<TagSavedSearch>();
        }

        public int CreateTagSavedSearch(int workspaceArtifactId, TagsContainer tagsContainer, int savedSearchFolderId)
        {
            try
            {
                IKeywordSearchRepository keywordSearchRepository = _repositoryFactory.GetKeywordSearchRepository();

                KeywordSearch searchDto = CreateKeywordSearchForTagging(tagsContainer, savedSearchFolderId);
                int keywordSearchId = keywordSearchRepository.CreateSavedSearch(workspaceArtifactId, searchDto);
                _logger.LogInformation("Created tagging keyword search: {keywordSearchId}", keywordSearchId);
                return keywordSearchId;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create Saved Search for promoted documents in destination workspace {workspaceArtifactId}.", workspaceArtifactId);
                throw new IntegrationPointsException($"Failed to create Saved Search for promoted documents in destination workspace {workspaceArtifactId}.", e);
            }
        }

        private KeywordSearch CreateKeywordSearchForTagging(TagsContainer tagsContainer, int savedSearchFolderId)
        {
            CriteriaCollection criteria = CreateWorkspaceAndJobCriteria(tagsContainer);

            return new KeywordSearch
            {
                Name = LimitLength(tagsContainer.SourceJobDto.Name),
                ArtifactTypeID = (int)ArtifactType.Document,
                SearchCriteria = criteria,
                SearchContainer = new SearchContainerRef(savedSearchFolderId),
                Fields = new List<FieldRef>
                {
                    new FieldRef(new List<Guid> {DocumentFieldsConstants.FileIconGuid}),
                    new FieldRef(DocumentFieldsConstants.EDIT_FIELD_NAME),
                    new FieldRef(new List<Guid> {DocumentFieldsConstants.ControlNumberGuid})
                }
            };
        }

        private string LimitLength(string name)
        {
            const int maxLength = 50;
            return name.Length > maxLength
                ? name.Remove(maxLength / 2 - 3) + "..." + name.Substring(name.Length - maxLength / 2)
                : name;
        }

        private CriteriaCollection CreateWorkspaceAndJobCriteria(TagsContainer tagsContainer)
        {
            var conditionCollection = new CriteriaCollection();
            CriteriaBase sourceJobCriteria = _multiObjectSavedSearchCondition.CreateConditionForMultiObject(
                SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid, CriteriaConditionEnum.AllOfThese, new List<int>{tagsContainer.SourceJobDto.ArtifactId});

            CriteriaBase sourceWorkspaceCriteria = _multiObjectSavedSearchCondition.CreateConditionForMultiObject(
                SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid, CriteriaConditionEnum.AllOfThese, new List<int> { tagsContainer.SourceWorkspaceDto.ArtifactId });

            conditionCollection.Conditions.Add(sourceJobCriteria);
            conditionCollection.Conditions.Add(sourceWorkspaceCriteria);

            return conditionCollection;
        }
    }
}