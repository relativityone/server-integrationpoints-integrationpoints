using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using Moq;

namespace kCura.IntegrationPoints.Domain.Tests.Integration.DataProviderBuilderTests.Mocks
{
	[Contracts.DataSourceProvider(DataProviderBuilderTests.PROVIDER_GUID_AS_STRING)]
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
