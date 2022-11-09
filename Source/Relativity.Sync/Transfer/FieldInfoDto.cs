using System;

namespace Relativity.Sync.Transfer
{
    internal sealed class FieldInfoDto : IEquatable<FieldInfoDto>
    {
        #region Display name constants

        private const string _FOLDER_PATH_DESTINATION_DISPLAY_NAME = "FolderPath_76B270CB-7CA9-4121-B9A1-BC0D655E5B2D";
        private const string _NATIVE_FILE_FILENAME_DISPLAY_NAME = "NativeFileFilename";
        private const string _NATIVE_FILE_SIZE_DISPLAY_NAME = "NativeFileSize";
        private const string _NATIVE_FILE_LOCATION_DISPLAY_NAME = "NativeFileLocation";
        private const string _SUPPORTED_BY_VIEWER_DISPLAY_NAME = "SupportedByViewer";
        private const string _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME = "RelativityNativeType";
        private const string _IMAGE_FILE_NAME_DISPLAY_NAME = "ImageFileName";
        private const string _IMAGE_FILE_LOCATION_DISPLAY_NAME = "ImageFileLocation";
        private const string _IMAGE_IDENTIFIER_DISPLAY_NAME = "ImageIdentifier";

        #endregion

        internal FieldInfoDto(SpecialFieldType specialFieldType, string sourceFieldName, string destinationFieldName, bool isIdentifier, bool isDocumentField)
        {
            if (isDocumentField && string.IsNullOrWhiteSpace(sourceFieldName))
            {
                throw new ArgumentException($"Source field ({nameof(sourceFieldName)}) name cannot be empty when creating document ({nameof(isDocumentField)}) field.", nameof(sourceFieldName));
            }
            SpecialFieldType = specialFieldType;
            SourceFieldName = sourceFieldName;
            DestinationFieldName = destinationFieldName;
            IsDocumentField = isDocumentField;
            IsIdentifier = isIdentifier;
        }

        public SpecialFieldType SpecialFieldType { get; }

        public string SourceFieldName { get; }

        public string DestinationFieldName { get; }

        public bool IsDocumentField { get; }

        public bool IsIdentifier { get; }

        public RelativityDataType RelativityDataType { get; set; }

        public int DocumentFieldIndex { get; set; } = -1;

        public bool Equals(FieldInfoDto other)
        {
            if (other == null)
            {
                return false;
            }

            return
                ReferenceEquals(this, other) ||
                SpecialFieldType == other.SpecialFieldType &&
                SourceFieldName.Equals(other.SourceFieldName, StringComparison.InvariantCulture) &&
                DestinationFieldName.Equals(other.DestinationFieldName, StringComparison.InvariantCulture) &&
                IsDocumentField == other.IsDocumentField &&
                RelativityDataType == other.RelativityDataType &&
                IsIdentifier == other.IsIdentifier &&
                DocumentFieldIndex == other.DocumentFieldIndex;
        }

        public override bool Equals(object obj)
        {
            if (obj is FieldInfoDto other)
            {
                return Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                const int hashFactor = 397;
                var hashCode = (int)SpecialFieldType;
                hashCode = (hashCode * hashFactor) ^ (SourceFieldName != null ? SourceFieldName.GetHashCode() : 0);
                hashCode = (hashCode * hashFactor) ^ (DestinationFieldName != null ? DestinationFieldName.GetHashCode() : 0);
                hashCode = (hashCode * hashFactor) ^ IsDocumentField.GetHashCode();
                hashCode = (hashCode * hashFactor) ^ IsIdentifier.GetHashCode();
                return hashCode;
            }
        }

        #region Factory methods

        public static FieldInfoDto GenericSpecialField(SpecialFieldType specialFieldType, string sourceFieldName, string destinationFieldName)
        {
            return new FieldInfoDto(specialFieldType, sourceFieldName, destinationFieldName, false, false);
        }

        public static FieldInfoDto DocumentField(string sourceFieldName, string destinationFieldName, bool isIdentifier, RelativityDataType dataType = RelativityDataType.FixedLengthText)
        {
            return new FieldInfoDto(SpecialFieldType.None, sourceFieldName, destinationFieldName, isIdentifier, true)
            {
                RelativityDataType = dataType
            };
        }

        public static FieldInfoDto FolderPathFieldFromDocumentField(string displayName)
        {
            return new FieldInfoDto(SpecialFieldType.FolderPath, displayName, _FOLDER_PATH_DESTINATION_DISPLAY_NAME, false, true);
        }

        public static FieldInfoDto FolderPathFieldFromSourceWorkspaceStructure()
        {
            return new FieldInfoDto(SpecialFieldType.FolderPath, string.Empty, _FOLDER_PATH_DESTINATION_DISPLAY_NAME, false, false);
        }

        public static FieldInfoDto NativeFileNameField()
        {
            return new FieldInfoDto(SpecialFieldType.NativeFileFilename, string.Empty, _NATIVE_FILE_FILENAME_DISPLAY_NAME, false, false);
        }

        public static FieldInfoDto NativeFileSizeField()
        {
            return new FieldInfoDto(SpecialFieldType.NativeFileSize, string.Empty, _NATIVE_FILE_SIZE_DISPLAY_NAME, false, false);
        }

        public static FieldInfoDto NativeFileLocationField()
        {
            return new FieldInfoDto(SpecialFieldType.NativeFileLocation, string.Empty, _NATIVE_FILE_LOCATION_DISPLAY_NAME, false, false);
        }

        public static FieldInfoDto SupportedByViewerField()
        {
            return new FieldInfoDto(SpecialFieldType.SupportedByViewer, _SUPPORTED_BY_VIEWER_DISPLAY_NAME, _SUPPORTED_BY_VIEWER_DISPLAY_NAME, false, true);
        }

        public static FieldInfoDto RelativityNativeTypeField()
        {
            return new FieldInfoDto(SpecialFieldType.RelativityNativeType, _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME, _RELATIVITY_NATIVE_TYPE_DISPLAY_NAME, false, true);
        }

        public static FieldInfoDto ImageFileNameField()
        {
            return new FieldInfoDto(SpecialFieldType.ImageFileName, string.Empty, _IMAGE_FILE_NAME_DISPLAY_NAME, false, false);
        }

        public static FieldInfoDto ImageFileLocationField()
        {
            return new FieldInfoDto(SpecialFieldType.ImageFileLocation, string.Empty, _IMAGE_FILE_LOCATION_DISPLAY_NAME, false, false);
        }

        public static FieldInfoDto ImageIdentifierField()
        {
            return new FieldInfoDto(SpecialFieldType.ImageIdentifier, string.Empty, _IMAGE_IDENTIFIER_DISPLAY_NAME, false, false);
        }

        #endregion
    }
}
