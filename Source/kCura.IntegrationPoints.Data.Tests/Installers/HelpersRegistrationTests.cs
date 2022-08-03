using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Data.Helpers;
using kCura.IntegrationPoints.Data.Installers;
using NUnit.Framework;
using Relativity.API;
using static kCura.IntegrationPoint.Tests.Core.TestHelpers.WindsorContainerTestHelpers;

namespace kCura.IntegrationPoints.Data.Tests.Installers
{
    [TestFixture, Category("Unit")]
    public class HelpersRegistrationTests
    {
        private IWindsorContainer _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new WindsorContainer();
            _sut.AddHelpers();
        }

        [Test]
        public void MassUpdateHelper_ShouldBeRegisteredWithProperLifestyle()
        {
            // assert
            _sut.Should()
                .HaveRegisteredSingleComponent<IMassUpdateHelper>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void MassUpdateHelper_ShouldBeRegisteredWithProperImplementation()
        {
            // assert
            _sut.Should()
                .HaveRegisteredProperImplementation<IMassUpdateHelper, MassUpdateHelper>();
        }

        [Test]
        public void MassUpdateHelper_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            RegisterDependencies(_sut);

            // assert
            _sut.Should()
                .ResolveWithoutThrowing<IMassUpdateHelper>();
        }

        private static void RegisterDependencies(IWindsorContainer container)
        {
            IRegistration[] dependencies =
            {
                CreateDummyObjectRegistration<IAPILog>(),
                CreateDummyObjectRegistration<IConfig>()
            };

            container.Register(dependencies);
        }
    }
}
