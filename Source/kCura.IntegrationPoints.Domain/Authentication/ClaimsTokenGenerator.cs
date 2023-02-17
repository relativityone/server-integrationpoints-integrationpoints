using System.Linq;

namespace kCura.IntegrationPoints.Domain.Authentication
{
    public class ClaimsTokenGenerator : IAuthTokenGenerator
    {
        public string GetAuthToken()
        {
            string token = System.Security.Claims.ClaimsPrincipal.Current.Claims.Single(x => x.Type.Equals("access_token")).Value;

            return token;
        }
    }
}
