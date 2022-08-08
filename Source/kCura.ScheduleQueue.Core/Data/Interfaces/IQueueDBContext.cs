using Relativity.API;

namespace kCura.ScheduleQueue.Core.Data
{
    public interface IQueueDBContext
    {
        string TableName { get; }
        IDBContext EddsDBContext { get; }
    }
}
