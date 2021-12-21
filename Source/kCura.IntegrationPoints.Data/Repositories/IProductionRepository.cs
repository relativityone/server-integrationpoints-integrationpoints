using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Productions.Services;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Enables interaction with Relativity Productions.
	/// </summary>
	public interface IProductionRepository
	{
		/// <summary>
		/// Retrieves production a user has access to in a workspace
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace Artifact ID.</param>
		/// <param name="productionArtifactId">Production Artifact ID.</param>
		/// <returns>Production DTO</returns>
		ProductionDTO GetProduction(int workspaceArtifactId, int productionArtifactId);

		/// <summary>
		/// Retrieves all export productions a user has access to in a workspace
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace Artifact ID.</param>
		/// <returns>List of Production DTOs</returns>
		Task<IEnumerable<ProductionDTO>> GetProductionsForExport(int workspaceArtifactId);

        /// <summary>
        /// Retrieves all import productions a user has access to in a workspace
        /// </summary>
        /// <param name="workspaceArtifactId">Workspace Artifact ID.</param>
        /// <returns>List of Production DTOs</returns>
        Task<IEnumerable<ProductionDTO>> GetProductionsForImport(int workspaceArtifactId);

		/// <summary>
		/// Creates a production in the workspace.
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace artifact ID.</param>
		/// <param name="production">An instance of Production object representing the production to be created.</param>
		/// <returns>Artifact ID of new production.</returns>
		int CreateSingle(int workspaceArtifactId, Production production);
	}
}
