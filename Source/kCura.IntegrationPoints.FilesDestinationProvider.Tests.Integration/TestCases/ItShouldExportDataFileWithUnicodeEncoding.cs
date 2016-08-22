using System.Text;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportDataFileWithUnicodeEncoding : DataFileEncodingTestCaseBase
	{
		public ItShouldExportDataFileWithUnicodeEncoding() : base(Encoding.Unicode)
		{
		}
	}
}
