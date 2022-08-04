using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Extensions;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Extensions
{
    [TestFixture, Category("Unit")]
    public class HttpConfigurationExtensionsTests
    {
        [Test]
        public void ShouldAddExceptionLoggerToServices()
        {
            // arrange
            var httpConfiguration = new HttpConfiguration();
            var exceptionLoggerMock = new Mock<IExceptionLogger>();

            // act
            httpConfiguration.AddExceptionLogger(exceptionLoggerMock.Object);

            // assert
            httpConfiguration.Services.GetServices(typeof(IExceptionLogger)).Should()
                .Contain(exceptionLoggerMock.Object, "because this exception logger was added");
        }

        [Test]
        public void ShouldAddMessageHandler()
        {
            // arrange
            var httpConfiguration = new HttpConfiguration();
            var messageHandlerMock = new Mock<DelegatingHandler>();

            // act
            httpConfiguration.AddMessageHandler(messageHandlerMock.Object);

            // assert
            httpConfiguration.MessageHandlers.Contains(messageHandlerMock.Object).Should()
                .BeTrue("because this message handler was added");
        }

        [Test]
        public void ShouldAddWebAPIFiltersProviders()
        {
            // arrange
            var httpConfiguration = new HttpConfiguration();
            var filterProviderMock = new Mock<IFilterProvider>();

            // act
            httpConfiguration.AddWebAPIFiltersProvider(filterProviderMock.Object);

            // assert
            httpConfiguration.Services.GetServices(typeof(IFilterProvider)).Should()
                .Contain(filterProviderMock.Object, "because this filter provider was added");
        }
    }
}
