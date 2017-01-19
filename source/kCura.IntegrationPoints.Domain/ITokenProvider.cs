using System;

namespace kCura.IntegrationPoints.Domain
{
	public interface ITokenProvider
	{
		string GetExternalSystemToken(string clientId, string clientSecret, Uri webApiRoute);
	}
}