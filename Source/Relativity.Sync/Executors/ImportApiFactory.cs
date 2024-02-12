using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using kCura.Relativity.ImportAPI;
using Relativity.API;
using Relativity.DataExchange;
using Relativity.Sync.Authentication;
using Relativity.Sync.Configuration;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;

namespace Relativity.Sync.Executors
{
    // We need to exclude this from code coverage, because we are using here concrete class from IAPI.
    [ExcludeFromCodeCoverage]
    internal sealed class ImportApiFactory : IImportApiFactory
    {
        private const int _ADMIN_USER_ID = 777;

        // Temporarily use RIP's GUID - this should be replaced with Sync app GUID once Sync is released to production
        private readonly Guid _ripAppGuid = new Guid("dcf6e9d1-22b6-4da3-98f6-41381e93c30c");

        private readonly IUserContextConfiguration _userContextConfiguration;
        private readonly IAuthTokenGenerator _tokenGenerator;
        private readonly ISyncToggles _syncToggles;
        private readonly IUserService _userService;
        private readonly IExtendedImportAPI _extendedImportApi;
        private readonly IHelper _helper;
        private readonly IAPILog _logger;

        public ImportApiFactory(IUserContextConfiguration userContextConfiguration, IAuthTokenGenerator tokenGenerator, ISyncToggles syncToggles, IUserService userService, IExtendedImportAPI extendedImportApi, IHelper helper, IAPILog logger)
        {
            _userContextConfiguration = userContextConfiguration;
            _tokenGenerator = tokenGenerator;
            _syncToggles = syncToggles;
            _userService = userService;
            _extendedImportApi = extendedImportApi;
            _helper = helper;
            _logger = logger;
        }

        public async Task<IImportAPI> CreateImportApiAsync()
        {
            int executingUserId = _userContextConfiguration.ExecutingUserId;
            bool executingUserIsAdmin = await _userService.ExecutingUserIsAdminAsync(executingUserId).ConfigureAwait(false);
            if (_syncToggles.IsEnabled<EnableNonAdminSyncLinksToggle>() && !executingUserIsAdmin)
            {
                executingUserId = _ADMIN_USER_ID;
            }

            _logger.LogInformation("Creating IAPI as userId: {executingUserId}", executingUserId);
            IRelativityTokenProvider relativityTokenProvider = new RelativityTokenProvider(executingUserId, _tokenGenerator);

            Uri webServiceUrl = GetWebServiceUrl();
            _logger.LogInformation("Using following web service URL to create ImportAPI: {webServiceUrl}", webServiceUrl.AbsoluteUri);

            ImportAPI importApiInstanceByToken = _extendedImportApi.CreateByTokenProvider(webServiceUrl.AbsoluteUri, relativityTokenProvider);
            return importApiInstanceByToken;
        }

        private Uri GetWebServiceUrl()
        {
            Uri relativityUri = GetRelativityUri();
            Uri webServiceUrl = new Uri($"{relativityUri.Scheme}://{relativityUri.Host}/Relativity.Rest/API/");
            return webServiceUrl;
        }

        private Uri GetRelativityUri()
        {
            return _helper.GetUrlHelper().GetApplicationURL(_ripAppGuid);
        }

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
    }
}
