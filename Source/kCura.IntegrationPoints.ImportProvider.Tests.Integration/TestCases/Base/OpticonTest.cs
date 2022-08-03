using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases.Base
{
    public abstract class OpticonTest : TestCaseBase
    {
        protected override string[] DocumentFields { get; set; }
        protected string[] ControlNumbers = new string[] { "REL0", "REL3", "REL5" };
        protected string[] ExtractedText = new string[] { "Doc 1 Page 1 ETDoc 1 Page 2 ETDoc 1 Page 3 ET",
            "Doc 2 Page 1 ETDoc 2 Page 2 ET",
            "Doc 3 Page 1 ETDoc 3 Page 2 ET" };
        protected int[] ImageCounts = new int[] { 3, 2, 2 };

        protected override SettingsObjects Prepare(int workspaceId, string resourceName, string loadFileName)
        {
            DocumentFields = new string[] { TestConstants.FieldNames.CONTROL_NUMBER,
                TestConstants.FieldNames.EXTRACTED_TEXT,
                TestConstants.FieldNames.HAS_IMAGES,
                TestConstants.FieldNames.IMAGE_COUNT };

            return base.Prepare(workspaceId, resourceName, loadFileName);
        }
    }
}
