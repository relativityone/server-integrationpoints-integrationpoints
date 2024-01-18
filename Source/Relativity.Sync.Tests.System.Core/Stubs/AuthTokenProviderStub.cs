using System;
using System.Net.Http;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.Helpers;

namespace Relativity.Sync.Tests.System.Core.Stubs
{
    internal class AuthTokenProviderStub : IAuthTokenProvider
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAuthToken(string serviceName)
        {
            return Task.FromResult("SyncAuthToken");
        }

        public DelegatingHandler GetDelegatingHandler(string serviceName)
        {
            throw new NotImplementedException();
        }

        public DelegatingHandler GetDelegatingHandler(string serviceName, HttpMessageHandler innerHandler)
        {
            throw new NotImplementedException();
        }
    }
}
