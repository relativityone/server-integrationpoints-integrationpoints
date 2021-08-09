using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.DataReaderClient;
using kCura.WinEDDS;

namespace Relativity.Sync.Logging
{
	internal class NativeImportSettingsForLogging : ImportSettingsForLoggingBase
	{
		private NativeImportSettingsForLogging(Settings settings) : base(settings)
		{
			ArtifactTypeId = settings.ArtifactTypeId;
			BulkLoadFileFieldDelimiter = settings.BulkLoadFileFieldDelimiter;
			DisableControlNumberCompatibilityMode = settings.DisableControlNumberCompatibilityMode;
			DisableExtractedTextFileLocationValidation = settings.DisableExtractedTextFileLocationValidation;
			DisableNativeLocationValidation = settings.DisableNativeLocationValidation;
			DisableNativeValidation = settings.DisableNativeValidation;
			LongTextColumnThatContainsPathToFullText = settings.LongTextColumnThatContainsPathToFullText;
			MultiValueDelimiter = settings.MultiValueDelimiter;
			NestedValueDelimiter = settings.NestedValueDelimiter;
			OIFileIdMapped = settings.OIFileIdMapped;
			FileSizeMapped = settings.FileSizeMapped;
			TimeKeeperManager = settings.TimeKeeperManager;

			FolderPathSourceFieldName = RemoveSensitiveDataIfNotEmpty(settings.FolderPathSourceFieldName);
			NativeFilePathSourceFieldName = RemoveSensitiveDataIfNotEmpty(settings.NativeFilePathSourceFieldName);
			OIFileIdColumnName = RemoveSensitiveDataIfNotEmpty(settings.OIFileIdColumnName);
			OIFileTypeColumnName = RemoveSensitiveDataIfNotEmpty(settings.OIFileTypeColumnName);
			FileSizeColumn = RemoveSensitiveDataIfNotEmpty(settings.FileSizeColumn);
			FileNameColumn = RemoveSensitiveDataIfNotEmpty(settings.FileNameColumn);
			SupportedByViewerColumn = RemoveSensitiveDataIfNotEmpty(settings.SupportedByViewerColumn);
		}

		public static NativeImportSettingsForLogging CreateWithoutSensitiveData(Settings settings)
		{
			return new NativeImportSettingsForLogging(settings);
		}
		
		#region Properties

		public int ArtifactTypeId { get; set; }
		public string BulkLoadFileFieldDelimiter { get; set; }
		public bool DisableControlNumberCompatibilityMode { get; set; }
		public bool DisableExtractedTextFileLocationValidation { get; set; }
		public bool? DisableNativeLocationValidation { get; set; }
		public bool? DisableNativeValidation { get; set; }
		public string FolderPathSourceFieldName { get; set; }
		public string LongTextColumnThatContainsPathToFullText { get; set; }
		public char MultiValueDelimiter { get; set; }
		public string NativeFilePathSourceFieldName { get; set; }
		public char NestedValueDelimiter { get; set; }
		public bool OIFileIdMapped { get; set; }
		public string OIFileIdColumnName { get; set; }
		public string OIFileTypeColumnName { get; set; }
		public bool FileSizeMapped { get; set; }
		public string FileSizeColumn { get; set; }
		public string FileNameColumn { get; set; }
		public string SupportedByViewerColumn { get; set; }
		public ITimeKeeperManager TimeKeeperManager { get; set; }
		
		#endregion
	}
}
