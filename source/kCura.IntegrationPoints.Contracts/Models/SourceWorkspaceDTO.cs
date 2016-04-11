using System;

namespace kCura.IntegrationPoints.Contracts.Models
{
	public class SourceWorkspaceDTO
	{
		private static readonly Guid _objectTypeGuid = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");
		public static Guid ObjectTypeGuid { get { return _objectTypeGuid; } }
		public int ArtifactTypeId { get; set; } 
		public int ArtifactId { get; set; } 
		public string Name { get; set; }
		public int SourceCaseArtifactId { get; set; }
		public string SourceCaseName { get; set; }
	}
}