namespace kCura.IntegrationPoints.Agent.Monitoring.SystemReporter
{
    public interface IDatabasePingReporter
    {
        bool IsDatabaseAccessible();
    }
}