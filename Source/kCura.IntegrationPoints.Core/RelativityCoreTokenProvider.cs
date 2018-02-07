using System;
using kCura.IntegrationPoints.Domain;
using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public class RelativityCoreTokenProvider : ITokenProvider
	{
		public string GetExternalSystemToken(string clientId, string clientSecret, Uri webApiRoute)
		{
			string token = ExtensionPointServiceFinder.SystemTokenProvider.GetExternalSystemToken(clientId, clientSecret, webApiRoute);

			return token;
		}
	}
}