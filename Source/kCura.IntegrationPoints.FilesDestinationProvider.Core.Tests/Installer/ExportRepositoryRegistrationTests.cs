using System;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.Toggles;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer;
using kCura.WinEDDS.Service.Export;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Toggles;
using static kCura.IntegrationPoint.Tests.Core.TestHelpers.WindsorContainerTestHelpers;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Installer
{
	[TestFixture, Category("Unit")]
	public class ExportRepositoryRegistrationTests
	{

		[Test]
		public void FileRepository_ShouldBeRegisteredWithProperLifestyle()
		{
			//arrange
			IWindsorContainer sut = new WindsorContainer();
			sut.AddExportRepositories();

			//assert
			sut.Should()
				.HaveRegisteredSingleComponent<IFileRepository>()
				.Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
		}

		[Test]
		public void FileRepository_ShouldBeResolvedWithoutThrowing()
		{
			//arrange
			IWindsorContainer sut = new WindsorContainer();
			sut.AddExportRepositories();
			RegisterDependencies(sut);

			//assert
			sut.Should().ResolveWithoutThrowing<IFileRepository>();
		}

		[Test]
		public void FileRepository_ShouldBeRegisteredWithProperImplementation()
		{
			//arrange
			IWindsorContainer sut = new WindsorContainer();
			sut.AddExportRepositories();

			//assert
			sut.Should().HaveRegisteredProperImplementation<IFileRepository, FileRepositoryProxy>();
		}

		private static void RegisterDependencies(IWindsorContainer container)
		{
			var servicesMgrMock = new Mock<IServicesMgr>();

			var toggleProvider = new Mock<IToggleProvider>();
			toggleProvider.Setup(x => x.IsEnabled<EnableKeplerizedImportAPIToggle>())
				.Returns(true);
			
			IRegistration[] dependencies =
			{
				Component.For<IToggleProvider>().Instance(toggleProvider.Object),
				Component.For<IServicesMgr>().Instance(servicesMgrMock.Object),
				CreateDummyObjectRegistration<IExternalServiceInstrumentationProvider>(),
				CreateDummyObjectRegistration<IRetryHandlerFactory>(),
				CreateDummyObjectRegistration<IRetryHandler>(),
			};

			container.Register(dependencies);
		}
	}
}
