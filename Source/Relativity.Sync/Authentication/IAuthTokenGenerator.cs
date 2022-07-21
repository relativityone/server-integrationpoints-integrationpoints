using System.Threading.Tasks;

namespace Relativity.Sync.Authentication
{
    internal interface IAuthTokenGenerator
    {
        Task<string> GetAuthTokenAsync(int userId);
    }
}