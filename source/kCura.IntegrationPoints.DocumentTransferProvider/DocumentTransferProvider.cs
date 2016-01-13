namespace kCura.IntegrationPoints.DocumentTransferProvider
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
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

		/// <summary>
		/// Gets all of the artifact ids that can be batched in reads
		/// </summary>
		/// <param name="identifier">The identifying field (Control Number)</param>
		/// <param name="options">The artifactId of the saved search in string format</param>
		/// <returns>An IDataReader containing all of the saved search's document artifact ids</returns>
		public IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			// TODO: get the RSAPI client
			return new DocumentArtifactIdDataReader(null, Convert.ToInt32(options));
		}

		/// <summary>
		/// Gets the RDO's who's artifact ids exist in the entryIds list
		/// (This method is called in batches of normally 1000 entryIds)
		/// </summary>
		/// <param name="fields">The fields the user mapped</param>
		/// <param name="entryIds">The artifact ids of the documents to copy (in string format)</param>
		/// <param name="options">The saved search artifact id (unused in this method)</param>
		/// <returns>An IDataReader that contains the Document RDO's for the entryIds</returns>
		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			// TODO: get the RSAPI client
			// entry ids are for batching
			// The fields are the fields that we provided
			return new DocumentTranfserDataReader(null, entryIds.Select(x => Convert.ToInt32(x)), fields);
		}
	}
}