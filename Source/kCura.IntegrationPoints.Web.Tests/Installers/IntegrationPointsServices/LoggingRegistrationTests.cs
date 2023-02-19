using Castle.Core;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Web.Installers.IntegrationPointsServices;
using kCura.IntegrationPoints.Web.IntegrationPointsServices.Logging;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Installers.IntegrationPointsServices
{
    [TestFixture, Category("Unit")]
    public class LoggingRegistrationTests
    {
        private IWindsorContainer _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new WindsorContainer();
            _sut = _sut.AddLoggingContext();
        }

        [Test]
        public void ICacheHolder_ShouldBeRegisteredWithProperLifestyle()
        {
            _sut.Should()
                .HaveRegisteredSingleComponent<ICacheHolder>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Singleton);
        }

        [Test]
        public void ICacheHolder_ShouldBeRegisteredWithProperImplementation()
        {
            _sut.Should().HaveRegisteredProperImplementation<ICacheHolder, CacheHolder>();
        }

        [Test]
        public void IWebCorrelationContextProvider_ShouldBeRegisteredWithProperLifestyle()
        {
            _sut.Should()
                .HaveRegisteredSingleComponent<IWebCorrelationContextProvider>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.PerWebRequest);
        }

        [Test]
        public void IWebCorrelationContextProvider_ShouldBeRegisteredWithProperImplementation()
        {
            _sut.Should().HaveRegisteredProperImplementation<IWebCorrelationContextProvider, WebActionContextProvider>();
        }

        [Test]
        public void IWebCorrelationContextProvider_ShouldBeResolvedAndNotThrow()
        {
            // arrange
            var sut = new WindsorContainer();
            sut.ConfigureChangingLifestyleFromPerWebRequestToTransientBecausePerWebRequestIsNotResolvableInTests();
            sut.AddLoggingContext();

            // act & assert
            sut.Should().ResolveWithoutThrowing<IWebCorrelationContextProvider>();
        }
    }
}
