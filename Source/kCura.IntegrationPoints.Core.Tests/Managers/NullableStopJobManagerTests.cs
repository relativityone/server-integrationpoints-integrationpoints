using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class NullableStopJobManagerTests : TestBase
	{
		private NullableStopJobManager _instance;

		[SetUp]
		public override void SetUp()
		{
			_instance = new NullableStopJobManager();
		}

		[Test]
		public void SyncRoot_NotNull()
		{
			Assert.IsNotNull(_instance.SyncRoot);
		}

		[Test]
		public void IsStopRequested_AlwaysReturnFalse()
		{
			Assert.IsFalse(_instance.IsStopRequested());
		}

		[Test]
		public void ThrowIfStopRequested_DoesNotThrowException()
		{
			Assert.DoesNotThrow(_instance.ThrowIfStopRequested);
		}

		[Test]
		public void Dispose_DoesNotThrowException()
		{
			Assert.DoesNotThrow(_instance.Dispose);
			Assert.DoesNotThrow(_instance.Dispose);
		}
	}
}