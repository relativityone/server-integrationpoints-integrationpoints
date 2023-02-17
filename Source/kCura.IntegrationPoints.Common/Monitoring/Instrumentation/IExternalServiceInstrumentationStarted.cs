using System;

namespace kCura.IntegrationPoints.Common.Monitoring.Instrumentation
{
    public interface IExternalServiceInstrumentationStarted
    {
        void Completed();

        void Failed(string reason);

        void Failed(Exception ex);
    }
}
