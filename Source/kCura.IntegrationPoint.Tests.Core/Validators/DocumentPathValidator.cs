using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.Relativity.Client.DTOs;
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

		public void ValidateDocument(Document actualDocument, Document expectedDocument)
		{
			string expectedFolderPath = _expectedDocumentPathStrategy.GetFolderPath(expectedDocument);
			string actualFolderPath = _actualFolderPathStrategy.GetFolderPath(actualDocument);
			Assert.That(expectedFolderPath == actualFolderPath,
				"Document with Control Number {0} hass different path than expected. Expected {1}; Actual {2};",
				GetControlNumber(expectedDocument), expectedFolderPath, actualFolderPath);
		}

		private static FieldValue GetControlNumber(Document document)
		{
			return document[TestConstants.FieldNames.CONTROL_NUMBER];
		}

		public static DocumentPathValidator CreateForField(int expectedDocsWorkspaceId, IFolderManager folderManager)
		{
			var actualFolderPathStrategy = new FolderPathFromFolderTreeStrategy(expectedDocsWorkspaceId, folderManager);
			var expectedFolderPathStrategy = new FolderPathFromFieldStrategy("Document Folder Path");

			return new DocumentPathValidator(expectedFolderPathStrategy, actualFolderPathStrategy);
		}

		public static DocumentPathValidator CreateForFolderTree(int expectedDocsWorkspaceId, int actualDocsWorkspaceId, IFolderManager folderManager)
		{
			var actualFolderPathStrategy = new FolderPathFromFolderTreeStrategy(actualDocsWorkspaceId, folderManager);
			var expectedFolderPathStrategy = new FolderPathFromFolderTreeStrategy(expectedDocsWorkspaceId, folderManager);

			return new DocumentPathValidator(expectedFolderPathStrategy, actualFolderPathStrategy);
		}

		public static DocumentPathValidator CreateForRoot(int expectedDocsWorkspaceId, IFolderManager folderManager)
		{
			var actualFolderPathStrategy = new FolderPathFromFolderTreeStrategy(expectedDocsWorkspaceId, folderManager);
			var expectedFolderPathStrategy = new FolderPathIsRootStrategy();

			return new DocumentPathValidator(expectedFolderPathStrategy, actualFolderPathStrategy);
		}
	}
}