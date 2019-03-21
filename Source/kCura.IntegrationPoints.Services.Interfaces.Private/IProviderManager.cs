using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace kCura.IntegrationPoints.Services
{
	[WebService("Source Provider Manager")]
	[ServiceAudience(Audience.Private)]
	public interface IProviderManager : IKeplerService, IDisposable
	{
		/// <summary>
		/// Get Artifact Id of the source provider object in the given workspace.
		/// </summary>
		/// <param name="workspaceArtifactId">The Workspace artifact Id of which has installed integration point application</param>
		/// <param name="sourceProviderGuidIdentifier">The source provider guid identifier that we want the artifact id of</param>
		/// <returns>The Artifact id of the source provider specified</returns>
		Task<int> GetSourceProviderArtifactIdAsync(int workspaceArtifactId, string sourceProviderGuidIdentifier);

		/// <summary>
		/// Get Artifact Id of the destination provider object in the given workspace.
		/// </summary>
		/// <param name="workspaceArtifactId">The Workspace artifact Id of which has installed integration point application</param>
		/// <param name="destinationProviderGuidIdentifier">The destination provider guid identifier that we want the artifact id of</param>
		/// <returns>The Artifact id of the destination provider specified</returns>
		Task<int> GetDestinationProviderArtifactIdAsync(int workspaceArtifactId, string destinationProviderGuidIdentifier);

		/// <summary>
		/// Get all existing source providers
		/// </summary>
		/// <param name="workspaceArtifactId">The Workspace artifact Id of which has installed integration point application</param>
		/// <returns>All source providers</returns>
		Task<IList<ProviderModel>> GetSourceProviders(int workspaceArtifactId);

		/// <summary>
		/// Get all existing destination providers
		/// </summary>
		/// <param name="workspaceArtifactId">The Workspace artifact Id of which has installed integration point application</param>
		/// <returns>All destination providers</returns>
		Task<IList<ProviderModel>> GetDestinationProviders(int workspaceArtifactId);

		// TODO
		Task<bool> InstallProvider(InstallProviderRequest request);

		// TODO
		Task<bool> UninstallProvider(UninstallProviderRequest request);
	}
}