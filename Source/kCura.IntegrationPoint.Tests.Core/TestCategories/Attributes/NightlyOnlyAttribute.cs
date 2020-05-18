using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes
{
	public class NightlyOnlyAttribute : CategoryAttribute
	{
		public NightlyOnlyAttribute() : base(TestCategories.NIGHTLY_ONLY)
		{
		}
	}
}