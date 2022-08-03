using System.Security.Claims;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Domain;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
    public class ClaimsPrincipalFactory
    {
        public ClaimsPrincipal CreateClaimsPrincipal(int userArtifactId, IHelper helper, IRetryHandlerFactory retryHandlerFactory)
        {
            var generator = new OAuth2TokenGenerator(
                helper,
                new OAuth2ClientFactory(retryHandlerFactory, helper),
                new TokenProviderFactoryFactory(),
                new CurrentUser(userID: userArtifactId));
            string authToken = generator.GetAuthToken();
            Claim[] claims =
            {
                new Claim(Claims.USER_ID,userArtifactId.ToString()),
                new Claim(Claims.ACCESS_TOKEN_IDENTIFIER, authToken)
            };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            return claimsPrincipal;
        }
    }
}
