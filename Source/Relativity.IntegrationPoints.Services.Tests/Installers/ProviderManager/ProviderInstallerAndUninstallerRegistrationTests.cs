﻿using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.Core.Provider.Internals;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Services.Installers.ProviderManager;
using Relativity.Toggles;
using static kCura.IntegrationPoint.Tests.Core.TestHelpers.WindsorContainerTestHelpers;

namespace Relativity.IntegrationPoints.Services.Tests.Installers.ProviderManager
{
    [TestFixture]
    [Category("Unit")]
    public class ProviderInstallerAndUninstallerRegistrationTests
    {
        private IWindsorContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = new WindsorContainer();
            _container.AddProviderInstallerAndUninstaller();
        }

        [Test]
        public void IIntegrationPointsRemover_ShouldBeRegisteredWithProperLifestyle()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().HaveRegisteredSingleComponent<IIntegrationPointsRemover>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void IIntegrationPointsRemover_ShouldBeRegisteredWithProperImplementation()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().HaveRegisteredProperImplementation<IIntegrationPointsRemover, IntegrationPointsRemover>();
        }

        [Test]
        public void IIntegrationPointsRemover_ShouldBeResolvedAndNotThrow()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().ResolveWithoutThrowing<IIntegrationPointsRemover>();
        }

        [Test]
        public void IApplicationGuidFinder_ShouldBeRegisteredWithProperLifestyle()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().HaveRegisteredSingleComponent<IApplicationGuidFinder>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void IApplicationGuidFinder_ShouldBeRegisteredWithProperImplementation()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().HaveRegisteredProperImplementation<IApplicationGuidFinder, ApplicationGuidFinder>();
        }

        [Test]
        public void IApplicationGuidFinder_ShouldBeResolvedAndNotThrow()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().ResolveWithoutThrowing<IApplicationGuidFinder>();
        }

        [Test]
        public void IDataProviderFactoryFactory_ShouldBeRegisteredWithProperLifestyle()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().HaveRegisteredSingleComponent<IDataProviderFactoryFactory>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void IDataProviderFactoryFactory_ShouldBeRegisteredWithProperImplementation()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().HaveRegisteredProperImplementation<IDataProviderFactoryFactory, DataProviderFactoryFactory>();
        }

        [Test]
        public void IDataProviderFactoryFactory_ShouldBeResolvedAndNotThrow()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().ResolveWithoutThrowing<IDataProviderFactoryFactory>();
        }

        [Test]
        public void IRipProviderInstaller_ShouldBeRegisteredWithProperLifestyle()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().HaveRegisteredSingleComponent<IRipProviderInstaller>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void IRipProviderInstaller_ShouldBeRegisteredWithProperImplementation()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().HaveRegisteredProperImplementation<IRipProviderInstaller, RipProviderInstaller>();
        }

        [Test]
        public void IRipProviderInstaller_ShouldBeResolvedAndNotThrow()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().ResolveWithoutThrowing<IRipProviderInstaller>();
        }

        [Test]
        public void IRipProviderUninstaller_ShouldBeRegisteredWithProperLifestyle()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().HaveRegisteredSingleComponent<IRipProviderUninstaller>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void IRipProviderUninstaller_ShouldBeRegisteredWithProperImplementation()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().HaveRegisteredProperImplementation<IRipProviderUninstaller, RipProviderUninstaller>();
        }

        [Test]
        public void IRipProviderUninstaller_ShouldBeResolvedAndNotThrow()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().ResolveWithoutThrowing<IRipProviderUninstaller>();
        }

        private void RegisterInstallerDependencies(IWindsorContainer container)
        {
            IRegistration[] dependencies =
            {
                CreateDummyObjectRegistration<IIntegrationPointRepository>(),
                CreateDummyObjectRegistration<IDeleteHistoryService>(),
                CreateDummyObjectRegistration<IRelativityObjectManager>(),
                CreateDummyObjectRegistration<IWorkspaceDBContext>(),
                CreateDummyObjectRegistration<IAPILog>(),
                CreateDummyObjectRegistration<IHelper>(),
                CreateDummyObjectRegistration<ISourceProviderRepository>(),
                CreateDummyObjectRegistration<IToggleProvider>(),
                CreateDummyObjectRegistration<IKubernetesMode>()
            };

            container.Register(dependencies);
        }
    }
}
