﻿using System;
using System.Linq;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public class FileDestinationProviderValidationMessages
	{
		public static readonly string FILE_COUNT_WARNING = "There are no items to export. Verify your source location.";
		public static readonly string PRODUCTION_NOT_EXIST = "Production does not exists.";
		public static readonly string VIEW_NOT_EXIST = "View does not exists.";
		public static readonly string ARTIFACT_NOT_EXIST = "artifact does not exists.";
		public static readonly string FIELD_MAP_NO_FIELDS = "There are no fields selected to export.";
		public static readonly string FIELD_MAP_UNKNOWN_FIELD = "field cannot be exported as it is not found in the workspace.";
		public static readonly string FIELD_MAP_ = "";
		public static readonly string RDO_INVALID_EXPORT_NATIVE_OPTION = "Export Natives or/and Include Native Files options should not be selected for RDO that does not contain File type column.";
		public static readonly string SETTINGS_IMAGES_UNKNOWN_FORMAT = "Selected images file format is unknown.";
		public static readonly string SETTINGS_IMAGES_UNKNOWN_SUBDIR_PREFIX = "Selected images subdirectory prefix is invalid.";
		public static readonly string SETTINGS_LOADFILE_UNKNOWN_ENCODING = "Selected load file encoding is unknown.";
		public static readonly string SETTINGS_NATIVES_UNKNOWN_SUBDIR_PREFIX = "Selected natives subdirectory prefix is invalid.";
		public static readonly string SETTINGS_TEXTFILES_UNKNOWN_ENCODING = "Selected text files encoding is unknown.";
		public static readonly string SETTINGS_TEXTFILES_UNKNOWN_PRECEDENCE = "Selected text files precedence is invalid.";
		public static readonly string SETTINGS_TEXTFILES_UNKNOWN_SUBDIR_PREFIX = "Selected text files subdirectory prefix is invalid.";
		public static readonly string SETTINGS_UNKNOWN_LOCATION = "Selected export location is unknown.";
	}
}