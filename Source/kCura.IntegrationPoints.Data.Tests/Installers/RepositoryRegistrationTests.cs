using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Installers
{
	[TestFixture]
	public class RepositoryRegistrationTests
	{
		[Test]
		public void IntegrationPointRepository_ShouldBeRegisteredWithProperLifestyle()
		{
			// arrange
			IWindsorContainer sut = new WindsorContainer();
			sut.AddRepositories();

			// assert
			sut.Should()
				.HaveRegisteredSingleComponent<IIntegrationPointRepository>()
				.Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
		}

		[Test]
		public void IntegrationPointRepository_ShouldBeRegisteredWithProperImplementation()
		{
			// arrange
			IWindsorContainer sut = new WindsorContainer();
			sut.AddRepositories();

			// assert
			sut.Should()
				.HaveRegisteredProperImplementation<IIntegrationPointRepository, IntegrationPointRepository>();
		}

		[Test]
		public void IntegrationPointRepository_ShouldBeResolvedWithoutThrowing()
		{
			// arrange
			IWindsorContainer sut = new WindsorContainer();
			sut.AddRepositories();
			RegisterDependencies(sut);

			// assert
			sut.Should()
				.ResolveWithoutThrowing<IIntegrationPointRepository>();
		}

		private static void RegisterDependencies(IWindsorContainer container)
		{
			IRegistration[] dependencies =
			{
				Component.For<IRelativityObjectManager>().Instance(new Mock<IRelativityObjectManager>().Object)
			};

			container.Register(dependencies);
		}
	}
}
