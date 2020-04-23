using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Workspace;
using Relativity.Sync.WorkspaceGenerator.Fields;

namespace Relativity.Sync.WorkspaceGenerator.RelativityServices
{
	public interface IWorkspaceService
	{
		Task<IEnumerable<WorkspaceRef>> GetAllActiveAsync();
		Task<WorkspaceRef> GetWorkspaceAsync(int workspaceID);
		Task<WorkspaceRef> CreateWorkspaceAsync(string name, string templateWorkspaceName);
		Task<int> GetRootFolderArtifactIDAsync(int workspaceID);
		Task CreateFieldsAsync(int workspaceID, IEnumerable<CustomField> fields);
	}
}