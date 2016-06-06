using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	class ItShouldExportDataFileWithUnicodeEncoding : DataFileEncodingTestCaseBase
	{
		public ItShouldExportDataFileWithUnicodeEncoding() : base(Encoding.Unicode)
		{
		}
	}
}
