namespace kCura.IntegrationPoints.Agent.Monitoring.SystemReporter
{
    public interface IKeplerPingReporter
    {
        bool IsKeplerServiceAccessible();
    }
}