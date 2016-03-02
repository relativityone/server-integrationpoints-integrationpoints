using System.Collections.Generic;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Models
{
	public class ArtifactDTO
	{
		public int ArtifactId { get; set; }
		public int ArtifactTypeId { get; set; }

		public IList<ArtifactFieldDTO> Fields { get; set; }
	}
}
