using System.IO;
using System.Text;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model;
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
		public ItShouldExportExtractedText(ConfigSettings configSettings)
		{
			_configSettings = configSettings;
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
			foreach (var item in DataFileFormatHelper.GetMetadataFileColumnValues<string>(_configSettings.LongTextFieldName, fileInfo))
			{
				Assert.That(File.Exists(item));
				var datFileInfo = new FileInfo(item);
				Assert.That(datFileInfo.Length, Is.GreaterThan(0));
				Assert.That(FileEncodingDetectionHelper.GetFileEncoding(datFileInfo.FullName), Is.EqualTo(Encoding.UTF8));
			}
		}

		#region Properties

		public override string MetadataFormat => "csv";
		private readonly ConfigSettings _configSettings;

		#endregion Properties
	}
}