using System.Collections.Concurrent;

namespace Relativity.Sync.Authentication
{
	internal sealed class OAuth2TokenGeneratorWithCache : IAuthTokenGenerator
	{
		private readonly ConcurrentDictionary<int, string> _authTokens = new ConcurrentDictionary<int, string>();
		private readonly IAuthTokenGenerator _authTokenGenerator;

		public OAuth2TokenGeneratorWithCache(IAuthTokenGenerator authTokenGenerator)
		{
			_authTokenGenerator = authTokenGenerator;
		}

		public string GetAuthToken(int userId)
		{
			return _authTokens.GetOrAdd(userId, id => _authTokenGenerator.GetAuthToken(id));
		}
	}
}