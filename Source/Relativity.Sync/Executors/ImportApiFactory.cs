﻿using System;
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
#pragma warning disable RG2002
    // We need to exclude this from code coverage, because we are using here concrete class from IAPI.
    [ExcludeFromCodeCoverage]
    internal sealed class ImportApiFactory : IImportApiFactory
    {
        private readonly IUserContextConfiguration _userContextConfiguration;
        private readonly IAuthTokenGenerator _tokenGenerator;
        private readonly ISyncToggles _syncToggles;
        private readonly IUserService _userService;
        private readonly IExtendedImportAPI _extendedImportApi;
        private readonly IAPILog _logger;
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

        public ImportApiFactory(IUserContextConfiguration userContextConfiguration, IAuthTokenGenerator tokenGenerator, ISyncToggles syncToggles, IUserService userService, IExtendedImportAPI extendedImportApi, IAPILog logger)
        {
            _userContextConfiguration = userContextConfiguration;
            _tokenGenerator = tokenGenerator;
            _syncToggles = syncToggles;
            _userService = userService;
            _extendedImportApi = extendedImportApi;
            _logger = logger;
        }
        
        public async Task<IImportAPI> CreateImportApiAsync(Uri webServiceUrl)
        {
            int executingUserId = _userContextConfiguration.ExecutingUserId;
            bool executingUserIsAdmin = await _userService.ExecutingUserIsAdminAsync(executingUserId).ConfigureAwait(false);
            if (_syncToggles.IsEnabled<EnableNonAdminSyncLinksToggle>() && !executingUserIsAdmin)
            {
                executingUserId = _ADMIN_USER_ID;
            }
            
            _logger.LogInformation("Creating IAPI as userId: {executingUserId}", executingUserId);
            IRelativityTokenProvider relativityTokenProvider = new RelativityTokenProvider(executingUserId, _tokenGenerator);

            ImportAPI importApiInstanceByToken = _extendedImportApi.CreateByTokenProvider(webServiceUrl.AbsoluteUri, relativityTokenProvider);
            return importApiInstanceByToken;
        }
    }
#pragma warning restore RG2002
}