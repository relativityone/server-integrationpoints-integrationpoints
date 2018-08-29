using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Credential;
using Relativity.Services.ResourceServer;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class ExtendedIWorkspaceManager : IWorkspaceManager
	{
		private Lazy<IWorkspaceManager> _managerWrapper;
		private readonly ExecutionIdentity _identity;
		private readonly ITestHelper _helper;

		private readonly object _lock = new object();
		private IWorkspaceManager Manager => _managerWrapper.Value;

		public ExtendedIWorkspaceManager(ITestHelper helper, ExecutionIdentity identity)
		{
			_helper = helper;
			_identity = identity;
			_managerWrapper = new Lazy<IWorkspaceManager>(helper.CreateUserProxy<IWorkspaceManager>);
		}

		public void Dispose()
		{
			lock (_lock)
			{
				Manager.Dispose();
				_managerWrapper = new Lazy<IWorkspaceManager>(_helper.CreateUserProxy<IWorkspaceManager>);
			}
		}

		public async Task AddCredentialsAsync(WorkspaceRef workspace, List<CredentialRef> credentials)
		{
			await Manager.AddCredentialsAsync(workspace, credentials);
		}

		public async Task<WorkspaceRef> CreateWorkspaceAsync(WorkspaceSetttings workspace)
		{
			return await Manager.CreateWorkspaceAsync(workspace);
		}

		public async Task DeleteAsync(WorkspaceRef workspace)
		{
			await Manager.DeleteAsync(workspace);
		}

		public async Task RemoveCredentialsAsync(WorkspaceRef workspace, List<CredentialRef> credentials)
		{
			await Manager.RemoveCredentialsAsync(workspace, credentials);
		}

		public async Task<List<Credential>> GetAssociatedCredentialsAsync(WorkspaceRef workspace)
		{
			return await Manager.GetAssociatedCredentialsAsync(workspace);
		}

		public async Task<Credential> GetAssociatedCredentialForTypeAsync(WorkspaceRef workspace, CredentialType credentialType)
		{
			return await Manager.GetAssociatedCredentialForTypeAsync(workspace, credentialType);
		}

		public Task<IEnumerable<WorkspaceRef>> RetrieveAllActive()
		{
			return Manager.RetrieveAllActive();
		}

	    public Task<bool> WorkspaceExists(WorkspaceRef workspace)
	    {
	        return Manager.WorkspaceExists(workspace);
	    }

		public async Task<List<SQLServer>> GetAssociatedSQLServersAsync(WorkspaceRef workspace)
		{
			return await Manager.GetAssociatedSQLServersAsync(workspace);
		}

		public async Task<List<FileShareResourceServer>> GetAssociatedFileShareResourceServersAsync(WorkspaceRef workspace)
		{
			return await Manager.GetAssociatedFileShareResourceServersAsync(workspace);
		}

		public async Task<FileShareResourceServer> GetDefaultWorkspaceFileShareResourceServerAsync(WorkspaceRef workspace)
		{
			return await Manager.GetDefaultWorkspaceFileShareResourceServerAsync(workspace);
		}
	}
}
