using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync.Metrics
{
    public interface IMetricCollection
    {
        IMetricCollection AddMetric<T>(T metric) where T: IMetric;
        Task SendAsync();
    }
}
