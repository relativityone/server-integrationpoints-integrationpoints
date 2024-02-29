using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;
using Relativity.Services.ServiceProxy;

namespace Relativity.Sync.WorkspaceGenerator.RelativityServices
{
    public class SavedSearchManager : ISavedSearchManager
    {
        private readonly Guid _fileIconGuid = new Guid("861295b5-5b1d-4830-89e7-77e0a7ef1c30");
        private readonly Guid _controlNumberGuid = new Guid("2a3f1212-c8ca-4fa9-ad6b-f76c97f05438");

        private readonly IServiceFactory _serviceFactory;

        public SavedSearchManager(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public async Task<int> CreateSavedSearchForTestCaseAsync(int workspaceId, string testCaseName)
        {
            using (var keywordSearchManager = _serviceFactory.CreateProxy<IKeywordSearchManager>())
            {
                KeywordSearch search = CreateSavedSearchDTO(testCaseName);
                return await keywordSearchManager.CreateSingleAsync(workspaceId, search);
            }
        }

        public async Task<int?> GetSavedSearchIdForTestCaseAsync(int workspaceId, string testCaseName)
        {
            using (IObjectManager objectManager = _serviceFactory.CreateProxy<IObjectManager>())
            {
                QueryRequest savedSearchIdRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Search },
                    Condition = $"'Name' == '{testCaseName}'",
                    Fields = new[]
                    {
                        new FieldRef {Name = "ArtifactID"}
                    }
                };

                QueryResult fieldQueryResult = await objectManager.QueryAsync(workspaceId, savedSearchIdRequest, 0, 1).ConfigureAwait(false);

                return fieldQueryResult.Objects.FirstOrDefault()?.ArtifactID;
            }
        }

        public async Task<int> CountSavedSearchDocumentsAsync(int workspaceId, int savedSearchId)
        {
            using (IObjectManager objectManager = _serviceFactory.CreateProxy<IObjectManager>())
            {
                QueryRequest savedSearchDocumentsRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Document },
                    Condition = $"(('ArtifactId' IN SAVEDSEARCH {savedSearchId}))"
                };

                QueryResult savedSearchDocumentsQueryResult = await objectManager.QueryAsync(workspaceId, savedSearchDocumentsRequest, 0, 0).ConfigureAwait(false);

                return savedSearchDocumentsQueryResult.TotalCount;
            }
        }

        private KeywordSearch CreateSavedSearchDTO(string testCaseName)
        {
            var controlNumberField = new Services.Field.FieldRef(new List<Guid> {_controlNumberGuid});
            var fileIconField = new Services.Field.FieldRef(new List<Guid> {_fileIconGuid});

            CriteriaCollection criteria = new CriteriaCollection();
            criteria.Conditions.Add(new Criteria()
            {
                Condition = new CriteriaCondition(controlNumberField, CriteriaConditionEnum.StartsWith, $"{testCaseName}{Consts.ControlNumberSeparator}")
            });

            KeywordSearch search = new KeywordSearch()
            {
                Name = testCaseName,
                ArtifactTypeID = (int)ArtifactType.Document,
                SearchCriteria = criteria,
                SearchContainer = new SearchContainerRef(),
                Fields = new List<Services.Field.FieldRef>()
                {
                    fileIconField,
                    controlNumberField
                }
            };
            return search;
        }
    }
}