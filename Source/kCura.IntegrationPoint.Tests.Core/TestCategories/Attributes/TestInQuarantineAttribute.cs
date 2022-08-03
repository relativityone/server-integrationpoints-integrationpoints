using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes
{
    public class TestInQuarantineAttribute : CategoryAttribute
    {
        public TestInQuarantineAttribute(TestQuarantineState state, string reason = null)
            : base(TestCategories.IN_QUARANTINE)
        {
        }
    }
}
