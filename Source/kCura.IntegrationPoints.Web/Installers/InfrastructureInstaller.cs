﻿using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Infrastructure.Session;
using Relativity.API;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;

namespace kCura.IntegrationPoints.Web.Installers
{
	public class InfrastructureInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			RegisterDelegatingHandlers(container);
			RegisterExceptionLoggers(container);

			RegisterSessionService(container);
			RegisterErrorService(container);
		}

		private static void RegisterExceptionLoggers(IWindsorContainer container)
		{
			container.Register(Classes
				.FromThisAssembly()
				.BasedOn<ExceptionLogger>()
				.LifestyleSingleton()
			);
		}

		private static void RegisterDelegatingHandlers(IWindsorContainer container)
		{
			container.Register(Classes
				.FromThisAssembly()
				.BasedOn<DelegatingHandler>()
				.LifestyleSingleton()
			);
		}

		private static void RegisterErrorService(IWindsorContainer container)
		{
			container.Register(Component
				.For<IErrorService>()
				.ImplementedBy<CustomPageErrorService>()
				.LifestyleSingleton()
			);
		}

		private static void RegisterSessionService(IWindsorContainer container)
		{
			container.Register(Component
				.For<ISessionService>()
				.UsingFactoryMethod(k => SessionServiceFactory.GetSessionService(k.Resolve<ICPHelper>))
				.LifestylePerWebRequest()
			);
		}
	}
}