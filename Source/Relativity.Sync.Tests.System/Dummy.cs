using FluentAssertions;
using NUnit.Framework;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public class Dummy
	{
		[Test]
		public void ItShouldPass()
		{
			true.Should().Be(true);
		}
	}
}
