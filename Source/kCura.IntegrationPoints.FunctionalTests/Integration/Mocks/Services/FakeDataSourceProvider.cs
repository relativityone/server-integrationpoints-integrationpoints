using System.Collections.Generic;
using System.Data;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    [DataSourceProvider("9A33EBEA-B4F9-4427-8AD4-5D4F35F0405A")]
    public class FakeDataSourceProvider : IDataSourceProvider
    {
        private readonly IDataReader _dataReader;

        public FakeDataSourceProvider(IDataReader dataReader)
        {
            _dataReader = dataReader;
        }

        public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
        {
            throw new System.NotImplementedException();
        }

        public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration)
        {
            throw new System.NotImplementedException();
        }

        public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
        {
            return _dataReader;
        }
    }
}
