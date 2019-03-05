using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.DocumentTransferProvider.Installers;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Tests.Installers
{
	[TestFixture]
	public class DocumentTransferProviderInstallerTests
	{
		private IWindsorContainer _sut;

		[SetUp]
		public void SetUp()
		{
			_sut = new WindsorContainer()
				.Install(new DocumentTransferProviderInstaller());
		}

		[Test]
		public void IExtendedImportApiFactory_ShouldBeRegisteredWithProperLifestyle()
		{
			_sut.Should()
				.HaveRegisteredSingleComponent<IExtendedImportApiFactory>()
				.WithLifestyle(LifestyleType.Singleton);
		}

		[Test]
		public void IExtendedImportApiFactory_ShouldBeRegisteredWithProperImplementation()
		{
			_sut.Should().HaveRegisteredProperImplementation<IExtendedImportApiFactory, ExtendedImportApiFactory>();
		}

		[Test]
		public void IExtendedImportApiFactory_ShouldBeResolvedAndNotThrow()
		{
			//arrange
			RegisterInstallerDependencies(_sut);

			//act & assert
			_sut.Should().ResolveWithoutThrowing<IExtendedImportApiFactory>();
		}

		[Test]
		public void IExtendedImportApiFacade_ShouldBeRegisteredWithProperLifestyle()
		{
			_sut.Should()
				.HaveRegisteredSingleComponent<IExtendedImportApiFacade>()
				.WithLifestyle(LifestyleType.Transient);
		}

		[Test]
		public void IExtendedImportApiFacade_ShouldBeRegisteredWithProperImplementation()
		{
			_sut.Should().HaveRegisteredProperImplementation<IExtendedImportApiFacade, ExtendedImportApiFacade>();
		}

		[Test]
		public void IExtendedImportApiFacade_ShouldBeResolvedAndNotThrow()
		{
			//arrange
			RegisterInstallerDependencies(_sut);

			//act & assert
			_sut.Should().ResolveWithoutThrowing<IExtendedImportApiFacade>();
		}

		[Test]
		public void IDataSourceProvider_ShouldBeRegisteredWithProperLifestyle()
		{
			_sut.Should()
				.HaveRegisteredSingleComponent<IDataSourceProvider>()
				.WithLifestyle(LifestyleType.Transient);
		}

		[Test]
		public void IDataSourceProvider_ShouldBeRegisteredWithProperName()
		{
			//arrange
			string expectedComponentName = new Guid(Domain.Constants.RELATIVITY_PROVIDER_GUID)
				.ToString();

			//act & assert
			_sut.Should()
				.HaveRegisteredSingleComponent<IDataSourceProvider>()
				.WithName(expectedComponentName);
		}

		[Test]
		public void IDataSourceProvider_ShouldBeRegisteredWithProperImplementation()
		{
			_sut.Should().HaveRegisteredProperImplementation<IDataSourceProvider, DocumentTransferProvider>();
		}

		[Test]
		public void IDataSourceProvider_ShouldBeResolvedAndNotThrow()
		{
			//arrange
			RegisterInstallerDependencies(_sut);

			//act & assert
			_sut.Should().ResolveWithoutThrowing<IDataSourceProvider>();
		}

		private void RegisterInstallerDependencies(IWindsorContainer container)
		{
			IRegistration[] dependencies =
			{
				Component.For<IWebApiConfig>().Instance(new Mock<IWebApiConfig>().Object),
				Component.For<IAPILog>().Instance(new Mock<IAPILog>().Object),
				Component.For<IRepositoryFactory>().Instance(new Mock<IRepositoryFactory>().Object)
			};

			container.Register(dependencies);
		}
	}
}
