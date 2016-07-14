using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using kCura.IntegrationPoints.Data.Contexts;
using Relativity;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class ClaimsPrincipalFactory : IOnBehalfOfUserClaimsPrincipalFactory
	{
		public ClaimsPrincipal CreateClaimsPrincipal(int userArtifactId)
		{
			Claim[] claims = new Claim[] {new Claim(Claims.USER_ID, userArtifactId.ToString())};
			ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
			return claimsPrincipal;
		}

		public ClaimsPrincipal CreateClaimsPrincipal2(int userArtifactId)
		{
			string authToken = GetBase64String("relativity.admin@kcura.com:Test!");
			Claim[] claims = new Claim[] { new Claim(Claims.USER_ID, userArtifactId.ToString()), new Claim(Claims.ACCESS_TOKEN_IDENTIFIER, authToken)};
			ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
			return claimsPrincipal;
		}

		private static string GetBase64String(string stringToConvertToBase64)
		{
			string base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes(stringToConvertToBase64));
			return base64String;
		}
	}
}
