using Relativity.API;
using Relativity.API.Foundation;
using Relativity.API.Foundation.Repositories;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FoundationRepositoryFactory : IFoundationRepositoryFactory
	{
		private IHelper _helper;

		public FoundationRepositoryFactory(IHelper helper)
		{
			_helper = helper;
		}

		public T GetRepository<T>(int workspaceID) where T : IRepository
		{
			IWorkspaceGateway workspaceGateway = _helper.GetServicesManager().CreateProxy<IWorkspaceGateway>(ExecutionIdentity.CurrentUser);
			IWorkspaceContext workspaceContext = workspaceGateway.GetWorkspaceContext(workspaceID);
			return workspaceContext.CreateRepository<T>();
		}
	}
}
