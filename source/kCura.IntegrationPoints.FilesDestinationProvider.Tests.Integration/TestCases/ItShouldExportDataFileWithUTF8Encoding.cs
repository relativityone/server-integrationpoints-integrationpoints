using System.Text;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportDataFileWithUtf8Encoding : DataFileEncodingTestCaseBase
	{
		public ItShouldExportDataFileWithUtf8Encoding() : base(Encoding.UTF8)
		{
		}
	}
}
