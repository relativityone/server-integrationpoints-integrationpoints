using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.Installers;
using Relativity.Logging;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Services
{
    public class IntegrationPointHealthCheckManager : KeplerServiceBase, IIntegrationPointHealthCheckManager
    {
        private Installer _installer;

        internal IntegrationPointHealthCheckManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer container)
        : base(logger, permissionRepositoryFactory, container)
        {
        }

        public IntegrationPointHealthCheckManager(ILog logger) : base(logger)
        {
            
        }

        public async Task<HealthCheckOperationResult> RunHealthChecksAsync()
        {
            IWindsorContainer container = GetDependenciesContainer(Data.Constants.ADMIN_CASE_ID);

            List<IHealthCheck> healthChecks = new List<IHealthCheck>();

            var instanceSettingHealthCheck = new InstanceSettingHealthCheck(container);
            healthChecks.Add(instanceSettingHealthCheck);
            healthChecks.Add(new RelativityManagerSoapHealthCheck(container, () => instanceSettingHealthCheck.WebApiPath));
            healthChecks.Add(new SuccessHealthCheck());

            HealthCheckOperationResult result = null;
            foreach (var healthCheck in healthChecks)
            {
                result = await healthCheck.Check();

                LogResult(result);

                if (!result.IsHealthy)
                {
                    HealthCheckOperationResult localResultCopyForLambdaEvaluation = result;
                    IHealthMeasure healthMeasure = Client.APMClient.HealthCheckOperation(Core.Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK, () => localResultCopyForLambdaEvaluation);
					healthMeasure.Write();
                    break;
                }
            }

            return result;
        }

        private void LogResult(HealthCheckOperationResult result)
        {
            if (result.IsHealthy)
            {
                Logger.LogVerbose(result.Message);
            }
            else
            {
                Logger.LogError(result.Message);
            }
        }

        protected override Installer Installer => _installer ?? (_installer = new HealthCheckInstaller());

        public void Dispose()
        {
        }
    }
}
