using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Services.Installers.Context;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace Relativity.IntegrationPoints.Services.Tests.Installers.Context
{
    [TestFixture, Category("Unit")]
    public class WorkspaceContextRegistrationTests
    {
        private IWindsorContainer _container;
        private const int _WORKSPACE_ID = 64531421;

        [SetUp]
        public void SetUp()
        {
            _container = new WindsorContainer();
            _container.AddWorkspaceContext(_WORKSPACE_ID);
        }

        [Test]
        public void ObjectManagerService_ShouldBeResolvedAndNotThrow()
        {
            // arrange
            RegisterInstallerDependencies(_container);

            // assert
            _container.Should().ResolveWithoutThrowing<IRelativityObjectManagerService>();
        }

        [Test]
        public void ObjectManagerService_ShouldResolveProperImplementation()
        {
            // arrange
            RegisterInstallerDependencies(_container);

            // assert
            _container.Should().ResolveImplementationWithoutThrowing<IRelativityObjectManagerService, RelativityObjectManagerService>();
        }

        [Test]
        public void IServiceContextHelper_ShouldBeResolvedAndNotThrow()
        {
            // arrange
            RegisterInstallerDependencies(_container);

            // assert
            _container.Should().ResolveWithoutThrowing<IServiceContextHelper>();
        }

        [Test]
        public void IServiceContextHelper_ShouldResolveProperImplementation()
        {
            // arrange
            RegisterInstallerDependencies(_container);

            // assert
            _container.Should().ResolveImplementationWithoutThrowing<IServiceContextHelper, ServiceContextHelperForKeplerService>();
        }

        [Test]
        public void IWorkspaceDBContext_ShouldBeResolvedAndNotThrow()
        {
            // arrange
            RegisterInstallerDependencies(_container);

            // assert
            _container.Should().ResolveWithoutThrowing<IWorkspaceDBContext>();
        }

        [Test]
        public void IWorkspaceDBContext_ShouldResolveProperImplementation()
        {
            // arrange
            RegisterInstallerDependencies(_container);

            // assert
            _container.Should().ResolveImplementationWithoutThrowing<IWorkspaceDBContext, WorkspaceDBContext>();
        }

        [Test]
        public void IWorkspaceDBContext_ShouldBeRegisteredWithProperLifestyle()
        {
            // arrange
            RegisterInstallerDependencies(_container);

            // assert
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
                    .UsingFactoryMethod(k=>helperMock.Object)
            };

            container.Register(dependencies);
        }
    }
}
