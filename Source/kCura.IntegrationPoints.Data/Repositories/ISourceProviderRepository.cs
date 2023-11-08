using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Repository responsible for Source Provider object
	/// </summary>
	public interface ISourceProviderRepository : IRepository<SourceProvider>
	{
		/// <summary>
		/// Gets the Source Provider artifact id given a guid identifier
		/// </summary>
		/// <param name="sourceProviderGuidIdentifier">Guid identifier of Source Provider type</param>
		/// <returns>Artifact id of the Source Provider</returns>
		int GetArtifactIdFromSourceProviderTypeGuidIdentifier(
			string sourceProviderGuidIdentifier,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);

		Task<List<SourceProvider>> GetSourceProviderRdoByApplicationIdentifierAsync(
			Guid appGuid,
			ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
	}
}