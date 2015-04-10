using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class NativeFileImportService
	{
		public bool ImportNativeFiles { get; set; }
		public string SourceFieldName { get; set; }
		public string DestinationFieldName { get; set; }

		public NativeFileImportService()
		{
			ImportNativeFiles = false;
			DestinationFieldName = "NATIVE_FILE_PATH_001";
		}
	}
}
