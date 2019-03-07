namespace Relativity.Sync.Authentication
{
	internal interface IAuthTokenGenerator
	{
		string GetAuthToken(int userId);
	}
}