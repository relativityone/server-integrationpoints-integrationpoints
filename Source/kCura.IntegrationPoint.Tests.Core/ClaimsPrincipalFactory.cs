using System.Security.Claims;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Domain;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
    public class ClaimsPrincipalFactory : IOnBehalfOfUserClaimsPrincipalFactory
    {
        public ClaimsPrincipal CreateClaimsPrincipal(int userArtifactId)
        {
            var claims = new[] { new Claim(Claims.USER_ID, userArtifactId.ToString()) };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            return claimsPrincipal;
        }

        public ClaimsPrincipal CreateClaimsPrincipal2(int userArtifactId, IHelper helper)
        {
            var generator = new OAuth2TokenGenerator(helper, new OAuth2ClientFactory(helper), new TokenProviderFactoryFactory(), new CurrentUser {ID = userArtifactId});
            string authToken = generator.GetAuthToken();
            var claims = new[] { new Claim(Claims.USER_ID, userArtifactId.ToString()), new Claim(Claims.ACCESS_TOKEN_IDENTIFIER, authToken) };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            return claimsPrincipal;
        }
    }
}
