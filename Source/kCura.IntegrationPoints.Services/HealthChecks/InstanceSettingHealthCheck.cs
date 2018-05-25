using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Services
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
            string value = isRepo.GetConfigurationValue(Domain.Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION, Domain.Constants.WEB_API_PATH);
            if (string.IsNullOrWhiteSpace(value))
            {
                return Task.FromResult(new HealthCheckOperationResult(false, "WebApiPath InstanceSetting is null or empty"));
            }

            WebApiPath = value;

            return Task.FromResult(new HealthCheckOperationResult(true, $"HealthCheck for: InstanceSetting {Domain.Constants.WEB_API_PATH} Result: OK"));
        }
    }
}