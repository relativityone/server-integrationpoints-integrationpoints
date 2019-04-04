using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.FileField;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.ViewField;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Installer
{
	[TestFixture]
	public class ExportRepositoryRegistrationTests
	{
		[Test]
		public void ViewFieldRepository_ShouldBeRegisteredWithProperLifestyle()
		{
			// arrange
			IWindsorContainer sut = new WindsorContainer();
			sut.AddExportRepositories();

			// assert
			sut.Should()
				.HaveRegisteredSingleComponent<IViewFieldRepository>()
				.Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
		}

		[Test]
		public void ViewFieldRepository_ShouldBeRegisteredWithProperImplementation()
		{
			// arrange
			IWindsorContainer sut = new WindsorContainer();
			sut.AddExportRepositories();

			// assert
			sut.Should()
				.HaveRegisteredProperImplementation<IViewFieldRepository, ViewFieldRepository>();
		}

		[Test]
		public void ViewFieldRepository_ShouldBeResolvedWithoutThrowing()
		{
			// arrange
			IWindsorContainer sut = new WindsorContainer();
			sut.AddExportRepositories();
			RegisterDependencies(sut);

			// assert
			sut.Should()
				.ResolveWithoutThrowing<IViewFieldRepository>();
		}

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
		public void FileRepository_ShouldBeRegisteredWithProperImplementation()
		{
			//arrange
			IWindsorContainer sut = new WindsorContainer();
			sut.AddExportRepositories();

			//assert
			sut.Should().HaveRegisteredProperImplementation<IFileRepository, FileRepository>();
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
		public void FileFieldRepository_ShouldBeRegisteredWithProperLifestyle()
		{
			//arrange
			IWindsorContainer sut = new WindsorContainer();
			sut.AddExportRepositories();

			//assert
			sut.Should()
				.HaveRegisteredSingleComponent<IFileFieldRepository>()
				.Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
		}

		[Test]
		public void FileFieldRepository_ShouldBeRegisteredWithProperImplementation()
		{
			//arrange
			IWindsorContainer sut = new WindsorContainer();
			sut.AddExportRepositories();

			//assert
			sut.Should().HaveRegisteredProperImplementation<IFileFieldRepository, FileFieldRepository>();
		}

		[Test]
		public void FileFieldRepository_ShouldBeResolvedWithoutThrowing()
		{
			//arrange
			IWindsorContainer sut = new WindsorContainer();
			sut.AddExportRepositories();
			RegisterDependencies(sut);

			//assert
			sut.Should().ResolveWithoutThrowing<IFileFieldRepository>();
		}


		private static void RegisterDependencies(IWindsorContainer container)
		{
			var viewFieldManagerMock = new Mock<IViewFieldManager>();
			var fileManagerMock = new Mock<IFileManager>();
			var fileFieldManagerMock = new Mock<IFileFieldManager>();
			var servicesMgrMock = new Mock<IServicesMgr>();
			servicesMgrMock.Setup(x => x.CreateProxy<IViewFieldManager>(ExecutionIdentity.CurrentUser))
				.Returns(viewFieldManagerMock.Object);
			servicesMgrMock.Setup(x => x.CreateProxy<IFileManager>(ExecutionIdentity.CurrentUser))
				.Returns(fileManagerMock.Object);
			servicesMgrMock.Setup(x => x.CreateProxy<IFileFieldManager>(ExecutionIdentity.CurrentUser))
				.Returns(fileFieldManagerMock.Object);

			IRegistration[] dependencies =
			{
				Component.For<IServicesMgr>().Instance(servicesMgrMock.Object),
				Component.For<IViewFieldManager>().Instance(viewFieldManagerMock.Object),
				Component.For<IFileManager>().Instance(fileManagerMock.Object),
				Component.For<IFileFieldManager>().Instance(fileFieldManagerMock.Object),
				Component.For<IExternalServiceInstrumentationProvider>()
					.Instance(new Mock<IExternalServiceInstrumentationProvider>().Object)
			};

			container.Register(dependencies);
		}
	}
}
