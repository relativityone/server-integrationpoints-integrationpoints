using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.UITests.Configuration.Helpers;

namespace kCura.IntegrationPoints.UITests.Configuration.Models
{
    public class FieldMapModel
    {
        public FieldObject SourceFieldObject { get; set; }
        public FieldObject DestinationFieldObject { get; set; }
        public TestConstants.FieldMapMatchType AutoMapMatchType { get; set; }

    }
}
