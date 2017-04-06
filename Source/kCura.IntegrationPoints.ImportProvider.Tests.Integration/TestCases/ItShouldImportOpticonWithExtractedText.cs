using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases.Base
{
	public class ItShouldImportOpticonWithExtractedText : OpticonTest
	{
		public override SettingsObjects Prepare(int workspaceId)
		{
			SettingsObjects objects = base.Prepare(workspaceId,
				TestConstants.Resources.OPTICON_WITH_TEXT,
				TestConstants.LoadFiles.OPTICON_WITH_TEXT);

			return objects;
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
				FieldValue hasImages = docResult.Artifact.Fields.First(x => x.Name == TestConstants.FieldNames.HAS_IMAGES);
				FieldValue imageCount = docResult.Artifact.Fields.First(x => x.Name == TestConstants.FieldNames.IMAGE_COUNT);
				FieldValue extText = docResult.Artifact.Fields.First(x => x.Name == TestConstants.FieldNames.EXTRACTED_TEXT);

				Assert.AreEqual(ControlNumbers[i], controlNumber.ValueAsLongText);
				Assert.AreEqual(ExtractedText[i], extText.ValueAsLongText);
				Assert.AreEqual(ImageCounts[i], imageCount.ValueAsWholeNumber);
				Assert.AreEqual("Yes", hasImages.ValueAsSingleChoice.Name);
			}
		}
	}
}
