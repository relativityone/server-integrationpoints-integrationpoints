using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using Relativity.Telemetry.APM;

namespace Relativity.IntegrationPoints.Services
{
    public class InstanceSettingHealthCheck : IHealthCheck
    {
        private readonly IWindsorContainer _container;
        public string WebApiPath { get; private set; }

        public InstanceSettingHealthCheck(IWindsorContainer container)
        {
            _container = container;
        }

        public Task<HealthCheckOperationResult> Check()
        {
            IInstanceSettingRepository isRepo = _container.Resolve<IRepositoryFactory>().GetInstanceSettingRepository();
            string value = isRepo.GetConfigurationValue(Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION, Constants.WEB_API_PATH);
            if (string.IsNullOrWhiteSpace(value))
            {
                return Task.FromResult(new HealthCheckOperationResult(false, "WebApiPath InstanceSetting is null or empty"));
            }

            WebApiPath = value;

            return Task.FromResult(new HealthCheckOperationResult(true, $"HealthCheck for: InstanceSetting {Constants.WEB_API_PATH} Result: OK"));
        }
    }
}