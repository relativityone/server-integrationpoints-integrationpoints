using System.Threading.Tasks;
using Relativity.Services.Security.Models;

namespace kCura.IntegrationPoints.Core.Authentication
{
    public interface IOAuth2ClientFactory
    {
        Task<OAuth2Client> GetOauth2ClientAsync(int userId);
    }
}
