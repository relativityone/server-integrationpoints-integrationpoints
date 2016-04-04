using System;

namespace kCura.IntegrationPoints.Contracts.Models
{
	public class SourceWorkspaceDTO
	{
		private static readonly Guid _objectTypeGuid = new Guid("604B3C11-CD68-487B-929D-99106C61B562");
		public static Guid ObjectTypeGuid { get { return _objectTypeGuid; } }
		public int ArtifactId { get; set; } 
		public string Name { get; set; }
		public int SourceWorkspaceArtifactId { get; set; }
		public string SourceWorkspaceName { get; set; }
	}
}