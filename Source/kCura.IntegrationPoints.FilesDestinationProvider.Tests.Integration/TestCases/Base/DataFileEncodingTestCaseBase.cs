using System.IO;
using System.Text;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base
{
	internal abstract class DataFileEncodingTestCaseBase : MetadataExportTestCaseBase
	{
		#region Fields

		private readonly Encoding _expectedEncoding;

		#endregion Fields

		protected DataFileEncodingTestCaseBase(Encoding expectedEncoding)
		{
			_expectedEncoding = expectedEncoding;
		}

		public override string MetadataFormat => "dat";

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance;
			settings.DataFileEncoding = _expectedEncoding;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var datFileInfo = GetFileInfo(directory);
			Assert.That(FileEncodingDetectionHelper.GetFileEncoding(datFileInfo.FullName), Is.EqualTo(_expectedEncoding));
		}
	}
}