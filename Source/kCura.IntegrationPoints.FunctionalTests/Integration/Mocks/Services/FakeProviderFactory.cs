using System;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    public class FakeProviderFactory : IProviderFactory
    {
        private readonly IDataSourceProvider _dataSourceProvider;

        public FakeProviderFactory(IDataSourceProvider dataSourceProvider)
        {
            _dataSourceProvider = dataSourceProvider;
        }

        public IDataSourceProvider CreateProvider(Guid identifier)
        {
            return _dataSourceProvider;
        }
    }
}