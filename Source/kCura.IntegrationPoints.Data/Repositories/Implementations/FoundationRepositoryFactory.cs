using Relativity.API;
using Relativity.API.Foundation;
using Relativity.API.Foundation.Repositories;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FoundationRepositoryFactory : IFoundationRepositoryFactory
	{
		private readonly IHelper _helper;

		public FoundationRepositoryFactory(IHelper helper)
		{
			_helper = helper;
		}

		public T GetRepository<T>(int workspaceId) where T : IRepository
		{
			using (var workspaceGateway = _helper.GetServicesManager().CreateProxy<IWorkspaceGateway>(ExecutionIdentity.CurrentUser))
			{
				IWorkspaceContext workspaceContext = workspaceGateway.GetWorkspaceContext(workspaceId);
				return workspaceContext.CreateRepository<T>();
			}
		}
	}
}
