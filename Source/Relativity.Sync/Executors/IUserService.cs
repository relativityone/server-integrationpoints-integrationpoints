using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
    internal interface IUserService
    {
        Task<bool> ExecutingUserIsAdminAsync(int userId);
    }
}
