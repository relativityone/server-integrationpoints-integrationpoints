using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Adaptors.Implementations
{
	public class RelativityClientAdaptor : IRelativityClientAdaptor
	{
		private readonly IRSAPIClient _rsapiClient;

		public RelativityClientAdaptor(IRSAPIClient rsapiClient)
		{
			_rsapiClient = rsapiClient;
		}

		public ResultSet<Document> ExecuteDocumentQuery(Query<Document> query)
		{
			ResultSet<Document> results = _rsapiClient.Repositories.Document.Query(query);

			return results;
		}
	}
}