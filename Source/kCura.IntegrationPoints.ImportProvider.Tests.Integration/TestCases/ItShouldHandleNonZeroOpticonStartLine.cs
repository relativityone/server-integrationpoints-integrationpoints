using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases.Base
{
    public class ItShouldHandleNonZeroOpticonStartLine : OpticonTest
    {
        public override SettingsObjects Prepare(int workspaceId)
        {
            SettingsObjects objects = base.Prepare(workspaceId,
                TestConstants.Resources.OPTICON_WITH_TEXT,
                TestConstants.LoadFiles.OPTICON_WITH_TEXT);

            objects.ImportProviderSettings.LineNumber = "3";
            return objects;
        }

        public override void Verify(int workspaceId)
        {
            int expectedDocs = 2;
            List<Document> workspaceContents = DocumentService.GetAllDocuments(workspaceId, DocumentFields);
            Assert.AreEqual(expectedDocs, workspaceContents.Count);

            for (int i = 0; i < expectedDocs; i++)
            {
                Document docResult = workspaceContents[i];

                Assert.AreEqual(ControlNumbers[i + 1], docResult.ControlNumber);
                Assert.AreEqual(ExtractedText[i + 1], docResult.ExtractedText);
                Assert.AreEqual(ImageCounts[i + 1], docResult.ImageCount);
                Assert.AreEqual("Yes", docResult.HasImages.Name);
            }
        }
    }
}
