using System.Threading.Tasks;

namespace Relativity.Sync.WorkspaceGenerator.RelativityServices
{
    public interface ISavedSearchManager
    {
        Task<int?> GetSavedSearchIdForTestCaseAsync(int workspaceId, string testCaseName);

        Task<int> CreateSavedSearchForTestCaseAsync(int workspaceId, string testCaseName);

        Task<int> CountSavedSearchDocumentsAsync(int workspaceId, int savedSearchId);
    }
}