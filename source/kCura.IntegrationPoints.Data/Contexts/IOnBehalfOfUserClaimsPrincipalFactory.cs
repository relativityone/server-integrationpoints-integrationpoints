using System.Security.Claims;

namespace kCura.IntegrationPoints.Data.Contexts
{
	public interface IOnBehalfOfUserClaimsPrincipalFactory
	{
		ClaimsPrincipal CreateClaimsPrincipal(int userArtifactId);
	}
}