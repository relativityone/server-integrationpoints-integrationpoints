using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases.Base
{
    using IntegrationPoint.Tests.Core.Validators;

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
            const int expectedDocs = 3;
            IEnumerable<Document> expectedDocuments = Enumerable.Range(1, expectedDocs).Select(i => 
                new Document(new Dictionary<string, object>
                {
                    {TestConstants.FieldNames.FOLDER_NAME, $"row{i}-v2"},
                    {TestConstants.FieldNames.CONTROL_NUMBER, i.ToString()},
                    {TestConstants.FieldNames.GROUP_IDENTIFIER, $"Row-{i}-GroupIdentifier"},
                }));

            IValidator validator = new LoadFileDocumentsValidator(expectedDocuments, workspaceId)
                .ValidateWith(new DocumentFieldsValidator(TestConstants.FieldNames.GROUP_IDENTIFIER))
                .ValidateWith(new DocumentFolderNameValidator());

            validator.Validate();
        }
    }
}
