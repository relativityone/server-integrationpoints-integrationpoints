using FluentAssertions;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public static class SyncJobParametersTests
	{
		[Test]
		public static void CorrelationIdShouldNotBeEmpty()
		{
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1);

			// ASSERT
			syncJobParameters.CorrelationId.Should().NotBeNullOrWhiteSpace();
		}

		[Test]
		public static void CorrelationIdShouldBeInitializedWithGivenValue()
		{
			const string id = "example id";

			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1, id);

			// ASSERT
			syncJobParameters.CorrelationId.Should().Be(id);
		}
	}
}