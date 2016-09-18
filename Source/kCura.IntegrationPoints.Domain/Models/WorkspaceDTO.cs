using System;

namespace kCura.IntegrationPoints.Domain.Models
{
	[Serializable]
	public class WorkspaceDTO
	{
		public int ArtifactId { get; set; } 
		public string Name { get; set; }
	}
}