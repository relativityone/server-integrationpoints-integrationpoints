using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Productions.Services;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IProductionManager
	{
		/// <summary>
		/// Retrieves all productions a user has access to in a workspace
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace Artifact ID</param>
		/// <param name="productionArtifactId">Production Artifact ID</param>
		/// <returns>Productions DTO</returns>
		ProductionDTO RetrieveProduction(int workspaceArtifactId, int productionArtifactId);

        /// <summary>
		/// Creates a production in the workspace.
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace artifact ID.</param>
		/// <param name="production">An instance of Production object representing the production to be created.</param>
		/// <returns>Artifact ID of new production.</returns>
		int CreateSingle(int workspaceArtifactId, Production production);

		/// <summary>
		/// Retrieves all productions eligible for export
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace Artifact ID</param>
		/// <returns>Productions eligible for export</returns>
		IEnumerable<ProductionDTO> GetProductionsForExport(int workspaceArtifactId);

		/// <summary>
		/// Retrieves all productions eligible for import
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace Artifact ID</param>
		/// <param name="federatedInstanceId">Federated Instance Artifact ID</param>
		/// <param name="federatedInstanceCredentials">Federated Instance Credentials</param>
		/// <returns>Productions eligible for import</returns>
		IEnumerable<ProductionDTO> GetProductionsForImport(int workspaceArtifactId, int? federatedInstanceId = null, string federatedInstanceCredentials = null);

		bool IsProductionInDestinationWorkspaceAvailable(int workspaceArtifactId, int productionId,
			int? federatedInstanceId = null, string federatedInstanceCredentials = null);

		bool IsProductionEligibleForImport(int workspaceArtifactId, int productionId,
			int? federatedInstanceId = null, string federatedInstanceCredentials = null);
	}
}
