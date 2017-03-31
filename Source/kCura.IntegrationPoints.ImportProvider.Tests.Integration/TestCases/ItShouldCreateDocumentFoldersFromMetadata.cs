using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases.Base
{
	public class ItShouldCreateDocumentFoldersFromMetadata : LoadFileTest
	{
		public override SettingsObjects Prepare(int workspaceId)
		{
			return base.Prepare(workspaceId,
				TestConstants.Resources.CSV_WITH_FOLDERS,
				TestConstants.LoadFiles.CSV_WITH_METADATA);
		}

		public override void Verify(int workspaceId)
		{
			int expectedDocs = 3;
			List<Result<Document>> workspaceContents = DocumentService.GetAllDocuments(workspaceId, DocumentFields);
			Assert.AreEqual(expectedDocs, workspaceContents.Count);

			for (int i = 0; i < expectedDocs; i++)
			{
				Result<Document> docResult = workspaceContents[i];
				Document doc = docResult.Artifact;
				FieldValue controlNumber = docResult.Artifact.Fields.First(x => x.Name == TestConstants.FieldNames.CONTROL_NUMBER);
				FieldValue groupIdentifier = docResult.Artifact.Fields.First(x => x.Name == TestConstants.FieldNames.GROUP_IDENTIFIER);

				int docNum = i + 1;
				Assert.AreEqual(docNum.ToString(), controlNumber.ValueAsLongText);
				Assert.AreEqual($"Row-{docNum}-GroupIdentifier", groupIdentifier.ValueAsLongText);
				Assert.AreEqual($"row{docNum}-v2", docResult.Artifact.FolderName);
			}
		}
	}
}
