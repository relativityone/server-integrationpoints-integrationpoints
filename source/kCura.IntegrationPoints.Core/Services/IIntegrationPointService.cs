using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IIntegrationPointService
	{
		/// <summary>
		/// Retrieves an integration point given the artifact id.
		/// </summary>
		/// <param name="artifactId">Artifact id of the integration point.</param>
		/// <returns>The integration point object.</returns>
		IntegrationPoint GetRdo(int artifactId);

		/// <summary>
		/// Retrieves all the integration points in the workspace.
		/// </summary>
		/// <returns>A list of integration point objects.</returns>
		IList<IntegrationPoint> GetAllIntegrationPoints();

		/// <summary>
		/// Retrieves the source configuration information for the given integration point artifact id.
		/// </summary>
		/// <param name="artifactId">Artifact id of the integration point.</param>
		/// <returns>The source configuration information.</returns>
		string GetSourceOptions(int artifactId);

		/// <summary>
		/// Retrieves the identifier field information for the given integration point artifact id.
		/// </summary>
		/// <param name="artifactId">Artifact id of the integration point.</param>
		/// <returns>The field entry information for the source identifier field.</returns>
		FieldEntry GetIdentifierFieldEntry(int artifactId);

		/// <summary>
		/// Retrieves an integration model of the integration point given the integration point artifact id.
		/// </summary>
		/// <param name="artifactId">Artifact id of the integration point.</param>
		/// <returns>The integration model object of the integration point.</returns>
		IntegrationModel ReadIntegrationPoint(int artifactId);

		/// <summary>
		/// Retrieves the field mapping for the integration point given the artifact id.
		/// </summary>
		/// <param name="artifactId">Artifact id of the integration point.</param>
		/// <returns>A list of field mappings for the integration point.</returns>
		IEnumerable<FieldMap> GetFieldMap(int artifactId);

		/// <summary>
		/// Creates or updates an integration point.
		/// </summary>
		/// <param name="model">The integration point model.</param>
		/// <returns>The artifact id of the integration point.</returns>
		int SaveIntegration(IntegrationModel model);

		/// <summary>
		/// Retrieves the list of email addresses associated with the integration point.
		/// </summary>
		/// <param name="artifactId">Artifact id of the integration point.</param>
		/// <returns>A list of email addresses.</returns>
		IEnumerable<string> GetRecipientEmails(int artifactId);

		/// <summary>
		/// Run integration point as a new job.
		/// </summary>
		/// <param name="workspaceArtifactId">workspace artifactId of the integration point object</param>
		/// <param name="integrationPointArtifactId">integration point artifact id</param>
		/// <param name="userId">user id of which will be used for logging</param>
		void RunIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, int userId);
	}
}