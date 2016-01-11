﻿namespace kCura.IntegrationPoints.DocumentTransferProvider
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using Contracts.Provider;
	using Contracts.Models;
	using Shared;

	[Contracts.DataSourceProvider(Constants.PROVIDER_GUID)]
	public class DocumentTransferProvider : IDataSourceProvider
	{
		public IEnumerable<FieldEntry> GetFields(string options)
		{
			throw new System.NotImplementedException();
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			throw new System.NotImplementedException();
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			throw new NotImplementedException();
		}
	}
}