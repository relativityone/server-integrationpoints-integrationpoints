using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.MicroKernel;
using Castle.MicroKernel.Handlers;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Diagnostics;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Web.Installers;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Unit
{
	[TestFixture]
	public class CastleWindsorValidationTests
	{
		private IWindsorContainer _container;
		private IHelper _helper;
		private IConfig _config;

		[SetUp]
		public void SetUp()
		{
			// Set up mocks
			_helper = NSubstitute.Substitute.For<IHelper>();
			_config = NSubstitute.Substitute.For<IConfig>();

			// Set up container
			_container = new WindsorContainer();
			var kernel = _container.Kernel;
			kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, true));

			// Register mocks
			_container.Register(Component.For<IConfig>().Instance(_config).LifestyleTransient());
			_container.Register(Component.For<IHelper>().Instance(_helper).LifestyleTransient());
		}

		[Test]
		public void WebInstallersInstallSuccesfully()
		{
			// Arrange
			_container.Install(new ControllerInstaller());

			// Act / Assert
			CheckForPotentiallyMisconfiguredComponents(_container);	
		}

		static public string AssemblyDirectory
		{
			get
			{
				var codeBase = Assembly.GetExecutingAssembly().CodeBase;

				var uri = new UriBuilder(codeBase);

				var path = Uri.UnescapeDataString(uri.Path);

				return Path.GetDirectoryName(path);
			}
		}

		private static void CheckForPotentiallyMisconfiguredComponents(IWindsorContainer container)
		{
			IDiagnosticsHost host = (IDiagnosticsHost)container.Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey);
			IPotentiallyMisconfiguredComponentsDiagnostic diagnostics = host.GetDiagnostic<IPotentiallyMisconfiguredComponentsDiagnostic>();

			IHandler[] misconfiguredHandlers = diagnostics.Inspect();

			if (misconfiguredHandlers.Any())
			{
				var message = new StringBuilder();
				var inspector = new DependencyInspector(message);

				foreach (IHandler handler in misconfiguredHandlers)
				{
					IExposeDependencyInfo exposeDependency = handler as IExposeDependencyInfo;
					exposeDependency?.ObtainDependencyDetails(inspector);
				}

				if (!String.IsNullOrEmpty(message.ToString()))
				{
					throw new Exception(message.ToString());
				}
			}
		}
	}
}