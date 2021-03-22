using System;
using kCura.IntegrationPoints.Core.Services.Domain;
using Relativity.IntegrationPoints.Contracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
	public class FakeProviderFactoryLifecycleStrategy : IProviderFactoryLifecycleStrategy
	{
		public IProviderFactory CreateProviderFactory(Guid applicationId)
		{
			return new FakeProviderFactory();
		}

		public void OnReleaseProviderFactory(Guid applicationId)
		{
			throw new NotImplementedException();
		}
	}
}