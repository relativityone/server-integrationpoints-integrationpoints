using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core
{
	[TestFixture]
	public abstract class TestBase
	{
		[OneTimeSetUp]
		public virtual void FixtureSetUp()
		{
			SetUp();
		}

		[SetUp]
		public abstract void SetUp();

	}
}