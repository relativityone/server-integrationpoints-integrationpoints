using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors.Implementations;
using kCura.IntegrationPoints.DocumentTransferProvider.DataReaders;
using kCura.Relativity.Client;

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
			return new List<FieldEntry>();
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
			return new DocumentArtifactIdDataReader(new RelativityClientAdaptor(this.CreateClient(1)), Convert.ToInt32(options));
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
			return new DocumentTranfserDataReader(new RelativityClientAdaptor(this.CreateClient(1)), entryIds.Select(x => Convert.ToInt32(x)), fields);
		}

		private IRSAPIClient CreateClient(int workspaceId)
		{
			// Create a new instance of RSAPIClient. The first parameter indicates the endpoint Uri, 
			// which indicates the scheme to use. The second parameter indicates the
			// authentication type. The RSAPIClient members page in the Services API class library
			// documents other possible constructors. The constructor also ensures a logged in session.

			string localHostFQDN = System.Net.Dns.GetHostEntry("localhost").HostName;
			Uri endpointUri = new Uri(string.Format("http://{0}/relativity.services", localHostFQDN));
			IRSAPIClient rsapiClient = new RSAPIClient(endpointUri, new IntegratedAuthCredentials());
			rsapiClient.APIOptions.WorkspaceID = workspaceId;
			return rsapiClient;
		}
	}
}