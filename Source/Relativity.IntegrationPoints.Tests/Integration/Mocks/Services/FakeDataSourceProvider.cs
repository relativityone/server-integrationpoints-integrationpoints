﻿using System.Collections.Generic;
using System.Data;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
	public class FakeDataSourceProvider : IDataSourceProvider
	{
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
			return new FakeDataReader();
		}
	}
}