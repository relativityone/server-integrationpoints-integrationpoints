using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes
{
    public class SmokeTestAttribute : CategoryAttribute
    {
        public SmokeTestAttribute() : base(TestCategories.SMOKE_TEST)
        {
        }
    }
}
