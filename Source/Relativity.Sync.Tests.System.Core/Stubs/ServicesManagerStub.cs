using System;
using Relativity.API;
using Relativity.Services.ServiceProxy;

namespace Relativity.Sync.Tests.System.Core.Stubs
{
    public class ServicesManagerStub : IServicesMgr
    {
        public Uri GetServicesURL()
        {
            return AppSettings.RsapiServicesUrl;
        }

        public Uri GetRESTServiceUrl()
        {
            return AppSettings.RelativityRestUrl;
        }

        public T CreateProxy<T>(ExecutionIdentity ident) where T : IDisposable
        {
            var userCredential = new UsernamePasswordCredentials(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword);
            var userSettings = new ServiceFactorySettings(AppSettings.RelativityRestUrl, userCredential);
            var userServiceFactory = new ServiceFactory(userSettings);
            return userServiceFactory.CreateProxy<T>();
        }
    }
}
