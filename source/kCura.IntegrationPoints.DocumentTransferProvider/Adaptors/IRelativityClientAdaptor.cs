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
		QueryResultSet<Document> ExecuteDocumentQuery(Query<Document> query);

		/// <summary>
		/// Executes a Subset of Document Query
		/// </summary>
		/// <param name="token">The query token value provided in the QueryResult.</param>
		/// <param name="start">The starting index used to select a subset of Artifacts from the List of all Artifacts that satisfy the initial Query() call. The List index starts with 1.</param>
		/// <param name="length">The maximum number of results to return.</param>
		/// <returns>A ResultSet of Documents</returns>
		QueryResultSet<Document> ExecuteSubSetOfDocumentQuery(string token, int start, int length);

		/// <summary>
		/// Return a result set of documents give the query model
		/// </summary>
		/// <param name="document">the document to be read </param>
		/// <returns></returns>
		ResultSet<Document> ReadDocument(Document document);
	}
}