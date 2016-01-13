namespace kCura.IntegrationPoints.DocumentTransferProvider
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
			// identifier will be the Control_Number
			throw new System.NotImplementedException();
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			// TODO: get the RSAPI client and get field names
			// entry ids are for batching
			// The fields are the fields that we provided
			return new DocumentArtifactIdDataReader(null, Convert.ToInt32(options));
		}
	}
}