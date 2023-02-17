using NUnit.Framework;
using Relativity.Services.Folder;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
    public class DocumentPathValidator : IDocumentValidator
    {
        private readonly IFolderPathStrategy _expectedDocumentPathStrategy;
        private readonly IFolderPathStrategy _actualFolderPathStrategy;

        protected DocumentPathValidator(IFolderPathStrategy expectedDocumentPathStrategy, IFolderPathStrategy actualFolderPathStrategy)
        {
            _expectedDocumentPathStrategy = expectedDocumentPathStrategy;
            _actualFolderPathStrategy = actualFolderPathStrategy;
        }

        public void ValidateDocument(Document destinationDocument, Document sourceDocument)
        {
            string expectedFolderPath = _expectedDocumentPathStrategy.GetFolderPath(sourceDocument);
            string actualFolderPath = _actualFolderPathStrategy.GetFolderPath(destinationDocument);
            Assert.That(string.Equals(expectedFolderPath, actualFolderPath),
                "Document with Control Number {0} has different path than expected. Expected {1}; Actual {2};",
                sourceDocument.ControlNumber, expectedFolderPath, actualFolderPath);
        }

        public static DocumentPathValidator CreateForField(int actualDocsWorkspaceId, IFolderManager folderManager)
        {
            var actualFolderPathStrategy = new FolderPathFromFolderTreeStrategy(actualDocsWorkspaceId, folderManager);
            var expectedFolderPathStrategy = new FolderPathFromFieldStrategy("Document Folder Path");

            return new DocumentPathValidator(expectedFolderPathStrategy, actualFolderPathStrategy);
        }

        public static DocumentPathValidator CreateForFolderTree(int expectedDocsWorkspaceId, int actualDocsWorkspaceId, IFolderManager folderManager)
        {
            var actualFolderPathStrategy = new FolderPathFromFolderTreeStrategy(actualDocsWorkspaceId, folderManager);
            var expectedFolderPathStrategy = new FolderPathFromFolderTreeStrategy(expectedDocsWorkspaceId, folderManager, true);

            return new DocumentPathValidator(expectedFolderPathStrategy, actualFolderPathStrategy);
        }

        public static DocumentPathValidator CreateForRoot(int expectedDocsWorkspaceId, IFolderManager folderManager, string folderName = "")
        {
            var actualFolderPathStrategy = new FolderPathFromFolderTreeStrategy(expectedDocsWorkspaceId, folderManager);
            var expectedFolderPathStrategy = new FolderPathIsRootStrategy(folderName);

            return new DocumentPathValidator(expectedFolderPathStrategy, actualFolderPathStrategy);
        }
    }
}
