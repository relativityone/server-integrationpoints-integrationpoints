using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
    public interface IIntegrationPointService
    {
        /// <summary>
        /// Retrieves all the integration points in the workspace.
        /// </summary>
        /// <returns>A list of integration point objects.</returns>
        List<IntegrationPointSlimDto> ReadAllSlim();

        /// <summary>
        /// Retrieves all the integration points in the workspace.
        /// </summary>
        /// <returns>A list of integration point objects.</returns>
        List<IntegrationPointDto> ReadAll();

        /// <summary>
        /// Retrieves an integration point with all fields including long text fields (fields mapping, source configuration, destination configuration)
        /// </summary>
        /// <param name="artifactID">Artifact id of the integration point.</param>
        /// <returns>The integration model object of the integration point.</returns>
        IntegrationPointDto Read(int artifactID);

        /// <summary>
        /// Retrieves an integration point WITHOUT long text fields (fields mapping, source configuration, destination configuration)
        /// </summary>
        IntegrationPointSlimDto ReadSlim(int artifactID);

        /// <summary>
        /// Retrieves the field mapping for the integration point given the artifact id.
        /// </summary>
        /// <param name="artifactId">Artifact id of the integration point.</param>
        /// <returns>A list of field mappings for the integration point.</returns>
        List<FieldMap> GetFieldMap(int artifactId);

        /// <summary>
        /// Retrieves the source configuration field value for given integration point.
        /// </summary>
        /// <param name="artifactId">Artifact id of the integration point.</param>
        string GetSourceConfiguration(int artifactId);

        /// <summary>
        /// Retrieves the destination configuration field value for given integration point.
        /// </summary>
        /// <param name="artifactId">Artifact id of the integration point.</param>
        DestinationConfiguration GetDestinationConfiguration(int artifactId);

        /// <summary>
        /// Retrieves the calculation state field value for given integration point.
        /// </summary>
        /// <param name="artifactId">Artifact id of the integration point.</param>
        CalculationState GetCalculationState(int artifactId);

        /// <summary>
        /// Gets all integration points with given source and destination provider.
        /// </summary>
        List<IntegrationPointSlimDto> GetBySourceAndDestinationProvider(int sourceProviderArtifactID, int destinationProviderArtifactID);

        /// <summary>
        /// Creates or updates an integration point.
        /// </summary>
        /// <param name="dto">The integration point model.</param>
        /// <returns>The artifact id of the integration point.</returns>
        int SaveIntegrationPoint(IntegrationPointDto dto);

        /// <summary>
        /// Updates last runtime and next runtime.
        /// </summary>
        void UpdateLastAndNextRunTime(int artifactId, DateTime? lastRuntime, DateTime? nextRuntime);

        /// <summary>
        /// Disables scheduler for given integration point.
        /// </summary>
        /// <param name="artifactId">Integration point artifactId.</param>
        void DisableScheduler(int artifactId);

        /// <summary>
        /// Updates job history.
        /// </summary>
        void UpdateJobHistory(int artifactId, List<int> jobHistory);

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
