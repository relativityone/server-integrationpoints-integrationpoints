using System;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.RelativityWebApi;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Services
{
    public class RelativityManagerSoapHealthCheck : IHealthCheck
    {
        private readonly IWindsorContainer _container;
        private readonly Func<string> _webApiPathGetter;

        public RelativityManagerSoapHealthCheck(IWindsorContainer container, Func<string> webApiPathGetter)
        {
            _container = container;
            _webApiPathGetter = webApiPathGetter;
        }

        public async Task<HealthCheckOperationResult> Check()
        {
            RelativityManagerSoap relativityManagerSoapFactory =
                _container.Resolve<IRelativityManagerSoapFactory>().Create(_webApiPathGetter());
            try
            {
                string relativityUrl = await relativityManagerSoapFactory.GetRelativityUrlAsync();
                if (string.IsNullOrWhiteSpace(relativityUrl))
                {
                    return new HealthCheckOperationResult(false, "RelativityUrl has no value");
                }
            }
            catch (Exception e)
            {
                return new HealthCheckOperationResult(false, "Relativity WebApi call failed", e);
            }

            return new HealthCheckOperationResult(true, "HealthCheck for: RelativityManager call Result: OK");
        }
    }
}