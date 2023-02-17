using System.Web.Http.Filters;
using Castle.Core;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Core.Interfaces.TextSanitizer;
using kCura.IntegrationPoints.Web.Filters;
using kCura.IntegrationPoints.Web.Installers;
using NUnit.Framework;
using Relativity.API;
using static kCura.IntegrationPoint.Tests.Core.TestHelpers.WindsorContainerTestHelpers;

namespace kCura.IntegrationPoints.Web.Tests.Installers
{
    [TestFixture, Category("Unit")]
    public class InfrastructureRegistrationTests
    {
        private IWindsorContainer _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new WindsorContainer();
            _sut.AddInfrastructure();
        }

        [Test]
        public void ExceptionFilter_ShouldBeRegisteredWithProperLifestyle()
        {
            // assert
            _sut.Should()
                .HaveRegisteredSingleComponent<IExceptionFilter>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Transient, "because we need to create single exception filter instance per each attribute instance");
        }

        [Test]
        public void ExceptionFilter_ShouldBeRegisteredWithProperImplementation()
        {
            // assert
            _sut.Should().HaveRegisteredProperImplementation<IExceptionFilter, ExceptionFilter>();
        }

        [Test]
        public void FilterProvider_ShouldBeRegisteredWithProperLifestyle()
        {
            // assert
            _sut.Should()
                .HaveRegisteredSingleComponent<IFilterProvider>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Singleton);
        }

        [Test]
        public void FilterProvider_ShouldBeRegisteredWithProperImplementation()
        {
            // assert
            _sut.Should().HaveRegisteredProperImplementation<IFilterProvider, WindsorFilterProvider>();
        }

        [Test]
        public void FilterProvider_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            RegisterInstallerDependencies(_sut);

            // act & assert
            _sut.Should().ResolveWithoutThrowing<IFilterProvider>();
        }

        private void RegisterInstallerDependencies(IWindsorContainer container)
        {
            container.AddFacility<TypedFactoryFacility>();

            IRegistration[] dependencies =
            {
                CreateDummyObjectRegistration<ITextSanitizer>(),
                CreateDummyObjectRegistration<IAPILog>()
            };

            container.Register(dependencies);
        }
    }
}
