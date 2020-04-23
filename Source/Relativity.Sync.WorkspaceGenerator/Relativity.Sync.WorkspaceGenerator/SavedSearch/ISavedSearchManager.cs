using System.Threading.Tasks;

namespace Relativity.Sync.WorkspaceGenerator.SavedSearch
{
	public interface ISavedSearchManager
	{
		Task CreateSavedSearchForTestCaseAsync(int workspaceID, string testCaseName);
	}
}