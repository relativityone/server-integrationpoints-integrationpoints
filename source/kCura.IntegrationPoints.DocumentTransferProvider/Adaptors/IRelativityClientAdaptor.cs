using kCura.Relativity.Client.DTOs;
using System;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Adaptors
{
	/// <summary>
	/// Wraps the RSAPI functionality in order to increase testability and remove direct dependency on the RSAPI client itself
	/// </summary>
	public interface IRelativityClientAdaptor
	{
		/// <summary>
		/// Executes a Document Query
		/// </summary>
		/// <param name="query">The Query to execute</param>
		/// <returns>A ResultSet of Documents</returns>
		ResultSet<Document> ExecuteDocumentQuery(Query<Document> query);

		/// <summary>
		/// Return a result set of documents give the query model
		/// </summary>
		/// <param name="document">the document to be read </param>
		/// <returns></returns>
		ResultSet<Document> ReadDocument(Document document);
	}
}