namespace kCura.IntegrationPoints.Domain.Logging
{
    public interface IDiagnosticLog
    {
        void LogDiagnostic(string messageTemplate, params object[] propertyValues);
    }
}
