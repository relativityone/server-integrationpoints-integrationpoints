using System;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Core.Authentication.WebApi;
using kCura.IntegrationPoints.Core.Authentication.WebApi.LoginHelperFacade;
using kCura.IntegrationPoints.Core.Installers.Registrations;
using kCura.IntegrationPoints.Domain.Authentication;
using NUnit.Framework;
using Relativity.API;
using static kCura.IntegrationPoint.Tests.Core.TestHelpers.WindsorContainerTestHelpers;

namespace kCura.IntegrationPoints.Core.Tests.Installers.Registrations
{
    [TestFixture, Category("Unit")]
    public class WebApiLoginServiceRegistrationTests
    {
        private IWindsorContainer _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new WindsorContainer();
            _sut.AddWebApiLoginService();
        }

        [Test]
        public void WebApiLoginService_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            RegisterDependencies(_sut);

            // assert
            _sut.Should()
                .ResolveWithoutThrowing<IWebApiLoginService>();
        }

        [Test]
        public void WebApiLoginService_ShouldBeRegisteredWithProperLifestyle()
        {
            // assert
            _sut.Should()
                .HaveRegisteredSingleComponent<IWebApiLoginService>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void WebApiLoginService_ShouldBeRegisteredWithProperImplementation()
        {
            // assert
            _sut.Should()
                .HaveRegisteredProperImplementation<IWebApiLoginService, WebApiLoginService>();
        }

        [Test]
        public void LoginHelperFacade_ShouldBeRegisteredInCorrectOrder()
        {
            // assert
            Type[] implementationsOrder =
            {
                typeof(LoginHelperRetryDecorator),
                typeof(LoginHelperInstrumentationDecorator),
                typeof(LoginHelperFacade),
            };

            _sut.Should()
                .HaveRegisteredMultipleComponents<ILoginHelperFacade>()
                .And.AllRegisteredInFollowingOrder(implementationsOrder);
        }

        [Test]
        public void LoginHelperFacade_ShouldBeRegisteredWithProperLifestyle()
        {
            // assert
            _sut.Should()
                .HaveRegisteredMultipleComponents<ILoginHelperFacade>()
                .And.OneOfThemWithImplementation<LoginHelperFacade>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Singleton);
        }

        [Test]
        public void LoginHelperInstrumentationDecorator_ShouldBeRegisteredWithProperLifestyle()
        {
            // assert
            _sut.Should()
                .HaveRegisteredMultipleComponents<ILoginHelperFacade>()
                .And.OneOfThemWithImplementation<LoginHelperInstrumentationDecorator>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void LoginHelperRetryDecorator_ShouldBeRegisteredWithProperLifestyle()
        {
            // assert
            _sut.Should()
                .HaveRegisteredMultipleComponents<ILoginHelperFacade>()
                .And.OneOfThemWithImplementation<LoginHelperRetryDecorator>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        private static void RegisterDependencies(IWindsorContainer container)
        {
            IRegistration[] dependencies =
            {
                CreateDummyObjectRegistration<IAPILog>(),
                CreateDummyObjectRegistration<IRetryHandlerFactory>(),
                CreateDummyObjectRegistration<IExternalServiceInstrumentationProvider>(),
                CreateDummyObjectRegistration<IAuthTokenGenerator>()
            };

            container.Register(dependencies);
        }
    }
}
