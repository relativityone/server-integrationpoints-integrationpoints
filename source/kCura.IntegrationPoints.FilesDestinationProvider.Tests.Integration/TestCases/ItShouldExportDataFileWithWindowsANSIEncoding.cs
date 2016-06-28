using System.Text;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportDataFileWithWindowsANSIEncoding : DataFileEncodingTestCaseBase
	{
		public ItShouldExportDataFileWithWindowsANSIEncoding() : base(Encoding.Default)
		{
		}
	}
}
