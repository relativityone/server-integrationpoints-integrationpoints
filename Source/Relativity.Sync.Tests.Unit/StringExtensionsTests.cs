using NUnit.Framework;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	public class StringExtensionsTests
	{
		[Test]
		[TestCase("", ExpectedResult = "")]
		[TestCase("hello world", ExpectedResult = "hello world")]
		[TestCase("hello world hello people hello country hello someone goodbye", ExpectedResult = "hello world hello peop...try hello someone goodbye")]
		[TestCase("hello world hello people hello country hello end50", ExpectedResult = "hello world hello people hello country hello end50")]
		public string TruncateTests(string testString)
		{
			// Act
			string actualString = StringExtensions.LimitLength(testString);
			
			// Assert
			return actualString;
		}
	}
}