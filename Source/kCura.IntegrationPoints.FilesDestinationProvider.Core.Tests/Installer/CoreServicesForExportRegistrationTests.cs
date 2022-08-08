using Castle.Core;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS.Service.Export;
using NUnit.Framework;
using static kCura.IntegrationPoint.Tests.Core.TestHelpers.WindsorContainerTestHelpers;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Installer
{
    [TestFixture, Category("Unit")]
    public class CoreServicesForExportRegistrationTests
    {
        [Test]
        public void AuditManager_ShouldBeRegisteredWithProperLifestyle()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();

            // act
            sut.AddCoreServicesForExport();

            // assert
            sut.Should()
                .HaveRegisteredSingleComponent<IAuditManager>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void AuditManager_ShouldBeRegisteredWithProperImplementation()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();

            // act
            sut.AddCoreServicesForExport();

            // assert
            sut.Should()
                .HaveRegisteredProperImplementation<IAuditManager, CoreAuditManager>();
        }

        [Test]
        public void AuditManager_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();
            RegisterDependencies(sut);

            // act
            sut.AddCoreServicesForExport();

            // assert
            sut.Should()
                .ResolveWithoutThrowing<IAuditManager>();
        }

        [Test]
        public void FieldManager_ShouldBeRegisteredWithProperLifestyle()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();

            // act
            sut.AddCoreServicesForExport();

            // assert
            sut.Should()
                .HaveRegisteredSingleComponent<IFieldManager>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void FieldManager_ShouldBeRegisteredWithProperImplementation()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();

            // act
            sut.AddCoreServicesForExport();

            // assert
            sut.Should()
                .HaveRegisteredProperImplementation<IFieldManager, CoreFieldManager>();
        }

        [Test]
        public void FieldManager_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();
            RegisterDependencies(sut);

            // act
            sut.AddCoreServicesForExport();

            // assert
            sut.Should()
                .ResolveWithoutThrowing<IFieldManager>();
        }

        [Test]
        public void WebApiServiceFactory_ShouldBeRegisteredWithProperLifestyle()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();

            // act
            sut.AddCoreServicesForExport();

            // assert
            sut.Should()
                .HaveRegisteredSingleComponent<WebApiServiceFactory>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void WebApiServiceFactory_ShouldBeRegisteredWithProperImplementation()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();

            // act
            sut.AddCoreServicesForExport();

            // assert
            sut.Should()
                .HaveRegisteredProperImplementation<WebApiServiceFactory, WebApiServiceFactory>();
        }

        [Test]
        public void CreateWebApiServiceFactoryDelegate_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();
            RegisterDependencies(sut);
            sut.Kernel.AddFacility<TypedFactoryFacility>();

            // act
            sut.AddCoreServicesForExport();

            // assert
            sut.Should()
                .ResolveWithoutThrowing<ExportServiceFactory.CreateWebApiServiceFactoryDelegate>();
        }

        [Test]
        public void CoreServiceFactory_ShouldBeRegisteredWithProperLifestyle()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();

            // act
            sut.AddCoreServicesForExport();

            // assert
            sut.Should()
                .HaveRegisteredSingleComponent<CoreServiceFactory>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void CoreServiceFactory_ShouldBeRegisteredWithProperImplementation()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();

            // act
            sut.AddCoreServicesForExport();

            // assert
            sut.Should()
                .HaveRegisteredProperImplementation<CoreServiceFactory, CoreServiceFactory>();
        }

        [Test]
        public void CreateCoreServiceFactoryDelegate_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();
            RegisterDependencies(sut);
            sut.Kernel.AddFacility<TypedFactoryFacility>();

            // act
            sut.AddCoreServicesForExport();

            // assert
            sut.Should()
                .ResolveWithoutThrowing<ExportServiceFactory.CreateCoreServiceFactoryDelegate>();
        }

        private static void RegisterDependencies(IWindsorContainer container)
        {
            RegisterExportRepositoriesDummies(container);

            IRegistration[] dependenciesRegistrations =
            {
                Component.For<CurrentUser>().Instance(new CurrentUser(0)),
                CreateDummyObjectRegistration<IRepositoryFactory>(),
            };

            container.Register(dependenciesRegistrations);
        }

        private static void RegisterExportRepositoriesDummies(IWindsorContainer container)
        {
            IRegistration[] repositoriesRegistrations =
            {
                CreateDummyObjectRegistration<IFileRepository>(),
            };

            container.Register(repositoriesRegistrations);
        }
    }
}
