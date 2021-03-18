using System;
using kCura.IntegrationPoints.Core.Services.Domain;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
	public class FakeProviderFactoryLifecycleStrategy : IProviderFactoryLifecycleStrategy
	{
		public IProviderFactory CreateProviderFactory(Guid applicationId)
		{
			throw new NotImplementedException();
		}

		public void OnReleaseProviderFactory(Guid applicationId)
		{
			throw new NotImplementedException();
		}
	}
}