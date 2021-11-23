using FluentAssertions;
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

		[Test]
		public void ReplaceGroupTest()
		{
			// Arrange
			string text =
				@"IAPI - Field [ExtractedText] Error: Cannot create \\files2.T005.ukso060000.relativity.one\T005C\Files\EDDS15546933\relativity_ukso060000-t005_edds15546933_10\Fields.ExtractedText\c52\35e because a file or directory with the same name already exists.";
			string regex = @"(Cannot create )(.*)( because)";
			
			// Act
			string result = text.ReplaceGroup(regex, 1, "{path}");
			
			// Assert
			result.Should()
				.Be(
					@"IAPI - Field [ExtractedText] Error: Cannot create {path} because a file or directory with the same name already exists.");
		}
		
		[Test]
		public void ReplaceGroupTest_NoMatch()
		{
			// Arrange
			string text =
				@"IAPI - Field [ExtractedText] Error: Cannot create \\files2.T005.ukso060000.relativity.one\T005C\Files\EDDS15546933\relativity_ukso060000-t005_edds15546933_10\Fields.ExtractedText\c52\35e because a file or directory with the same name already exists.";
			string regex = @"(Cannot create )(something specific)( because)";
			
			// Act
			string result = text.ReplaceGroup(regex, 1, "{path}");
			
			// Assert
			result.Should()
				.Be(text);
		}
	}
}