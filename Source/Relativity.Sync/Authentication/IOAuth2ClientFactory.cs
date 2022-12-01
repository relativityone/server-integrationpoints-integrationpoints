using System.Threading.Tasks;

namespace Relativity.Sync.Authentication
{
    internal interface IOAuth2ClientFactory
    {
        Task<Services.Security.Models.OAuth2Client> GetOauth2ClientAsync(int userId);
    }
}
