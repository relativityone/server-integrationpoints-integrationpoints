using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.RelativitySync.RipOverride
{
    public interface IExportServiceManager
    {
        void Execute(Job job);
    }
}