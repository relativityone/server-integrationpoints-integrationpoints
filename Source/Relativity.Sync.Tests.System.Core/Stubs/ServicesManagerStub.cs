using System;
using Relativity.API;
using Relativity.Services.Interfaces.Helpers;
using Relativity.Services.ServiceProxy;

namespace Relativity.Sync.Tests.System.Core.Stubs
{
    public class ServicesManagerStub : IServicesMgr
    {
        public Uri GetServicesURL()
        {
	        throw new NotSupportedException("Should be deprecated to take out RsapiServices");
        }

        public Uri GetRESTServiceUrl()
        {
            return AppSettings.RelativityRestUrl;
        }

        public T CreateProxy<T>(ExecutionIdentity ident) where T : IDisposable
        {
            if (typeof(T) == typeof(IAuthTokenProvider))
            {
                return (T)(new AuthTokenProviderStub() as IAuthTokenProvider);
            }

            var userCredential = new UsernamePasswordCredentials(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword);
            var userSettings = new ServiceFactorySettings(AppSettings.RelativityRestUrl, userCredential);
            var userServiceFactory = new ServiceFactory(userSettings);
            return userServiceFactory.CreateProxy<T>();
        }
    }
}
