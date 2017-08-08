using Relativity.Services.Security.Models;

namespace kCura.IntegrationPoints.Core.Authentication
{
	public interface IOAuth2ClientFactory
	{
		OAuth2Client GetOauth2Client(int userId);
	}
}