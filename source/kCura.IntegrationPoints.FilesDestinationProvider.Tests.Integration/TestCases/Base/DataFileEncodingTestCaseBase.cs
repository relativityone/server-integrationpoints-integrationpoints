using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base
{
	abstract class DataFileEncodingTestCaseBase : IExportTestCase
	{
		#region Fields

		private const string _METADATA_FORMAT = "dat";

		private readonly Encoding _expectedEncoding;

		#endregion Fields

		protected DataFileEncodingTestCaseBase(Encoding expectedEncoding)
		{
			_expectedEncoding = expectedEncoding;
		}

		public ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportFilesLocation += $"_{GetType().Name}";

			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance;
			settings.DataFileEncoding = _expectedEncoding;

			return settings;
		}
		
		public void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			var datFileInfo = directory.EnumerateFiles($"*.{_METADATA_FORMAT}", SearchOption.TopDirectoryOnly)
				.FirstOrDefault();

			Assert.IsNotNull(datFileInfo);
			Assert.That(FileEncodingDetectionHelper.GetFileEncoding(datFileInfo.FullName), Is.EqualTo(_expectedEncoding));
		}

	}
}
