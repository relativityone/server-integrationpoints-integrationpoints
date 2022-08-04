using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases.Base;

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
            List<Document> workspaceContents = DocumentService.GetAllDocuments(workspaceId, DocumentFields);
            Assert.AreEqual(expectedDocs, workspaceContents.Count);

            for (int i = 0; i < expectedDocs; i++)
            {
                Document docResult = workspaceContents[i];

                int docNum = i + 1;
                Assert.AreEqual(docNum.ToString(), docResult.ControlNumber);
                Assert.AreEqual($"Doc {docNum} ET", docResult.ExtractedText);
            }
        }
    }
}
