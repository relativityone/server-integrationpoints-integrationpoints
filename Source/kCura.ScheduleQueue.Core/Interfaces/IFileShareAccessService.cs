using System.Threading.Tasks;

namespace kCura.ScheduleQueue.Core.Interfaces
{
    public interface IFileShareAccessService
    {
        Task MountBcpPathAsync();
    }
}
