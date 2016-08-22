using System;

namespace kCura.IntegrationPoints.Domain.Models
{
	public class ObjectTypeDTO
	{
		public int ArtifactId { get; set; } 
		public int ArtifactTypeId { get; set; }
		public string Name { get; set; }
		public Guid Guid { get; set; }
	}
}