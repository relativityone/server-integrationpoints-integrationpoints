using System.Collections.Generic;
using System.Data;
using Moq;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Domain.Tests.Integration.DataProviderBuilderTests.Mocks
{
	[DataSourceProvider(DataProviderBuilderTests.PROVIDER_GUID_AS_STRING)]
	public class DataSourceProviderMock : IDataSourceProvider
	{
		public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
		{
			yield return new FieldEntry
			{
				DisplayName = "FieldEntry"
			};
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration)
		{
			var mockDataReader = new Mock<IDataReader>();
			return mockDataReader.Object;
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
		{
			var mockDataReader = new Mock<IDataReader>();
			return mockDataReader.Object;
		}
	}
}
