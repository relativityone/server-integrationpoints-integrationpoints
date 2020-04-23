using System.Threading.Tasks;

namespace Relativity.Sync.WorkspaceGenerator.RelativityServices
{
	public interface ISavedSearchManager
	{
		Task CreateSavedSearchForTestCaseAsync(int workspaceID, string testCaseName);
	}
}