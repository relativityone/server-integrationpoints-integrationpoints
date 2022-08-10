using kCura.IntegrationPoints.Domain.Logging;

namespace kCura.IntegrationPoints.Core.Logging
{
    public class EmptyDiagnosticLog : IDiagnosticLog
    {
        public void LogDiagnostic(string messageTemplate, params object[] propertyValues)
        {
            // Intentionally left blank
        }
    }
}
