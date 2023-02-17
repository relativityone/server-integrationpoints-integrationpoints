using FluentAssertions;
using kCura.IntegrationPoints.Core.Extensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Extensions
{
    [TestFixture]
    public class DoubleExtenstionsTests
    {
        [Test]
        public void ConvertBytesToGigabytes_ShouldProperlyConvertToGigabytes()
        {
            // ARRANGE
            const double gigabyte = 1073741824;

            // ACT
            var convertedToGigabytes = gigabyte.ConvertBytesToGigabytes();

            // ASSERT
            convertedToGigabytes.Should().Be(1.00);
        }

        [Test]
        public void ConvertBytesToGigabytes_ShouldProperlyConvertToGigabytesWithPrecision()
        {
            // ARRANGE
            const double number = 123456789;

            // ACT
            var convertedToGigabytes = number.ConvertBytesToGigabytes(5);

            // ASSERT
            convertedToGigabytes.Should().Be(0.11498);
        }

        [Test]
        public void ConvertBytesToGigabytes_ShouldProperlyConvertZeroWhenZeroArgument()
        {
            // ARRANGE
            const double number = 0;

            // ACT
            var convertedToGigabytes = number.ConvertBytesToGigabytes();

            // ASSERT
            convertedToGigabytes.Should().Be(0);
        }
    }
}
