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
		/// Return a long text field value of a given document
		/// </summary>
		/// <param name="documentArtifactId">The document's artifact Id</param>
		/// <param name="longTextFieldArtifactId">The long text field's artifact Id</param>
		/// <returns></returns>
		String GetLongTextFieldValue(int documentArtifactId, int longTextFieldArtifactId);
	}
}