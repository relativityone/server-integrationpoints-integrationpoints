using FluentAssertions;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    internal sealed class KeplerQueryHelpersTests
    {
        [TestCase(@"test", @"test")]
        [TestCase(@"test \ field", @"test \\ field")]
        [TestCase(@"test \\ field", @"test \\\\ field")]
        [TestCase(@"test 'field'", @"test \'field\'")]
        [TestCase(@"test \'field\'", @"test \\\'field\\\'")]
        [TestCase(@"matt's test field", @"matt\'s test field")]
        [TestCase(@"""my"" [test] () field", @"""my"" [test] () field")]
        public void EscapeForSingleQuotesTests(string input, string expected)
        {
            string actual = KeplerQueryHelpers.EscapeForSingleQuotes(input);
            actual.Should().Be(expected);
        }
    }
}
