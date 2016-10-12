using System;
using System.Linq;
using Castle.Core.Internal;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Extensions
{
	static class ExportFileExtension
	{
		internal static bool AreSettingsApplicableForProdBegBatesNameCheck(this ExportFile exportFile)
		{
			return exportFile.TypeOfExport != ExportFile.ExportType.Production &&
			       !exportFile.ImagePrecedence.IsNullOrEmpty() && exportFile.ImagePrecedence.Any(item => Convert.ToInt32(item.Value) > 0);
		}
	}
}
