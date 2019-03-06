using System.Web;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using kCura.IntegrationPoints.Web.Installers.Context;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Installers.Context
{
	[TestFixture]
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
		public void LastWorkspaceContextService_ShouldBeRegisteredWithProperLifestyle()
		{
			// arrange
			IWindsorContainer sut = new WindsorContainer();
			sut.AddWorkspaceContext();

			// assert
			sut.Should()
				.HaveRegisteredMultipleComponents<IWorkspaceContext>()
				.And.OneOfThemWithImplementation<LastWorkspaceContextService>()
				.Which.Should()
				.BeRegisteredWithLifestyle(LifestyleType.PerWebRequest);
		}

		[Test]
		public void IWorkspaceContext_ShouldBeResolvedWithoutThrowing()
		{
			// arrange
			IWindsorContainer sut = new WindsorContainer();
			ChangeLifestyleFromPerWebRequestToTransient(sut); // we cannot resolve PerWebRequest object in tests
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
			ChangeLifestyleFromPerWebRequestToTransient(sut); // we cannot resolve PerWebRequest object in tests
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
				.WasDependencyPresent<SessionWorkspaceContextService, LastWorkspaceContextService>()
				.Should()
				.BeTrue("because {0} depends on {1}", nameof(SessionWorkspaceContextService), nameof(LastWorkspaceContextService));
		}

		private static void RegisterDependencies(IWindsorContainer container)
		{
			IRegistration[] dependencies =
			{
				Component.For<HttpRequestBase>().Instance(new Mock<HttpRequestBase>().Object),
				Component.For<ISessionService>().Instance(new Mock<ISessionService>().Object),
				Component.For<IAPILog>().Instance(new Mock<IAPILog>().Object)
			};

			container.Register(dependencies);
		}

		private static void ChangeLifestyleFromPerWebRequestToTransient(IWindsorContainer container)
		{
			container.Kernel.ComponentModelCreated += model =>
			{
				if (model.LifestyleType == LifestyleType.PerWebRequest)
				{
					model.LifestyleType = LifestyleType.Transient;
				}
			};
		}
	}
}
