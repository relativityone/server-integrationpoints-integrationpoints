using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Data.Installers;
using NUnit.Framework;
using Relativity.API;
using static kCura.IntegrationPoint.Tests.Core.TestHelpers.WindsorContainerTestHelpers;

namespace kCura.IntegrationPoints.Data.Tests.Installers
{
    [TestFixture, Category("Unit")]
    public class RetryingMechanismRegistrationTests
    {
        private IWindsorContainer _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new WindsorContainer();
            _sut.AddRetryingMechanism();
        }

        [Test]
        public void RetryHandlerFactory_ShouldBeRegisteredWithProperLifestyle()
        {
            // assert
            _sut.Should()
                .HaveRegisteredSingleComponent<IRetryHandlerFactory>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.Singleton);
        }

        [Test]
        public void RetryHandlerFactory_ShouldBeRegisteredWithProperImplementation()
        {
            // assert
            _sut.Should()
                .HaveRegisteredProperImplementation<IRetryHandlerFactory, RetryHandlerFactory>();
        }

        [Test]
        public void RetryHandlerFactory_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            RegisterDependencies(_sut);

            // assert
            _sut.Should()
                .ResolveWithoutThrowing<IRetryHandlerFactory>();
        }

        private static void RegisterDependencies(IWindsorContainer container)
        {
            IRegistration[] dependencies =
            {
                CreateDummyObjectRegistration<IAPILog>()
            };

            container.Register(dependencies);
        }
    }
}
