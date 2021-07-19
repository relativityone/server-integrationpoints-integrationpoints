using System.Threading.Tasks;
using Castle.Windsor;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Services.Installers;
using Relativity.Logging;
using WorkloadDiscovery;

namespace Relativity.IntegrationPoints.Services
{
	public class IntegrationPointAgentManager : KeplerServiceBase, IIntegrationPointsAgentManager
	{
		private Installer _installer;

		protected override Installer Installer => _installer ?? (_installer = new IntegrationPointManagerInstaller());

		public IntegrationPointAgentManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer container)
			: base(logger, permissionRepositoryFactory, container)
		{
		}

		public IntegrationPointAgentManager(ILog logger)
			: base(logger)
		{
		}

		public Task<Workload> GetWorkloadAsync()
		{
			return Task.FromResult(new Workload());
		}

		public void Dispose()
		{
			throw new System.NotImplementedException();
		}
	}
}