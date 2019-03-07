namespace Relativity.Sync.Authentication
{
	internal interface IOAuth2ClientFactory
	{
		Services.Security.Models.OAuth2Client GetOauth2Client(int userId);
	}
}