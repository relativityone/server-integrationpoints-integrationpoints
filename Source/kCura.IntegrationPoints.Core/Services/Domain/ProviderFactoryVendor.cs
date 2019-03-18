using System;
using System.Collections.Concurrent;
using kCura.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.Core.Services.Domain
{
	internal class ProviderFactoryVendor : IDisposable
	{
		private readonly ConcurrentDictionary<Guid, IProviderFactory> _factories = new ConcurrentDictionary<Guid, IProviderFactory>();

		private readonly IProviderFactoryLifecycleStrategy _providerFactoryStrategy;

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