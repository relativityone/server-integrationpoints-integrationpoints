using System.Collections.Generic;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core
{
	public struct ExportSettings
	{
		public int ExportedObjArtifactId { get; set; }
		public int WorkspaceId { get; set; }
		public string ExportFilesLocation { get; set; }
		public List<int> SelViewFieldIds { get; set; }
		public int ArtifactTypeId { get; set; }
	}
}
