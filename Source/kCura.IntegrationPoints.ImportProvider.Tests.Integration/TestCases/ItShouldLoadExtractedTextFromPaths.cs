using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases.Base;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases
{
	public class ItShouldLoadExtractedTextFromPaths : LoadFileTest
	{
		public override SettingsObjects Prepare(int workspaceId)
		{
			return base.Prepare(workspaceId,
				TestConstants.Resources.CSV_WITH_ET_PATH,
				TestConstants.LoadFiles.CSV_WITH_ET_PATH);
		}

		public override void Verify(int workspaceId)
		{
			int expectedDocs = 3;
			List<Result<Document>> workspaceContents = DocumentService.GetAllDocuments(workspaceId, DocumentFields);
			Assert.AreEqual(expectedDocs, workspaceContents.Count);

			for (int i = 0; i < expectedDocs; i++)
			{
				Result<Document> docResult = workspaceContents[i];
				FieldValue controlNumber = docResult.Artifact.Fields.First(x => x.Name == TestConstants.FieldNames.CONTROL_NUMBER);
				FieldValue extText = docResult.Artifact.Fields.First(x => x.Name == TestConstants.FieldNames.EXTRACTED_TEXT);

				int docNum = i + 1;
				Assert.AreEqual(docNum.ToString(), controlNumber.ValueAsLongText);
				Assert.AreEqual($"Doc {docNum} ET", extText.ValueAsLongText);
			}
		}
	}
}
