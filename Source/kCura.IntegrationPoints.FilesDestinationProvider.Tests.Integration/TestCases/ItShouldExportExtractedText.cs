using System.IO;
using System.Text;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process.Internals;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	/// <summary>
	///     This is a case when we select Extracted Text field in Text Precendence list. Extract Text field is also selected in
	///     the header metadata file
	///     As the output "Extract Text" column should appear in the metadafile
	/// </summary>
	internal class ItShouldExportExtractedText : MetadataExportTestCaseBase
	{
		private readonly ExportTestConfiguration _testConfiguration;

		public ItShouldExportExtractedText(ExportTestConfiguration testConfiguration)
		{
			_testConfiguration = testConfiguration;
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.CSV;
			settings.ExportFullTextAsFile = true;
			settings.TextFileEncodingType = Encoding.UTF8;
			settings.FilePath = ExportSettings.FilePathType.Absolute;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var fileInfo = GetFileInfo(directory);
			foreach (var item in DataFileFormatHelper.GetMetadataFileColumnValues<string>(_testConfiguration.LongTextFieldName, fileInfo))
			{
				Assert.That(File.Exists(item));
				var datFileInfo = new FileInfo(item);
				Assert.That(datFileInfo.Length, Is.GreaterThan(0));
				Assert.That(FileEncodingDetectionHelper.GetFileEncoding(datFileInfo.FullName), Is.EqualTo(Encoding.UTF8));
			}
		}
		
		public override string MetadataFormat => "csv";
	}
}