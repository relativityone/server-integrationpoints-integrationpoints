using System;
using System.Collections.Generic;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class KeplerKeywordSearchRepository : IKeywordSearchRepository
    {
        private readonly IServicesMgr _servicesMgr;

        public KeplerKeywordSearchRepository(IServicesMgr servicesMgr)
        {
            _servicesMgr = servicesMgr;
        }

        public int CreateSavedSearch(int workspaceId, KeywordSearch searchDto)
        {
            using (var proxy = _servicesMgr.CreateProxy<IKeywordSearchManager>(ExecutionIdentity.CurrentUser))
            {
                return proxy.CreateSingleAsync(workspaceId, searchDto).Result;
            }
        }

        public int CreateSearchContainerInRoot(int workspaceId, string name)
        {
            using (var proxy = _servicesMgr.CreateProxy<ISearchContainerManager>(ExecutionIdentity.CurrentUser))
            {
                SearchContainer searchContainer = new SearchContainer
                {
                    Name = name,
                    ParentSearchContainer = {ArtifactID = 0}
                };
                return proxy.CreateSingleAsync(workspaceId, searchContainer).Result;
            }
        }

        public SearchContainer QuerySearchContainer(int workspaceId, string name)
        {
            Condition condition = new TextCondition(ClientFieldNames.Name, TextConditionEnum.EqualTo, name);
            var sort = new Sort
            {
                Direction = SortEnum.Descending,
                Order = 0,
                FieldIdentifier = {Name = "ArtifactID"}
            };
            var query = new Query(condition.ToQueryString(), new List<Sort> {sort});

            SearchContainerQueryResultSet result;
            using (var proxy = _servicesMgr.CreateProxy<ISearchContainerManager>(ExecutionIdentity.CurrentUser))
            {
                result = proxy.QueryAsync(workspaceId, query).Result;
            }

            if (!result.Success)
            {
                throw new Exception($"Failed to query Saved Search Folder {result.Message}");
            }
            if (result.Results.Count > 0)
            {
                return result.Results[0].Artifact;
            }
            return null;
        }
    }
}
