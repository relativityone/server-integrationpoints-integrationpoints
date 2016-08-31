using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class ResourcePoolManager : IResourcePoolManager
	{
		public List<ProcessingSourceLocationDTO> GetProcessingSourceLocation(int workspaceId)
		{
			// Just a temoral solution (mock) to not block UI work
			List<ProcessingSourceLocationDTO> processingSourceLocations = new List<ProcessingSourceLocationDTO>
			{
				new ProcessingSourceLocationDTO
				{
					ArtifactId = 1,
					Location = "\\localhost"
				}
			};
			return processingSourceLocations;
		}
	}
}
