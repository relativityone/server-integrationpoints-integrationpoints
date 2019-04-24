using kCura.IntegrationPoints.Core.Extensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Extensions
{
	[TestFixture]
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
	}
}