using kCura.IntegrationPoint.Tests.Core.Models.FTP;
using kCura.IntegrationPoints.UITests.Actions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.FTPProvider
{
	public class ImportFromFTPTest : UiTest
	{
		private IntegrationPointsImportFTPAction _integrationPointsAction;

//		protected override bool InstallLegalHoldApp => true;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();
			_integrationPointsAction = new IntegrationPointsImportFTPAction(Driver, Context);
		}

		[Test, Order(1)]
		public void Test()
		{
			var model = new ImportFromFTPModel("TC_IFTP_CUS_1", "Dashboard");
			_integrationPointsAction.CreateNewImportFromFTPIntegrationPoint(model);
		}
	}
}