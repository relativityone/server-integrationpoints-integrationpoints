using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Web.Context.UserContext;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using kCura.IntegrationPoints.Web.Installers.Context;
using NUnit.Framework;
using System.Web;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.API;
using static kCura.IntegrationPoint.Tests.Core.TestHelpers.WindsorContainerTestHelpers;

namespace kCura.IntegrationPoints.Web.Tests.Installers.Context
{
    [TestFixture, Category("Unit")]
    public class UserContextRegistrationTests
    {
        [Test]
        public void RequestHeadersUserContextService_ShouldBeRegisteredWithProperLifestyle()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();
            sut.AddUserContext();

            // assert
            sut.Should()
                .HaveRegisteredMultipleComponents<IUserContext>()
                .And.OneOfThemWithImplementation<RequestHeadersUserContextService>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.PerWebRequest);
        }

        [Test]
        public void SessionUserContextService_ShouldBeRegisteredWithProperLifestyle()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();
            sut.AddUserContext();

            // assert
            sut.Should()
                .HaveRegisteredMultipleComponents<IUserContext>()
                .And.OneOfThemWithImplementation<SessionUserContextService>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.PerWebRequest);
        }

        [Test]
        public void NotFoundUserContextService_ShouldBeRegisteredWithProperLifestyle()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();
            sut.AddUserContext();

            // assert
            sut.Should()
                .HaveRegisteredMultipleComponents<IUserContext>()
                .And.OneOfThemWithImplementation<NotFoundUserContextService>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Singleton);
        }

        [Test]
        public void IUserContext_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();
            sut.ConfigureChangingLifestyleFromPerWebRequestToTransientBecausePerWebRequestIsNotResolvableInTests();
            sut.AddUserContext();
            RegisterDependencies(sut);

            // assert
            sut.Should()
                .ResolveWithoutThrowing<IUserContext>();
        }

        [Test]
        public void IUserContext_ShouldBeResolvedWithProperChainOfResponsibilityOrder()
        {
            // arrange
            IWindsorContainer sut = new WindsorContainer();
            sut.ConfigureChangingLifestyleFromPerWebRequestToTransientBecausePerWebRequestIsNotResolvableInTests();
            sut.AddUserContext();
            RegisterDependencies(sut);

            var dependenciesRecorder = new WindsorDependenciesGraphRecorder(sut.Kernel);

            // act
            sut.Resolve<IUserContext>();

            // assert
            dependenciesRecorder
                .WasDependencyPresent<RequestHeadersUserContextService, SessionUserContextService>()
                .Should()
                .BeTrue("because {0} depends on {1}", nameof(RequestHeadersUserContextService), nameof(SessionUserContextService));
            dependenciesRecorder
                .WasDependencyPresent<SessionUserContextService, NotFoundUserContextService>()
                .Should()
                .BeTrue("because {0} depends on {1}", nameof(SessionUserContextService), nameof(NotFoundUserContextService));
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
