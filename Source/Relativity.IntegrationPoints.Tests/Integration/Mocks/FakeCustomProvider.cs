using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using System;
using System.Collections.Generic;
using System.Data;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	public class FakeCustomProvider : IDataSourceProvider
	{
		public Func<IDataReader> GetBatchableIdsFunc { get; set; }

		public Func<IDataReader> GetDataFunc { get; set; }

		public Func<IEnumerable<FieldEntry>> GetFieldsFunc { get; set; }

		public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
		{
			return GetFieldsFunc() ?? throw new NotImplementedException();
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration)
		{
			return GetDataFunc() ?? throw new NotImplementedException();
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
		{
			return GetBatchableIdsFunc() ?? throw new NotImplementedException();
		}
	}
}