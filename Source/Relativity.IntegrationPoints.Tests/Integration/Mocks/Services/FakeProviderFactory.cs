using System;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
	public class FakeProviderFactory : IProviderFactory
	{
		public IDataSourceProvider CreateProvider(Guid identifier)
		{
			return new FakeDataSourceProvider();
		}
	}
}