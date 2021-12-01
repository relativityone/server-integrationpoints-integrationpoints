using System;
using Relativity.API;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
    public class FakeServiceManager : IServicesMgr
    {
        public Uri GetServicesURL()
        {
            throw new NotImplementedException();
        }

        public T CreateProxy<T>(ExecutionIdentity ident) where T : IDisposable
        {
            throw new NotImplementedException();
        }

        public Uri GetRESTServiceUrl()
        {
            throw new NotImplementedException();
        }
    }
}
