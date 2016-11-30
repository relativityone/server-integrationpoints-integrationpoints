using System;
using System.Linq;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public class FileDestinationProviderValidationMessages
	{
		public static readonly string FILE_COUNT_WARNING = "There are no items to export. Verify your source location.";
		public static readonly string PRODUCTION_NOT_EXIST = "Production does not exists";
		public static readonly string VIEW_NOT_EXIST = "View does not exists";

		public static readonly string RDO_INVALID_EXPORT_NATIVE_OPTION 
			= "Export Natives or/and Include Native Files options should not be selected for RDO that does not contain File type column";
	}
}