using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Credential;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class ExtendedIWorkspaceManager : IWorkspaceManager
	{
		private readonly ITestHelper _helper;
		private readonly ExecutionIdentity _identity;
		private Lazy<IWorkspaceManager> _managerWrapper;
		private IWorkspaceManager Manager => _managerWrapper.Value;

		public ExtendedIWorkspaceManager(ITestHelper helper, ExecutionIdentity identity)
		{
			_helper = helper;
			_identity = identity;
			_managerWrapper = new Lazy<IWorkspaceManager>(helper.CreateUserProxy<IWorkspaceManager>);
		}

		private readonly object _lock = new object();

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
	}
}
