using System.Net.Mail;
using FluentAssertions;
using kCura.IntegrationPoints.Email.Dto;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Email.Tests
{
    [TestFixture, Category("Unit")]
    public class SmtpClientFactoryTests
    {
        [Test]
        public void ShouldGetClientForValidInput()
        {
            // arrange
            var emailConfiguration = new SmtpClientSettings
            (
                domain: "testDomain",
                port: 1234,
                useSsl: true,
                userName: "testUser",
                password: "testPass"
            );
            var clientFactory = new SmtpClientFactory();

            // act
            SmtpClient client = clientFactory.Create(emailConfiguration);

            // assert
            AssertSmptClientWasConstructedWithProperSettings(client, emailConfiguration);
        }

        [Test]
        public void ShouldGetClientForNullUsername()
        {
            // arrange
            var emailConfiguration = new SmtpClientSettings
            (
                domain: "testDomain",
                port: 1234,
                useSsl: true,
                userName: null,
                password: "testPass"
            );
            var clientFactory = new SmtpClientFactory();

            // act
            SmtpClient client = clientFactory.Create(emailConfiguration);

            // assert
            AssertSmptClientWasConstructedWithProperSettings(client, emailConfiguration);
        }

        [Test]
        public void ShouldGetClientForNullPassword()
        {
            // arrange
            var emailConfiguration = new SmtpClientSettings
            (
                domain: "testDomain",
                port: 1234,
                useSsl: true,
                userName: "user",
                password: null
            );
            var clientFactory = new SmtpClientFactory();

            // act
            SmtpClient client = clientFactory.Create(emailConfiguration);

            // assert
            AssertSmptClientWasConstructedWithProperSettings(client, emailConfiguration);
        }

        private static void AssertSmptClientWasConstructedWithProperSettings(SmtpClient client, SmtpClientSettings emailConfiguration)
        {
            client.Host.Should().Be(emailConfiguration.Domain);
            client.Credentials.Should().NotBeNull();
            client.Port.Should().Be(emailConfiguration.Port);
            client.EnableSsl.Should().Be(emailConfiguration.UseSSL);
        }
    }
}
