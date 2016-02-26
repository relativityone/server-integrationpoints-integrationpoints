using System;
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

		public QueryResultSet<Document> ExecuteDocumentQuery(Query<Document> query)
		{
			QueryResultSet<Document> results = _rsapiClient.Repositories.Document.Query(query);
			return results;
		}

		public QueryResultSet<Document> ExecuteSubSetOfDocumentQuery(string token, int start, int length)
		{
			QueryResultSet<Document> results = _rsapiClient.Repositories.Document.QuerySubset(token, start, length);
			return results;
		}

		public ResultSet<Document> ReadDocument(Document document)
		{
			ResultSet<Document> results = _rsapiClient.Repositories.Document.Read(document);
			return results;
		}
	}
}