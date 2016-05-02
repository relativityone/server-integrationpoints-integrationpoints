using System.Collections.Generic;
using System.Security.Claims;
using Relativity;

namespace kCura.IntegrationPoints.Data.Contexts
{
	public class OnBehalfOfUserClaimsPrincipalFactory : IOnBehalfOfUserClaimsPrincipalFactory
	{
		public ClaimsPrincipal CreateClaimsPrincipal(int userArtifactId)
		{
			IList<Claim> claims = new List<Claim>();
			claims.Add(new Claim(Claims.USER_ID, userArtifactId.ToString()));
			var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
			return claimsPrincipal;
		}
	}
}