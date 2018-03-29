using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.Import
{
	public class ImportFromFTPTest : UiTest
	{
		private IntegrationPointsAction _integrationPointsAction;

//		protected override bool InstallLegalHoldApp => true;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();
			_integrationPointsAction = new IntegrationPointsAction(Driver, Context);
		}

		[Test, Order(1)]
		public void Test()
		{
			var model = new ImportFromFTPModel("TC_IFTP_CUS_1");
			model.TransferredObject = "Dashboard";

			_integrationPointsAction.CreateNewImportFromFTPIntegrationPoint(model);
		}
	}
}