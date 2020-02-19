using System;
using kCura.Relativity.Client;
using kCura.IntegrationPoints.Domain.Authentication;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers
{
	internal sealed class RSAPIClientTokenGenerator : IAuthTokenGenerator
	{
		private readonly IRSAPIClient _rsapiClient;

		public RSAPIClientTokenGenerator(IRSAPIClient rsapiClient)
		{
			_rsapiClient = rsapiClient;
		}

		public string GetAuthToken()
		{
			ReadResult readResult = _rsapiClient.GenerateRelativityAuthenticationToken(_rsapiClient.APIOptions);

			if (readResult.Success)
			{
				return _rsapiClient.APIOptions.Token;
			}

			return null;
		}
	}
}
