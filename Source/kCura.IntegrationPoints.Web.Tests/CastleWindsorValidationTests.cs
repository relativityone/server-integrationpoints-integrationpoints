using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Web.Installers;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests
{
	[TestFixture]
	public class CastleWindsorValidationTests : CastleWindsorValidatorBase
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
	}
}