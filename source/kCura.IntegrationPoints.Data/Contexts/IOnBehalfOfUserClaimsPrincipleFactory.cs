using System.Security.Claims;

namespace kCura.IntegrationPoints.Data.Contexts
{
	public interface IOnBehalfOfUserClaimsPrincipleFactory
	{
		ClaimsPrincipal CreateClaimsPrinciple(int userArtifactId);
	}
}