using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Services.Installers.Context;

namespace Relativity.IntegrationPoints.Services.Tests.Installers.Context
{
    [TestFixture]
    [Category("Unit")]
    public class WorkspaceContextRegistrationTests
    {
        private const int _WORKSPACE_ID = 64531421;

        private IWindsorContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = new WindsorContainer();
            _container.AddWorkspaceContext(_WORKSPACE_ID);
        }

        [Test]
        public void ObjectManagerService_ShouldBeResolvedAndNotThrow()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().ResolveWithoutThrowing<IRelativityObjectManagerService>();
        }

        [Test]
        public void ObjectManagerService_ShouldResolveProperImplementation()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().ResolveImplementationWithoutThrowing<IRelativityObjectManagerService, RelativityObjectManagerService>();
        }

        [Test]
        public void IServiceContextHelper_ShouldBeResolvedAndNotThrow()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().ResolveWithoutThrowing<IServiceContextHelper>();
        }

        [Test]
        public void IServiceContextHelper_ShouldResolveProperImplementation()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().ResolveImplementationWithoutThrowing<IServiceContextHelper, ServiceContextHelperForKeplerService>();
        }

        [Test]
        public void IWorkspaceDBContext_ShouldBeResolvedAndNotThrow()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().ResolveWithoutThrowing<IWorkspaceDBContext>();
        }

        [Test]
        public void IWorkspaceDBContext_ShouldResolveProperImplementation()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().ResolveImplementationWithoutThrowing<IWorkspaceDBContext, WorkspaceDBContext>();
        }

        [Test]
        public void IWorkspaceDBContext_ShouldBeRegisteredWithProperLifestyle()
        {
            // Arrange
            RegisterInstallerDependencies(_container);

            // Assert
            _container.Should().HaveRegisteredSingleComponent<IWorkspaceDBContext>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        private void RegisterInstallerDependencies(IWindsorContainer container)
        {
            var helperMock = new Mock<IServiceHelper>();

            IRegistration[] dependencies =
            {
                Component
                    .For<IHelper, IServiceHelper>()
                    .UsingFactoryMethod(k => helperMock.Object)
            };

            container.Register(dependencies);
        }
    }
}
