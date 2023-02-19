using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IKeywordSearchRepository
    {
        int CreateSavedSearch(int workspaceId, KeywordSearch searchDto);

        int CreateSearchContainerInRoot(int workspaceId, string name);

        SearchContainer QuerySearchContainer(int workspaceId, string name);
    }
}
