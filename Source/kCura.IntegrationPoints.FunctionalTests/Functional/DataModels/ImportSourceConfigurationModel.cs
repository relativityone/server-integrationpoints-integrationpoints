namespace Relativity.IntegrationPoints.Tests.Functional.DataModels
{
    public class ImportSourceConfigurationModel
    {
        public enum ImportTypeEnum
        {
            ImageLoadFile = 1
        }

        public string HasColumnName { get; set; }

        public string EncodingType { get; set; }

        public int AsciiColumn { get; set; }

        public int AsciiQuote { get; set; }

        public int AsciiNewLine { get; set; }

        public int AsciiMultiLine { get; set; }

        public int AsciiNestedValue { get; set; }

        public string WorkspaceId { get; set; }

        public int ImportType { get; set; }

        public string LoadFile { get; set; }

        public string LineNumber { get; set; }

        public string DestinationFolderArtifactId { get; set; }

        public bool ImageImport { get; set; }

        public bool ForProduction { get; set; }

        public string AutoNumberImages { get; set; }

        public string ImportOverwriteMode { get; set; }

        public int IdentityFieldId { get; set; }

        public string ExtractedTextFieldContainsFilePath { get; set; }

        public string ExtractedTextFileEncoding { get; set; }

        public string CopyFilesToDocumentRepository { get; set; }

        public string ImageProductionNumbering { get; set; }

        public string SelectedCaseFileRepoPath { get; set; }

        public string ImportNativeFileCopyMode { get; set; }
    }
}
