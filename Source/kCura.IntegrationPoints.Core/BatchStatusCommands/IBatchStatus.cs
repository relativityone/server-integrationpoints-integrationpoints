using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core
{
    public interface IBatchStatus
    {
        void OnJobStart(Job job);

        void OnJobComplete(Job job);
    }
}
