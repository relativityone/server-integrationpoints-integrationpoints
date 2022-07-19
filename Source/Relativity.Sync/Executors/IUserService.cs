using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
    internal interface IUserService
    {
        Task<bool> ExecutingUserIsAdminAsync(int userId);
    }
}