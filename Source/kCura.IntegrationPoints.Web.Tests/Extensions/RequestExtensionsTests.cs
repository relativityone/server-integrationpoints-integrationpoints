using System.Web;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Extensions;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Extensions
{
    [TestFixture, Category("Unit")]
    public class RequestExtensionsTests
    {
        private const string _DEFAULT_APPLICATION_PATH = "/Relativity";

        [Test]
        public void ShouldReturnDefaultPathIfApplicationPathIsNull()
        {
            // arrange
            HttpRequestBase request = CreateHttpRequestBaseMockWithApplicationPath(applicationPath: null);

            // act
            string result = request.GetApplicationRootPath();

            // assert
            result.Should().Be(_DEFAULT_APPLICATION_PATH);
        }

        [Test]
        public void ShouldReturnDefaultPathIfApplicationPathIsWhiteSpace()
        {
            // arrange
            HttpRequestBase request = CreateHttpRequestBaseMockWithApplicationPath(applicationPath: "     ");

            // act
            string result = request.GetApplicationRootPath();

            // assert
            result.Should().Be(_DEFAULT_APPLICATION_PATH);
        }

        [TestCase("/Relativity/IntegrationPoints// ", "Relativity")]
        [TestCase("/IntegrationPoints/dcf", "IntegrationPoints")]
        [TestCase("//IntegrationPoints/dcf", "IntegrationPoints")]
        [TestCase("/    /IntegrationPoints/dcf", "IntegrationPoints")]
        [TestCase("IntegrationPoints", "IntegrationPoints")]
        [TestCase("/IntegrationPoints", "IntegrationPoints")]
        public void ShouldReturnCorrectApplicationRootPath(string applicationPath, string expectedRootPath)
        {
            // arrange
            HttpRequestBase request = CreateHttpRequestBaseMockWithApplicationPath(applicationPath);

            // act
            string result = request.GetApplicationRootPath();

            // assert
            result.Should().Be(expectedRootPath);
        }

        private HttpRequestBase CreateHttpRequestBaseMockWithApplicationPath(string applicationPath)
        {
            var mock = new Mock<HttpRequestBase>();
            mock.Setup(x => x.ApplicationPath).Returns(applicationPath);
            return mock.Object;
        }
    }
}
