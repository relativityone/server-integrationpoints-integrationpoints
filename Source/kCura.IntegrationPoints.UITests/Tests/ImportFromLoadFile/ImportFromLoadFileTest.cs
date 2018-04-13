using System.IO;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Actions;
using kCura.IntegrationPoints.UITests.Auxiliary;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.ImportFromLoadFile
{
	public class ImportFromLoadFileTest : UiTest
	{
		private int _workspaceId;
		private IntegrationPointsImportFromLoadFileAction _integrationPointsAction;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			Context = new Configuration.TestContext();
			Context.WorkspaceId = 1234;
			_workspaceId = Context.GetWorkspaceId();
			CopyFilesToFileshare();
			EnsureGeneralPageIsOpened();
			_integrationPointsAction = new IntegrationPointsImportFromLoadFileAction(Driver, Context);
			Install(_workspaceId);
		}

		private void CopyFilesToFileshare()
		{
			string fileshareLocation = SharedVariables.FileshareLocation;
			string workspaceFolderName = $"EDDS{_workspaceId}";
			string sourceLocation = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestDataImportFromLoadFile");
			string destinationLocation = Path.Combine(fileshareLocation, workspaceFolderName, "DataTransfer", "Import");
			FileCopyHelper.CopyDirectory(sourceLocation, destinationLocation);
		}

		[Test, Order(1)]
		public void DocumentImportFromLoadFile_TC_ILF_DOC_1()
		{
			// TODO
		}
	}
}
