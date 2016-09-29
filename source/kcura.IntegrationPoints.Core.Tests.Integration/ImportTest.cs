using kCura.IntegrationPoint.Tests.Core.Templates;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Integration
{
	[TestFixture]
	[Category(kCura.IntegrationPoint.Tests.Core.Constants.INTEGRATION_CATEGORY)]
	public class ImportTest : SourceProviderTemplate
	{
		public ImportTest(): base("Import Test")
		{
		}

		[Test]
		public void TestingImportSadFace()
		{
			//Import.ImportNewDocuments(WorkspaceArtifactId, Import.GetImportTable("ImportDoc", 30));
		}
	}
}