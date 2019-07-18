﻿using System;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Installer;
using kCura.WinEDDS.Service.Export;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.FileField;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.ViewField;
using Relativity.Services.View;
using static kCura.IntegrationPoint.Tests.Core.TestHelpers.WindsorContainerTestHelpers;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Installer
{
	[TestFixture]
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
			sut.Should().HaveRegisteredProperImplementation<IFileRepository, FileRepository>();
		}
		private static void RegisterDependencies(IWindsorContainer container)
		{
			var servicesMgrMock = new Mock<IServicesMgr>();
			
			IRegistration[] dependencies =
			{
				Component.For<IServicesMgr>().Instance(servicesMgrMock.Object),
				CreateDummyObjectRegistration<IExternalServiceInstrumentationProvider>(),
				CreateDummyObjectRegistration<IRetryHandlerFactory>(),
				CreateDummyObjectRegistration<IRetryHandler>(),
			};

			container.Register(dependencies);
		}
	}
}
