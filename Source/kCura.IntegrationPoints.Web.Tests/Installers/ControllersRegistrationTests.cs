using Castle.Core;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Web.Controllers;
using kCura.IntegrationPoints.Web.Installers;
using Microsoft.AspNet.SignalR.Hubs;
using NUnit.Framework;
using System;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace kCura.IntegrationPoints.Web.Tests.Installers
{
    [TestFixture, Category("Unit")]
    public class ControllersRegistrationTests
    {
        private IWindsorContainer _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new WindsorContainer();
            _sut = _sut.AddControllers();
        }

        [Test]
        public void MvcControllersShouldHaveControllerSufix()
        {
            InterfaceImplementationInWebProjectShouldHaveSufix<IController>("Controller");
        }

        [Test]
        public void MvcControllersShouldLiveInControllersNamespace()
        {
            InterfaceImplementationInWebProjectShouldLiveInNamespace<IController>("Controllers");
        }

        [Test]
        public void MvcControllersAreRegistered()
        {
            InterfaceImplementationInWebProjectShouldBeRegistered<IController>();
        }

        [Test]
        public void MvcControllersAreTransient()
        {
            _sut.Should()
                .HaveRegisteredMultipleComponents<IController>()
                .And.AllWithLifestyle(LifestyleType.Transient, "because all mvc controllers should be transient");
        }

        [Test]
        public void MvcControllersExposeThemselvesAsService()
        {
            _sut.Should()
                .HaveRegisteredMultipleComponents<IController>()
                .And.AllExposeThemselvesAsService("because all mvc controllers should expose themselves as service");
        }

        [Test]
        public void ApiControllersShouldHaveControllerSufix()
        {
            InterfaceImplementationInWebProjectShouldHaveSufix<IHttpController>("Controller");
        }

        [Test]
        public void ApiControllersShouldLiveInControllersNamespace()
        {
            InterfaceImplementationInWebProjectShouldLiveInNamespace<IHttpController>("Controllers");
        }

        [Test]
        public void ApiControllersAreRegistered()
        {
            InterfaceImplementationInWebProjectShouldBeRegistered<IHttpController>();
        }

        [Test]
        public void ApiControllersAreTransient()
        {
            _sut.Should()
                .HaveRegisteredMultipleComponents<IHttpController>()
                .And.AllWithLifestyle(LifestyleType.Transient, "because all api controllers should be transient");
        }

        [Test]
        public void ApiControllersExposeThemselvesAsService()
        {
            _sut.Should()
                .HaveRegisteredMultipleComponents<IHttpController>()
                .And.AllExposeThemselvesAsService("because all api controllers should expose themselves as service");
        }

        [Test]
        public void SignalRHubsShouldHaveHubSufix()
        {
            InterfaceImplementationInWebProjectShouldHaveSufix<IHub>("Hub");
        }

        [Test]
        public void SignalRHubsShouldLiveInSignalRHubsNamespace()
        {
            InterfaceImplementationInWebProjectShouldLiveInNamespace<IHub>("SignalRHubs");
        }

        [Test]
        public void SignalRHubsAreRegistered()
        {
            InterfaceImplementationInWebProjectShouldBeRegistered<IHub>();
        }

        [Test]
        public void SignalRHubsAreTransient()
        {
            _sut.Should()
                .HaveRegisteredMultipleComponents<IHub>()
                .And.AllWithLifestyle(LifestyleType.Transient, "because all SignalR hubs should be transient");
        }

        [Test]
        public void SignalRHubsExposeThemselvesAsService()
        {
            _sut.Should()
                .HaveRegisteredMultipleComponents<IHub>()
                .And.AllExposeThemselvesAsService("because all SignalR hubs should expose themselves as service");
        }

        private void InterfaceImplementationInWebProjectShouldHaveSufix<T>(string sufix)
        {
            // arrange
            Type[] implementationsWithSufix = GetPublicClassesFromWebProjectAssembly(c => c.Name.EndsWith(sufix));
            // ReSharper disable once CoVariantArrayConversion
            object[] implementationsOfInterface = _sut.GetImplementationTypesFor<T>();

            // assert
            implementationsOfInterface.Should().BeSubsetOf(implementationsWithSufix,
                "because all registered {0} implementations should have {1} sufix",
                nameof(T),
                sufix);
        }

        private void InterfaceImplementationInWebProjectShouldLiveInNamespace<T>(string namespaceName)
        {
            // arrange
            Type[] implementationsInNamespace = GetPublicClassesFromWebProjectAssembly(c => c.Namespace.Contains(namespaceName));
            // ReSharper disable once CoVariantArrayConversion
            object[] implementationsOfInterface = _sut.GetImplementationTypesFor<T>();

            // assert
            implementationsOfInterface.Should().BeSubsetOf(implementationsInNamespace,
                "because all registered {0} implementations should live in {1} namespace",
                nameof(T),
                namespaceName);
        }

        public void InterfaceImplementationInWebProjectShouldBeRegistered<T>()
        {
            Type[] implementationsInAssembly = GetPublicClassesFromWebProjectAssembly(
                type => typeof(T).IsAssignableFrom(type));
            Type[] registeredImplementations = _sut.GetImplementationTypesFor<T>();

            registeredImplementations.Should()
                .BeEquivalentTo(implementationsInAssembly,
                    "because all {0} implementations should be registered",
                    nameof(T)
                    );
        }

        private Type[] GetPublicClassesFromWebProjectAssembly(Func<Type, bool> predicate)
        {
            return typeof(IntegrationPointsController).Assembly.GetExportedTypes()
                .Where(t => t.IsClass)
                .Where(t => t.IsAbstract == false)
                .Where(predicate)
                .OrderBy(t => t.Name)
                .ToArray();
        }
    }
}
