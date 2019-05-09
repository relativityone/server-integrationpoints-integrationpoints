using System;
using NUnit.Framework;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class DisposableStopwatchTests
	{
		[Test]
		public void ItShouldInvokeActionOnDispose()
		{
			// ARRANGE
			bool wasInvoked = false;
			Action<TimeSpan> action = (timeSpan) => wasInvoked = true;

			// ACT
			var stopwatch = new DisposableStopwatch(action);
			stopwatch.Dispose();

			// ASSERT
			Assert.That(wasInvoked);
		}

		[Test]
		public void ItShouldInvokeActionInUsingClause()
		{
			// ARRANGE
			bool wasInvoked = false;
			Action<TimeSpan> action = (timeSpan) => wasInvoked = true;

			// ACT
			using (new DisposableStopwatch(action))
			{
				// nothing to do, testing dispose method
			}

			// ASSERT
			Assert.That(wasInvoked);
		}
	}
}