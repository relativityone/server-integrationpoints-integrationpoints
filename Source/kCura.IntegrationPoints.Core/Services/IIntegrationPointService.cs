﻿using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;

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
		void RetryIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, int userId);

		/// <summary>
		/// Marks an Integration Point to be stopped.
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace artifact id of the integration point.</param>
		/// <param name="integrationPointArtifactId">Integration point artifact id.</param>
		void MarkIntegrationPointToStopJobs(int workspaceArtifactId, int integrationPointArtifactId);
	}
}