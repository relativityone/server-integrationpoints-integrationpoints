using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes
{
    public class NotWorkingOnTridentAttribute : CategoryAttribute
    {
        public NotWorkingOnTridentAttribute(string reason = null) : base(TestCategories.NOT_WORKING_ON_TRIDENT)
        {
        }
    }
}
