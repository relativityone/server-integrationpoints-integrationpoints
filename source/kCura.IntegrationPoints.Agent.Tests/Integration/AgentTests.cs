using System.Data;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	[TestFixture]
	public class AgentTests : IntegrationTestBase
	{
		[Test]
		[Explicit]
		public void CreateJob()
		{
		}

		[Test]
		[Ignore]
		public void Testing123()
		{
			IDBContext context = Substitute.For<IDBContext>();
			IHelper helper = NSubstitute.Substitute.For<IHelper>();
			helper.GetDBContext(-1).Returns(context);
			Manager.Settings.Factory = new HelperConfigSqlServiceFactory(helper);

			int workspaceArtifactId = Helper.Workspace.CreateWorkspace("Testing Integration5", "New Case Template");
			Helper.Workspace.ImportApplicationToWorkspace(workspaceArtifactId, @"C:\SourceCode\IntegrationPoints\source\bin\Application\RelativityIntegrationPoints.Auto.rap", true);
			Helper.Import.ImportNewDocuments(workspaceArtifactId, GetImportTable());
			int savedSearchArtifactId = Helper.SavedSearch.CreateSavedSearch("localhost", SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, workspaceArtifactId, "All Documents");
			IntegrationModel integrationModel = new IntegrationModel();
			integrationModel.SourceProvider = savedSearchArtifactId;
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
	}
}