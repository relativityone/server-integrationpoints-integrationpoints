using System;

namespace Relativity.IntegrationPoints.Tests.Functional.DataModels
{
    public class ImportDestinationConfigurationModel
    {
        public enum ImportTypeEnum
        {
            ImageLoadFile = 1
        }

        public int artifactTypeID { get; set; }

        public string destinationProviderType { get; set; }

        public int CaseArtifactId { get; set; }

        public bool ImageImport { get; set; }

        public bool ForProduction { get; set; }

        public string AutoNumberImages { get; set; }

        public string ImportOverwriteMode { get; set; }

        public int IdentityFieldId { get; set; }

        public string ExtractedTextFieldContainsFilePath { get; set; }

        public string ExtractedTextFileEncoding { get; set; }

        public string CopyFilesToDocumentRepository { get; set; }

        public string SelectedCaseFileRepoPath { get; set; }

        public string ImportNativeFileCopyMode { get; set; }

        public string ImageProductionNumbering { get; set; }

        public string WorkspaceId { get; set; }

        public int ImportType { get; set; }

        public string LoadFile { get; set; }

        public string LineNumber { get; set; }

        public string DestinationFolderArtifactId { get; set; }
    }
}
