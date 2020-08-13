using System.Threading.Tasks;

namespace Relativity.Sync.WorkspaceGenerator.RelativityServices
{
	public interface ISavedSearchManager
	{
		Task<int> CreateSavedSearchForTestCaseAsync(int workspaceID, string testCaseName);
	}
}