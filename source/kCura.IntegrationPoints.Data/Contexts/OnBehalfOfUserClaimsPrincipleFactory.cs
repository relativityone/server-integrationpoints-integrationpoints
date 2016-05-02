using System.Collections.Generic;
using System.Security.Claims;
using Relativity;

namespace kCura.IntegrationPoints.Data.Contexts
{
	public class OnBehalfOfUserClaimsPrincipleFactory : IOnBehalfOfUserClaimsPrincipleFactory
	{
		public ClaimsPrincipal CreateClaimsPrinciple(int userArtifactId)
		{
			IList<Claim> claims = new List<Claim>();
			claims.Add(new Claim(Claims.USER_ID, userArtifactId.ToString()));
			var claimsPrinciple = new ClaimsPrincipal(new ClaimsIdentity(claims));
			return claimsPrinciple;
		}
	}
}