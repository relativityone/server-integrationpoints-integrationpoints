using System;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.ExportToLoadFile;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Common
{
	public static class CustodianToEntityUtils
	{
		public static bool IsEntityOptionAvailable(SelectElement select) =>
			select
				?.Options
				?.FirstOrDefault(x => x.Text == ExportToLoadFileTransferredObjectConstants.ENTITY) != null;

		public static string GetValidTransferredObjectName(Func<bool> isEntityTransferredObjectOptionAvailable, IntegrationPointGeneralModel model)
		{
			return model.TransferredObject == ExportToLoadFileTransferredObjectConstants.ENTITY && !isEntityTransferredObjectOptionAvailable()
				? ExportToLoadFileTransferredObjectConstants.CUSTODIAN
				: model.TransferredObject;
		}
	}
}
