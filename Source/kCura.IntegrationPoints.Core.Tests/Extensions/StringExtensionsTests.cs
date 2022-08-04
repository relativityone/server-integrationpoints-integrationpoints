using FluentAssertions;
using kCura.IntegrationPoints.Core.Extensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Extensions
{
    [TestFixture, Category("Unit")]
    public class StringExtensionsTest
    {
        [TestCase(@"c:\foo", @"c:", ExpectedResult = true)]
        [TestCase(@"c:\foo", @"c:\", ExpectedResult = true)]
        [TestCase(@"c:\foo", @"c:\foo", ExpectedResult = true)]
        [TestCase(@"c:\foo", @"c:\foo\", ExpectedResult = true)]
        [TestCase(@"c:\foo\", @"c:\foo", ExpectedResult = true)]
        [TestCase(@"c:\foo\bar\", @"c:\foo\", ExpectedResult = true)]
        [TestCase(@"c:\foo\bar", @"c:\foo\", ExpectedResult = true)]
        [TestCase(@"c:\foo\a.txt", @"c:\foo", ExpectedResult = true)]
        [TestCase(@"c:\FOO\a.txt", @"c:\foo", ExpectedResult = false)]
        [TestCase(@"c:/foo/a.txt", @"c:\foo", ExpectedResult = true)]
        [TestCase(@"c:\foobar", @"c:\foo", ExpectedResult = false)]
        [TestCase(@"c:\foobar\a.txt", @"c:\foo", ExpectedResult = false)]
        [TestCase(@"c:\foobar\a.txt", @"c:\foo\", ExpectedResult = false)]
        [TestCase(@"c:\foo\a.txt", @"c:\foobar", ExpectedResult = false)]
        [TestCase(@"c:\foo\a.txt", @"c:\foobar\", ExpectedResult = false)]
        [TestCase(@"c:\foo\..\bar\baz", @"c:\foo", ExpectedResult = false)]
        [TestCase(@"c:\foo\..\bar\baz", @"c:\bar", ExpectedResult = true)]
        [TestCase(@"c:\foo\..\bar\baz", @"c:\barr", ExpectedResult = false)]
        public bool IsSubPathOfTest(string path, string baseDirPath)
        {
            return path.IsSubPathOf(baseDirPath);
        }

        [TestCase("test", "test")]
        [TestCase("   ", "   ")]
        [TestCase(null, null)]
        [TestCase("", null)]
        public void NullIfEmpty_ReturnsExpectedValue_WhenStringIsPassed(string str, string expectedResult)
        {
            // act
            string result = str.NullIfEmpty();

            // assert
            result.Should().Be(expectedResult);
        }

        [TestCase("test    ", "test")]
        [TestCase("test test", "testtest")]
        [TestCase("   ", "")]
        [TestCase(null, null)]
        [TestCase("", "")]
        public void TrimAll_ShoudRemoveAllWhiteSpacesFromTheString(string str, string expectedResult)
        {
            // Act
            string result = str.TrimAll();

            // Assert
            result.Should().Be(expectedResult);
        }
    }
}