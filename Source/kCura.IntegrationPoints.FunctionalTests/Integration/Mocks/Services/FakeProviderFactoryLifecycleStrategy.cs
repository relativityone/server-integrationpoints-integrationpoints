using System;
using kCura.IntegrationPoints.Core.Services.Domain;
using Relativity.IntegrationPoints.Contracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    public class FakeProviderFactoryLifecycleStrategy : IProviderFactoryLifecycleStrategy
    {
        private readonly IProviderFactory _providerFactory;

        public FakeProviderFactoryLifecycleStrategy(IProviderFactory providerFactory)
        {
            _providerFactory = providerFactory;
        }

        public IProviderFactory CreateProviderFactory(Guid applicationId)
        {
            return _providerFactory;
        }

        public void OnReleaseProviderFactory(Guid applicationId)
        {
        }
    }
}