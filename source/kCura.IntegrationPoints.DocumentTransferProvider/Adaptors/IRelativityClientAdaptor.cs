using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Adaptors
{
	public interface IRelativityClientAdaptor
	{
		ResultSet<Document> ExecuteDocumentQuery(Query<Document> query);
	}
}