namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
    public static class TestConstants
    {
        public static class FieldNames
        {
            public const string CONTROL_NUMBER = "Control Number";
            public const string EMAIL_SUBJECT = "Email Subject";
            public const string GROUP_IDENTIFIER = "Group Identifier";
            public const string EXTRACTED_TEXT = "Extracted Text";
            public const string HAS_IMAGES = "Has Images";
            public const string HAS_NATIVES = "Has Native";
            public const string IMAGE_COUNT = "Relativity Image Count";
            public const string CUSTODIAN = "Custodian - Single Choice";
            public const string ISSUE_DESIGNATION = "Issue Designation";
            public const string FOLDER_NAME = "Folder Name";
            public const string ARTIFACT_ID = "Artifact ID";
            public const string PARENT_ARTIFACT_ID = "Parent Artifact ID";
            public const string DOCUMENT_EXTENSION = "Document Extension";
            public const string FILE_NAME = "File Name";
            public const string NATIVE_FILE = "Native File";
            public const string BATES_BEG = "Bates Beg";
            public const string FILE = "File";
            public const string FOLDER_PATH = "Folder Path";
        }

        public static class FieldTypeNames
        {
            public const string DATE = "Date";
            public const string DECIMAL = "Decimal";
            public const string FIXED_LENGTH_TEXT = "Fixed-Length Text";
            public const string LONG_TEXT = "Long Text";
        }

        public enum FieldMapMatchType
        {
            IsIdentifier,
            Name
        }

        public static class Resources
        {
            public const string CSV_WITH_METADATA = "CsvWithMetadata";
            public const string OPTICON_WITH_TEXT = "OpticonWithText";
            public const string CSV_WITH_ET_PATH = "CsvWithETPath";
            public const string CSV_WITH_NATIVE_PATH = "CsvWithNativePath";
            public const string CSV_WITH_CHOICES = "CsvWithChoices";
            public const string CSV_WITH_FOLDERS = "CsvWithFolders";
        }

        public static class LoadFiles
        {
            public const string CSV_WITH_METADATA = "CsvWithMetadata.csv";
            public const string CONCORDANCE_WITH_METADATA = "ConcordanceWithMetadata.csv";
            public const string OPTICON_WITH_TEXT = "OpticonWithText.opt";
            public const string CSV_WITH_ET_PATH = "CsvWithETPath.csv";
            public const string CSV_WITH_NATIVE_PATH = "CsvWithNativePath.csv";
            public const string CSV_WITH_CHOICES = "CsvWithChoices.csv";
        }

        public static class TestCategories
        {
            public const string STOP_JOB = "StopJob";
        }
    }
}
