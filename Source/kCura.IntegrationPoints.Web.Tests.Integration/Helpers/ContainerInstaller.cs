using System.Net;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.WinEDDS.Api;
using NSubstitute;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Integration.Helpers
{
	internal class ContainerInstaller
	{
		public static WindsorContainer CreateContainer()
		{
			var container = new WindsorContainer();

			container.Register(Component.For<IContextContainerFactory>()
				.Instance(Substitute.For<IContextContainerFactory>())
				.LifestyleTransient());
			container.Register(Component.For<IManagerFactory>()
				.Instance(Substitute.For<IManagerFactory>())
				.LifestyleTransient());
			container.Register(Component.For<IServiceManagerProvider>()
				.Instance(Substitute.For<IServiceManagerProvider>())
				.LifestyleTransient());
			container.Register(Component.For<ICPHelper, IHelper>()
				.Instance(new TestHelper())
				.LifestyleTransient());
			container.Register(Component.For<IAuthProvider>()
				.ImplementedBy<AuthProvider>()
				.LifestyleSingleton());
			container.Register(Component.For<IAuthTokenGenerator>()
				.ImplementedBy<ClaimsTokenGenerator>()
				.LifestyleTransient());
			container.Register(Component.For<IAPILog>()
				.UsingFactoryMethod(k => k.Resolve<IHelper>().GetLoggerFactory().GetLogger())
				.LifestyleTransient());
			container.Register(Component.For<ICredentialProvider>()
				.ImplementedBy<UserPasswordCredentialProvider>()
				.LifestyleTransient());
			container.Register(Component.For<ICaseManagerFactory>()
				.ImplementedBy<CaseManagerFactory>()
				.LifestyleTransient());
			container.Register(Component.For<ImportProviderImageController>()
				.LifestyleTransient());

			return container;
		}
	}
}
