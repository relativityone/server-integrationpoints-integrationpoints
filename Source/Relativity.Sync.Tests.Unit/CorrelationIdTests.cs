using FluentAssertions;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class CorrelationIdTests
	{
		[Test]
		public void ItShouldSetCorrelationIdValue()
		{
			const string value = "cid";

			CorrelationId correlationId = new CorrelationId(value);

			// ASSERT
			correlationId.Value.Should().Be(value);
		}

		[Test]
		public void ItShouldOverrideToStringWithCorrelationIdValue()
		{
			const string value = "cid";

			CorrelationId correlationId = new CorrelationId(value);

			// ASSERT
			correlationId.ToString().Should().Be(value);
		}
	}
}