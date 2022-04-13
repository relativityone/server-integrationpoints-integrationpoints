using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using kCura.Relativity.ImportAPI;
using Relativity.DataExchange;
using Relativity.Sync.Authentication;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
#pragma warning disable RG2002
	// We need to exclude this from code coverage, because we are using here concrete class from IAPI.
	[ExcludeFromCodeCoverage]
	internal sealed class ImportApiFactory : IImportApiFactory
	{
		private readonly IUserContextConfiguration _userContextConfiguration;
		private readonly IAuthTokenGenerator _tokenGenerator;

#pragma warning disable RG0001 
		private class RelativityTokenProvider : IRelativityTokenProvider
		{
			private readonly int _executingUserId;
			private readonly IAuthTokenGenerator _tokenGenerator;

			public RelativityTokenProvider(int executingUserId, IAuthTokenGenerator tokenGenerator)
			{
				_executingUserId = executingUserId;
				_tokenGenerator = tokenGenerator;
			}

			public string GetToken()
			{
				return _tokenGenerator.GetAuthTokenAsync(_executingUserId).GetAwaiter().GetResult();
			}
		}
#pragma warning restore RG0001

		public ImportApiFactory(IUserContextConfiguration userContextConfiguration, IAuthTokenGenerator tokenGenerator)
		{
			_userContextConfiguration = userContextConfiguration;
			_tokenGenerator = tokenGenerator;
		}

		public Task<IImportAPI> CreateImportApiAsync(Uri webServiceUrl)
		{
			IRelativityTokenProvider relativityTokenProvider = new RelativityTokenProvider(_userContextConfiguration.ExecutingUserId, _tokenGenerator);

			return Task.FromResult<IImportAPI>(ExtendedImportAPI.CreateByTokenProvider(webServiceUrl.AbsoluteUri, relativityTokenProvider));
		}
	}
#pragma warning restore RG2002
}