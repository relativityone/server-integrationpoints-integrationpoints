using System;

namespace Relativity.Sync.Transfer
{
	internal sealed class FieldInfoDto
	{
		#region Display name constants

		private const string _RELATIVITY_SOURCE_CASE_DISPLAY_NAME = "Relativity Source Case";
		private const string _RELATIVITY_SOURCE_JOB_DISPLAY_NAME = "Relativity Source Job";
		private const string _FOLDER_PATH_FROM_WORKSPACE_DISPLAY_NAME = "76B270CB-7CA9-4121-B9A1-BC0D655E5B2D";
		private const string _NATIVE_FILE_FILENAME_DISPLAY_NAME = "NativeFileFilename";
		private const string _NATIVE_FILE_SIZE_DISPLAY_NAME = "NativeFileSize";
		private const string _NATIVE_FILE_LOCATION_DISPLAY_NAME = "NativeFileLocation";
		private const string _SUPPORTED_BY_VIEWER_DISPLAY_NAME = "SupportedByViewer";
		private const string _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME = "RelativityNativeType";

		#endregion

		private FieldInfoDto(SpecialFieldType specialFieldType, string displayName, bool isIdentifier, bool isDocumentField)
		{
			SpecialFieldType = specialFieldType;
			DisplayName = displayName;
			IsDocumentField = isDocumentField;
			IsIdentifier = isIdentifier;
		}

		public SpecialFieldType SpecialFieldType { get; }

		public string DisplayName { get; }

		public bool IsDocumentField { get; }

		public RelativityDataType RelativityDataType { get; set; }

		public bool IsIdentifier { get; set; }

		public int DocumentFieldIndex { get; set; } = -1;

		#region Factory methods

		public static FieldInfoDto GenericSpecialField(SpecialFieldType specialFieldType, string displayName)
		{
			return new FieldInfoDto(specialFieldType, displayName, false, false);
		}

		public static FieldInfoDto DocumentField(string displayName, bool isIdentifier)
		{
			return new FieldInfoDto(SpecialFieldType.None, displayName, isIdentifier, true);
		}

		public static FieldInfoDto SourceWorkspaceField()
		{
			return new FieldInfoDto(SpecialFieldType.SourceWorkspace, _RELATIVITY_SOURCE_CASE_DISPLAY_NAME, false, false);
		}

		public static FieldInfoDto SourceJobField()
		{
			return new FieldInfoDto(SpecialFieldType.SourceJob, _RELATIVITY_SOURCE_JOB_DISPLAY_NAME, false, false);
		}

		public static FieldInfoDto FolderPathFieldFromDocumentField(string displayName)
		{
			return new FieldInfoDto(SpecialFieldType.FolderPath, displayName, false, true);
		}

		public static FieldInfoDto FolderPathFieldFromSourceWorkspaceStructure()
		{
			return new FieldInfoDto(SpecialFieldType.FolderPath, _FOLDER_PATH_FROM_WORKSPACE_DISPLAY_NAME, false, false);
		}

		public static FieldInfoDto NativeFileFilenameField()
		{
			return new FieldInfoDto(SpecialFieldType.NativeFileFilename, _NATIVE_FILE_FILENAME_DISPLAY_NAME, false, false);
		}

		public static FieldInfoDto NativeFileSizeField()
		{
			return new FieldInfoDto(SpecialFieldType.NativeFileSize, _NATIVE_FILE_SIZE_DISPLAY_NAME, false, false);
		}

		public static FieldInfoDto NativeFileLocationField()
		{
			return new FieldInfoDto(SpecialFieldType.NativeFileLocation, _NATIVE_FILE_LOCATION_DISPLAY_NAME, false, false);
		}

		public static FieldInfoDto SupportedByViewerField()
		{
			return new FieldInfoDto(SpecialFieldType.SupportedByViewer, _SUPPORTED_BY_VIEWER_DISPLAY_NAME, false, true);
		}

		public static FieldInfoDto RelativityNativeTypeField()
		{
			return new FieldInfoDto(SpecialFieldType.RelativityNativeType, _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME, false, true);
		}

		#endregion
	}
}