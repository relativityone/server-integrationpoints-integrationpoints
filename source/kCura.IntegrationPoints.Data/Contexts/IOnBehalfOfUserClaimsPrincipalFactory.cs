using System.Security.Claims;

namespace kCura.IntegrationPoints.Data.Contexts
{
	public interface IOnBehalfOfUserClaimsPrincipalFactory
	{
		/// <summary>
		/// Returns a ClaimsPrincipal object that is created with the user's context.
		/// </summary>
		/// <param name="userArtifactId">The artifact id of the user used to construct the Claims Principal.</param>
		/// <returns>A ClaimsPrincipal object that contains the user's identity.</returns>
		ClaimsPrincipal CreateClaimsPrincipal(int userArtifactId);
	}
}