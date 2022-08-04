using System;
using FluentAssertions;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Utilities;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Utilities
{
    [TestFixture, Category("Unit")]
    public class CommaSeparatedNumbersToArrayConverterTests
    {
        [Test]
        public void Convert_ShouldConvertToArrayWhenStringWithSingleIntPassed()
        {
            //arrange
            string formatString = "123";

            //act
            int[] result = CommaSeparatedNumbersToArrayConverter.Convert(formatString);

            //assert
            result.Should().Contain(new[] { 123 });
        }

        [Test]
        public void Convert_ShouldConvertToArrayWhenCommaSeparatedStringPassed()
        {
            //arrange
            string formatString = "123,321,1,9999";

            //act
            int[] result = CommaSeparatedNumbersToArrayConverter.Convert(formatString);

            //assert
            result.Should().Contain(new []{ 123, 321, 1, 9999 });
        }

        [Test]
        [TestCase("")]
        [TestCase(",")]
        [TestCase("123 321,1,9999")]
        [TestCase("123;321,1,9999")]
        public void Convert_ShouldThrowWhenInvalidStringPassed(string formatString)
        {
            //act
            Action action = () => CommaSeparatedNumbersToArrayConverter.Convert(formatString);

            //assert
            action.ShouldThrow<FormatException>();
        }

        [Test]
        public void Convert_ShouldThrowWhenNullIsPassed()
        {
            //act
            Action action = 
                () => CommaSeparatedNumbersToArrayConverter.Convert(commaSeparatedNumbers: null);

            //assert
            action.ShouldThrow<NullReferenceException>();
        }
    }
}
