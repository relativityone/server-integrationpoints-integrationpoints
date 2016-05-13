using System.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	[TestFixture]
	[Explicit]
	public class AgentTests : WorkspaceDependentTemplate
	{
		[Test]
		[Explicit]
		public void CreateJob()
		{
		}

		[Test]
		[Explicit]
		public void Testing123()
		{
			IIntegrationPointService service = Container.Resolve<IIntegrationPointService>();

			IntegrationModel model = new IntegrationModel();
			model.SourceProvider = 
			service.SaveIntegration(model);
			var ips = service.GetAllIntegrationPoints();
			

			Assert.IsNotNull(ips);
			//IDBContext context = Substitute.For<IDBContext>();
			//IHelper helper = NSubstitute.Substitute.For<IHelper>();
			//helper.GetDBContext(-1).Returns(context);
			//Manager.Settings.Factory = new HelperConfigSqlServiceFactory(helper);

			//int workspaceArtifactId = GerronHelper.Workspace.CreateWorkspace("Testing Integration5", "New Case Template");
			//GerronHelper.Workspace.ImportApplicationToWorkspace(workspaceArtifactId, SharedVariables.RapFileLocation, true);
			//GerronHelper.Import.ImportNewDocuments(workspaceArtifactId, GetImportTable());
			//int savedSearchArtifactId = GerronHelper.SavedSearch.CreateSavedSearch("localhost", SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, workspaceArtifactId, "All Documents");
			//IntegrationModel integrationModel = new IntegrationModel();
			//integrationModel.SourceProvider = savedSearchArtifactId;
		}

		private DataTable GetImportTable()
		{
			DataTable table = new DataTable();
			table.Columns.Add("Control Number", typeof(string));
			//table.Columns.Add("NATIVE_FILE_PATH_001", typeof(string));
			//table.Columns.Add("Parent Document ID", typeof(string));
			table.Rows.Add("Doc1");//, "C:\\important3.txt");//, "");
			table.Rows.Add("Doc2");//, "C:\\important4.txt");//, "");
								   // table.Rows.Add("Doc2", "C:\\addressomg.txt", "Doc2");
			return table;
		}

		public AgentTests() : base ("source", "target")
		{
		}
	}
}