using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Windsor;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Services.Installers;
using Relativity.Logging;
using Relativity.Telemetry.APM;

namespace Relativity.IntegrationPoints.Services
{
    public class IntegrationPointHealthCheckManager : KeplerServiceBase, IIntegrationPointHealthCheckManager
    {
        private Installer _installer;

        internal IntegrationPointHealthCheckManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer container)
        : base(logger, permissionRepositoryFactory, container)
        { }

        public IntegrationPointHealthCheckManager(ILog logger) : base(logger)
        { }

        public Task<HealthCheckOperationResult> RunHealthChecksAsync()
        {
            return RunHealthCheckAsync();
        }

        public Task<HealthCheckOperationResult> RunDeploymentHealthChecksAsync()
        {
            return RunHealthCheckAsync();
        }

        private static Task<HealthCheckOperationResult> RunHealthCheckAsync()
        {
            HealthCheckOperationResult healthCheckOperationResult = new HealthCheckOperationResult(isHealthy: true, message: "Integration Points application is healthy!");
            Client.APMClient.HealthCheckOperation(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK, () => healthCheckOperationResult)
                .Write();

            return Task.FromResult(healthCheckOperationResult);
        }

        protected override Installer Installer => _installer ?? (_installer = new HealthCheckInstaller());

        public void Dispose()
        {
        }
    }
}
