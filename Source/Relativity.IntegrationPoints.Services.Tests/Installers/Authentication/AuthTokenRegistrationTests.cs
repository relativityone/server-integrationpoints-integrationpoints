using Castle.Core;
using Castle.Windsor;
using Relativity.IntegrationPoints.Services.Installers.Authentication;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Domain.Authentication;
using NUnit.Framework;

namespace Relativity.IntegrationPoints.Services.Tests.Installers.Authentication
{
    [TestFixture, Category("Unit")]
    public class AuthTokenRegistrationTests
    {
        private IWindsorContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = new WindsorContainer();
            _container.AddAuthTokenGenerator();
        }

        [Test]
        public void IAuthTokenGenerator_ShouldBeRegisteredWithProperLifestyle()
        {
            // assert
            _container.Should()
                .HaveRegisteredSingleComponent<IAuthTokenGenerator>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void IAuthTokenGenerator_ShouldBeRegisteredWithProperImplementation()
        {
            // assert
            _container.Should().HaveRegisteredProperImplementation<IAuthTokenGenerator, ClaimsTokenGenerator>();
        }

        [Test]
        public void IAuthTokenGenerator_ShouldBeResolvedAndNotThrow()
        {
            // assert
            _container.Should().ResolveWithoutThrowing<IAuthTokenGenerator>();
        }
    }
}
