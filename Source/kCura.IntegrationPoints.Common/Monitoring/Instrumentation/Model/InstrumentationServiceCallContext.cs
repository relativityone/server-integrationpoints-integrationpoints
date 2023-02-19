using System;

namespace kCura.IntegrationPoints.Common.Monitoring.Instrumentation.Model
{
    [Serializable]
    public class InstrumentationServiceCallContext
    {
        public string ServiceType { get; }

        public string ServiceName { get; }

        public string OperationName { get; }

        public InstrumentationServiceCallContext(string serviceType, string serviceName, string operationName)
        {
            ServiceType = serviceType;
            ServiceName = serviceName;
            OperationName = operationName;
        }
    }
}
