using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
	public interface IIntegrationPointService
	{
		/// <summary>
		/// Retrieves all the integration points in the workspace.
		/// </summary>
		/// <returns>A list of integration point objects.</returns>
		IList<Data.IntegrationPoint> GetAllRDOs();

		/// <summary>
		/// Retrieves all the integration points in the workspace including all data fields.
		/// </summary>
		/// <returns>A list of integration point objects.</returns>
		IList<Data.IntegrationPoint> GetAllRDOsWithAllFields();

		/// <summary>
		/// Retrieves the identifier field information for the field map.
		/// </summary>
		/// <param name="fieldMap">Field map.</param>
		/// <returns>The field entry information for the source identifier field.</returns>
		FieldEntry GetIdentifierFieldEntry(string fieldMap);

		/// <summary>
		/// Retrieves an integration model of the integration point given the integration point artifact id.
		/// </summary>
		/// <param name="artifactID">Artifact id of the integration point.</param>
		/// <returns>The integration model object of the integration point.</returns>
		IntegrationPointModel ReadIntegrationPointModel(int artifactID);

		/// <summary>
		/// Retrieves an integration point given the integration point artifact id.
		/// </summary>
		/// <param name="artifactID">Artifact id of the integration point.</param>
		/// <returns>The integration point object.</returns>
		Data.IntegrationPoint ReadIntegrationPoint(int artifactID);

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
		int SaveIntegration(IntegrationPointModel model);

		/// <summary>
		/// Updates an integration point.
		/// </summary>
		/// <param name="integrationPoint">The integration point.</param>
		void UpdateIntegrationPoint(Data.IntegrationPoint integrationPoint);

		/// <summary>
		/// Run integration point as a new job.
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace artifact id of the integration point.</param>
		/// <param name="integrationPointArtifactId">Integration point artifact id.</param>
		/// <param name="userId">User id of the user running the job.</param>
		void RunIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, int userId);

		/// <summary>
		/// Retry integration point as a new job.
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace artifact id of the integration point.</param>
		/// <param name="integrationPointArtifactId">Integration point artifact id.</param>
		/// <param name="userId">User id of the user running the job.</param>
		void RetryIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, int userId, bool switchToAppendOverlayMode);

		/// <summary>
		/// Marks an Integration Point to be stopped.
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace artifact id of the integration point.</param>
		/// <param name="integrationPointArtifactId">Integration point artifact id.</param>
		void MarkIntegrationPointToStopJobs(int workspaceArtifactId, int integrationPointArtifactId);
	}
}