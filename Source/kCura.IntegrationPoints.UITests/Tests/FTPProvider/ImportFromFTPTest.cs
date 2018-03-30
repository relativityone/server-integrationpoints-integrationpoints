using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.FTP;
using kCura.IntegrationPoints.UITests.Actions;
using kCura.IntegrationPoints.UITests.Common;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.FTPProvider
{
	public class ImportFromFTPTest : UiTest
	{
		private IntegrationPointsImportAction _integrationPointsAction;

//		protected override bool InstallLegalHoldApp => true;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();
			_integrationPointsAction = new IntegrationPointsImportAction(Driver, Context);
		}

		[Test, Order(1)]
		public void Test()
		{
			var model = new ImportFromFTPModel("TC_IFTP_CUS_1", "Dashboard");
			_integrationPointsAction.CreateNewImportFromFTPIntegrationPoint(model);
		}
	}
}