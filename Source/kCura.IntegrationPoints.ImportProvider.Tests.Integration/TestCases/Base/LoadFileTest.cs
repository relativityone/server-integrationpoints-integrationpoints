using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases.Base
{
    public abstract class LoadFileTest : TestCaseBase
    {
        protected override string[] DocumentFields { get; set; }

        protected string[] NativeMD5Hashes = new string[] { "EA-0C-7B-4B-48-78-0F-8A-0A-82-0D-E2-E8-9E-39-87",
            "EF-A0-F7-D3-9C-11-06-B8-67-53-34-63-8D-2D-DC-B7",
            "ED-0D-C4-F3-5C-6D-87-9B-36-CB-A6-6D-33-F1-B1-87" };

        protected string[] CustodianSingleChoices = new string[] {
            "custodian1", "custodian1", "custodian2" };
        protected string[][] IssueMultiChoices = new string[][]
        {
            new string[] { "Priority", "Urgent", "Apples" },
            new string[] { "Priority", "Normal", "Apples" },
            new string[] { "Priority", "Urgent", "Oranges" }
        };

        protected override SettingsObjects Prepare(int workspaceId, string resourceName, string loadFileName)
        {
            DocumentFields = new string[] { TestConstants.FieldNames.CONTROL_NUMBER,
                TestConstants.FieldNames.EXTRACTED_TEXT,
                TestConstants.FieldNames.EMAIL_SUBJECT,
                TestConstants.FieldNames.GROUP_IDENTIFIER,
                TestConstants.FieldNames.FOLDER_NAME,
                TestConstants.FieldNames.CUSTODIAN,
                TestConstants.FieldNames.ISSUE_DESIGNATION };

            return base.Prepare(workspaceId, resourceName, loadFileName);
        }
    }
}
