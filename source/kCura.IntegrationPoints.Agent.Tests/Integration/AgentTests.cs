using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	using System.Data;
	using Apps.Common.Config;
	using Apps.Common.Data;
	using Core.Models;
	using global::Relativity.API;
	using NSubstitute;

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
			int savedSearchArtifactId = Helper.SavedSearch.CreateSavedSearch("localhost", Helper.SharedVariables.RelativityUserName, Helper.SharedVariables.RelativityPassword, workspaceArtifactId, "All Documents");
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