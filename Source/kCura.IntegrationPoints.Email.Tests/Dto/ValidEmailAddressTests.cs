using FluentAssertions;
using kCura.IntegrationPoints.Email.Dto;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Email.Tests.Dto
{
    [TestFixture, Category("Unit")]
    public class ValidEmailAddressTests
    {
        [Test]
        public void This_ShouldBeImplicitlyCastableToString()
        {
            // arrange
            string expectedEmailAddress = "relativity.admin@kcura.com";
            var validEmailAddress = new ValidEmailAddress(expectedEmailAddress);

            // act
            string emailAddress = validEmailAddress;

            // assert
            emailAddress.Should().Be(expectedEmailAddress);
        }

        [Test]
        public void ToString_ShouldReturnEmailAddress()
        {
            // arrange
            string expectedEmailAddress = "relativity.admin@kcura.com";
            var validEmailAddress = new ValidEmailAddress(expectedEmailAddress);

            // act
            string emailAddress = validEmailAddress.ToString();

            // assert
            emailAddress.Should().Be(expectedEmailAddress);
        }
    }
}
