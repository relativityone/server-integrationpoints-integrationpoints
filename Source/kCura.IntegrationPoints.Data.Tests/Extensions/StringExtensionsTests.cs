using System;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Extensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Extensions
{
    [TestFixture, Category("Unit")]
    public class StringExtensionsTests
    {
        [TestCase("")]
        [TestCase("abc")]
        [TestCase("a`b")]
        [TestCase("\"ab\"")]
        public void EscapeSingleQuote_ShouldNotModifyStringWithoutSingleQuote(string input)
        {
            // act
            string result = input.EscapeSingleQuote();

            // assert
            result.Should().Be(input, "because input does not contain single quote");
        }

        [Test]
        public void EscapeSingleQuote_ShouldThrowExceptionForNullValue()
        {
            // arrange
            string input = null;

            // act 
            Action escapeSingleQuoteAction = () => input.EscapeSingleQuote();

            // assert
            escapeSingleQuoteAction.ShouldThrow<ArgumentNullException>();
        }

        [TestCase("'", @"\'")]
        [TestCase("a'", @"a\'")]
        [TestCase("'c", @"\'c")]
        [TestCase("a'c", @"a\'c")]
        [TestCase("''c", @"\'\'c")]
        [TestCase("'a'c", @"\'a\'c")]
        public void EscapeSingleQuote_Should(string input, string expectedResult)
        {
            // act
            string result = input.EscapeSingleQuote();

            // assert
            result.Should().Be(expectedResult, "becuase single quote was present in input");
        }
    }
}
