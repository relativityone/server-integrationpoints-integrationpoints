using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Installers
{
    [TestFixture]
    public class RepositoryRegistrationTests
    {
        private IWindsorContainer _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new WindsorContainer();
            _sut.AddRepositories();
        }

        [Test]
        public void IntegrationPointRepository_ShouldBeRegisteredWithProperLifestyle()
        {
            // assert
            _sut.Should()
                .HaveRegisteredSingleComponent<IIntegrationPointRepository>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void IntegrationPointRepository_ShouldBeRegisteredWithProperImplementation()
        {
            // assert
            _sut.Should()
                .HaveRegisteredProperImplementation<IIntegrationPointRepository, IntegrationPointRepository>();
        }

        [Test]
        public void IntegrationPointRepository_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            RegisterDependencies(_sut);

            // assert
            _sut.Should()
                .ResolveWithoutThrowing<IIntegrationPointRepository>();
        }

        [Test]
        public void SourceProviderRepository_ShouldBeRegisteredWithProperLifestyle()
        {
            _sut.Should()
                .HaveRegisteredSingleComponent<ISourceProviderRepository>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void SourceProviderRepository_ShouldBeRegisteredWithProperImplementation()
        {
            // assert
            _sut.Should()
                .HaveRegisteredProperImplementation<ISourceProviderRepository, SourceProviderRepository>();
        }

        [Test]
        public void SourceProviderRepository_ShouldBeResolvedWithoutThrowing()
        {
            // arrage
            RegisterDependencies(_sut);

            // assert
            _sut.Should()
                .ResolveWithoutThrowing<ISourceProviderRepository>();
        }

        [Test]
        public void DestinationProviderRepository_ShouldBeRegisteredWithProperLifestyle()
        {
            _sut.Should()
                .HaveRegisteredSingleComponent<IDestinationProviderRepository>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void DestinationProviderRepository_ShouldBeRegisteredWithProperImplementation()
        {
            // assert
            _sut.Should()
                .HaveRegisteredProperImplementation<IDestinationProviderRepository, DestinationProviderRepository>();
        }

        [Test]
        public void DestinationProviderRepository_ShouldBeResolvedWithoutThrowing()
        {
            // arrage
            RegisterDependencies(_sut);

            // assert
            _sut.Should()
                .ResolveWithoutThrowing<IDestinationProviderRepository>();
        }

        private static void RegisterDependencies(IWindsorContainer container)
        {
            IRegistration[] dependencies =
            {
                Component.For<IRelativityObjectManager>().Instance(new Mock<IRelativityObjectManager>().Object),
                Component.For<IIntegrationPointSerializer>().Instance(new Mock<IIntegrationPointSerializer>().Object),
                Component.For<IAPILog>().Instance(new Mock<IAPILog>().Object)
            };

            container.Register(dependencies);
        }
    }
}
