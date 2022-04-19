using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using kCura.Relativity.ImportAPI;
using Relativity.DataExchange;
using Relativity.Sync.Authentication;
using Relativity.Sync.Transfer;
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
		private readonly INonAdminCanSyncUsingLinks _nonAdminCanSyncUsingLinks;
		private readonly IUserService _userService;
		private readonly IExtendedImportAPI _extendedImportApi;
		private readonly ISyncLog _logger;
		private const int _ADMIN_USER_ID = 777;

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

		public ImportApiFactory(IUserContextConfiguration userContextConfiguration, IAuthTokenGenerator tokenGenerator, INonAdminCanSyncUsingLinks nonAdminCanSyncUsingLinks, IUserService userService, IExtendedImportAPI extendedImportApi, ISyncLog logger)
		{
			_userContextConfiguration = userContextConfiguration;
			_tokenGenerator = tokenGenerator;
			_nonAdminCanSyncUsingLinks = nonAdminCanSyncUsingLinks;
			_userService = userService;
			_extendedImportApi = extendedImportApi;
			_logger = logger;
		}
		
		public Task<IImportAPI> CreateImportApiAsync(Uri webServiceUrl)
		{
			int executingUserId = _userContextConfiguration.ExecutingUserId;
			bool executingUserIsAdmin = _userService.ExecutingUserIsAdminAsync(_userContextConfiguration).GetAwaiter().GetResult();
			if (_nonAdminCanSyncUsingLinks.IsEnabled() && !executingUserIsAdmin)
			{
				executingUserId = _ADMIN_USER_ID;
			}
			
			_logger.LogInformation("Creating IAPI as userId: {executingUserId}", executingUserId);
			IRelativityTokenProvider relativityTokenProvider = new RelativityTokenProvider(executingUserId, _tokenGenerator);

			var byTokenProvider = _extendedImportApi.CreateByTokenProvider(webServiceUrl.AbsoluteUri, relativityTokenProvider);
			return Task.FromResult<IImportAPI>(byTokenProvider);
		}
	}
#pragma warning restore RG2002
}