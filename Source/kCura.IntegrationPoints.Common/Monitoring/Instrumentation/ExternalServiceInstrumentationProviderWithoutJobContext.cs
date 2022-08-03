using kCura.IntegrationPoints.Common.Monitoring.Instrumentation.Model;
using Relativity.API;

namespace kCura.IntegrationPoints.Common.Monitoring.Instrumentation
{
    public class ExternalServiceInstrumentationProviderWithoutJobContext : IExternalServiceInstrumentationProvider
    {
        private readonly IAPILog _logger;

        public ExternalServiceInstrumentationProviderWithoutJobContext(IAPILog logger)
        {
            _logger = logger;
        }

        public IExternalServiceInstrumentation Create(string serviceType, string serviceName, string operationName)
        {
            var callContext = new InstrumentationServiceCallContext(serviceType, serviceName, operationName);
            return new ExternalServiceLogsInstrumentation(callContext, _logger);
        }

        public IExternalServiceSimpleInstrumentation CreateSimple(string serviceType, string serviceName, string operationName)
        {
            return new ExternalServiceSimpleInstrumentation(Create(serviceType, serviceName, operationName));
        }
    }
}
