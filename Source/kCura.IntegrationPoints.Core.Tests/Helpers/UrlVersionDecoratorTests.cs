using System.Reflection;
using System.Diagnostics;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using NUnit.Framework;
using FluentAssertions;

namespace kCura.IntegrationPoints.Core.Tests.Helpers
{
    [TestFixture]
    public class UrlVersionDecoratorTests
    {
        private string _assemblyVersion;

        [SetUp]
        public void Setup()
        {
            _assemblyVersion = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(UrlVersionDecorator)).Location).FileVersion;
        }

        [Test]
        public void AppendVersion_ShouldAddAmpersand_WhenThereAreParameters()
        {
            // Arrange
            const string url = "someUrl.aspx?param=x";

            // Act
            string result = UrlVersionDecorator.AppendVersion(url);

            // Assert
            result.EndsWith("&v=" + _assemblyVersion).Should().BeTrue();
        }

        [Test]
        public void AppendVersion_ShouldAddQuestionMark_WhenThereAreNoParameters()
        {
            // Arrange
            const string url = "someUrl.aspx";

            // Act
            string result = UrlVersionDecorator.AppendVersion(url);

            // Assert
            result.EndsWith("?v=" + _assemblyVersion).Should().BeTrue();
        }
    }
}
