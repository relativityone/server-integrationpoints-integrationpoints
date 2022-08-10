using kCura.IntegrationPoints.Domain.Logging;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Logging
{
    public class DiagnosticLog : IDiagnosticLog
    {
        private readonly IAPILog _apiLog;

        public DiagnosticLog(IAPILog apiLog)
        {
            _apiLog = apiLog;
        }

        public void LogDiagnostic(string messageTemplate, params object[] propertyValues)
        {
            _apiLog.LogInformation("[Diagnostic] " + messageTemplate, propertyValues);
        }
    }
}
