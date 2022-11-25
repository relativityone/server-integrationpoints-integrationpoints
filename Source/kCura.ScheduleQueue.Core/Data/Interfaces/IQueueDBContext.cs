using kCura.IntegrationPoints.Data.DbContext;

namespace kCura.ScheduleQueue.Core.Data
{
    public interface IQueueDBContext
    {
        string TableName { get; }
        IEddsDBContext EddsDBContext { get; }
    }
}
