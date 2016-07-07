using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IErrorManager
	{
		/// <summary>
		/// Creates Relativity errors.
		/// </summary>
		/// <param name="workspaceArtifactId">The artifat id of the workspace to create the error for.</param>
		/// <param name="errors">The errors to create.</param>
		void Create(int workspaceArtifactId, IEnumerable<ErrorDTO> errors);
	}
}