﻿using System.Text;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportDataFileWithBigEndianEncoding : DataFileEncodingTestCaseBase
	{
		public ItShouldExportDataFileWithBigEndianEncoding() : base(Encoding.BigEndianUnicode)
		{
		}
	}
}
