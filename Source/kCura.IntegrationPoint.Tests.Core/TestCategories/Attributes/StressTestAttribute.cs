using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes
{
    public class StressTestAttribute : CategoryAttribute
    {
        public StressTestAttribute() : base(TestCategories.STRESS_TEST) { }
    }
}
