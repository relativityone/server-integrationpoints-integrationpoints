using System;
using System.Collections.Concurrent;
using Relativity.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.Core.Services.Domain
{
    public class ProviderFactoryVendor : IDisposable
    {
        private readonly ConcurrentDictionary<Guid, IProviderFactory> _factories = new ConcurrentDictionary<Guid, IProviderFactory>();

        private readonly IProviderFactoryLifecycleStrategy _providerFactoryStrategy;

        internal ProviderFactoryVendor() { } // this constructor is required to mock this class 

        public ProviderFactoryVendor(IProviderFactoryLifecycleStrategy providerFactoryStrategy)
        {
            _providerFactoryStrategy = providerFactoryStrategy;
        }

        public virtual IProviderFactory GetProviderFactory(Guid applicationId)
        {
            return _factories.GetOrAdd(applicationId, _providerFactoryStrategy.CreateProviderFactory);
        }

        public virtual void Dispose()
        {
            foreach (Guid applicationId in _factories.Keys)
            {
                _providerFactoryStrategy.OnReleaseProviderFactory(applicationId);
            }
            _factories.Clear();
        }
    }
}