using System;
using System.Web;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using kCura.IntegrationPoints.Web.Installers.Context;
using NUnit.Framework;
using Relativity.API;
using static kCura.IntegrationPoint.Tests.Core.TestHelpers.WindsorContainerTestHelpers;

namespace kCura.IntegrationPoints.Web.Tests.Installers.Context
{
    [TestFixture, Category("Unit")]
    public class WorkspaceContextRegistrationTests
    {
        [Test]
        public void RequestContextWorkspaceContextService_ShouldBeRegisteredWithProperLifestyle()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();
            sut.AddWorkspaceContext();

            // assert
            sut.Should()
                .HaveRegisteredMultipleComponents<IWorkspaceContext>()
                .And.OneOfThemWithImplementation<RequestContextWorkspaceContextService>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.PerWebRequest);
        }

        [Test]
        public void SessionWorkspaceContextService_ShouldBeRegisteredWithProperLifestyle()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();
            sut.AddWorkspaceContext();

            // assert
            sut.Should()
                .HaveRegisteredMultipleComponents<IWorkspaceContext>()
                .And.OneOfThemWithImplementation<SessionWorkspaceContextService>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.PerWebRequest);
        }

        [Test]
        public void NotFoundWorkspaceContextService_ShouldBeRegisteredWithProperLifestyle()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();
            sut.AddWorkspaceContext();

            // assert
            sut.Should()
                .HaveRegisteredMultipleComponents<IWorkspaceContext>()
                .And.OneOfThemWithImplementation<NotFoundWorkspaceContextService>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.PerWebRequest);
        }

        [Test]
        public void WorkspaceContext_ShouldBeRegisteredInCorrectOrder()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();
            sut.AddWorkspaceContext();

            // assert
            Type[] implementationsOrder =
            {
                typeof(RequestContextWorkspaceContextService),
                typeof(SessionWorkspaceContextService),
                typeof(NotFoundWorkspaceContextService)
            };

            sut.Should()
                .HaveRegisteredMultipleComponents<IWorkspaceContext>()
                .And.AllRegisteredInFollowingOrder(implementationsOrder);
        }

        [Test]
        public void IWorkspaceContext_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();
            sut.ConfigureChangingLifestyleFromPerWebRequestToTransientBecausePerWebRequestIsNotResolvableInTests();
            sut.AddWorkspaceContext();
            RegisterDependencies(sut);

            // assert
            sut.Should()
                .ResolveWithoutThrowing<IWorkspaceContext>();
        }

        [Test]
        public void IWorkspaceContext_ShouldBeResolvedWithProperChainOfResponsibilityOrder()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();
            sut.ConfigureChangingLifestyleFromPerWebRequestToTransientBecausePerWebRequestIsNotResolvableInTests();
            sut.AddWorkspaceContext();
            RegisterDependencies(sut);

            var dependenciesRecorder = new WindsorDependenciesGraphRecorder(sut.Kernel);

            // act
            sut.Resolve<IWorkspaceContext>();

            // assert
            dependenciesRecorder
                .WasDependencyPresent<RequestContextWorkspaceContextService, SessionWorkspaceContextService>()
                .Should()
                .BeTrue("because {0} depends on {1}", nameof(RequestContextWorkspaceContextService), nameof(SessionWorkspaceContextService));
            dependenciesRecorder
                .WasDependencyPresent<SessionWorkspaceContextService, NotFoundWorkspaceContextService>()
                .Should()
                .BeTrue("because {0} depends on {1}", nameof(SessionWorkspaceContextService), nameof(NotFoundWorkspaceContextService));
        }

        private static void RegisterDependencies(IWindsorContainer container)
        {
            IRegistration[] dependencies =
            {
                CreateDummyObjectRegistration<HttpRequestBase>(),
                CreateDummyObjectRegistration<ISessionService>(),
                CreateDummyObjectRegistration<IAPILog>()
            };

            container.Register(dependencies);
        }
    }
}
