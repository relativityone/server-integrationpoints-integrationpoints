using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process.Internals
{
	public class ExportTestContext
	{
		public int WorkspaceID { get; set; }

		public int ViewID { get; set; }

		public int ExportedObjArtifactID { get; set; }

		public int ProductionArtifactID { get; set; }

		public FieldEntry[] DefaultFields { get; set; }

		public FieldEntry LongTextField { get; set; }

		public string WorkspaceName { get; set; }

		public DocumentsTestData DocumentsTestData { get; set; }
	}
}