using System;
using System.Linq;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.DocumentTransferProvider.Installers;
using Moq;
using NUnit.Framework;
using Relativity.API;

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
			//arrange & act
			IHandler handler = _sut
				.GetHandlersFor<IExtendedImportApiFactory>()
				.Single();

			//assert
			handler.ComponentModel.LifestyleType
				.Should().Be(LifestyleType.Singleton);
		}

		[Test]
		public void IExtendedImportApiFactory_ShouldBeRegisteredWithProperImplementation()
		{
			//arrange & act
			Type implementationType = _sut
				.GetImplementationTypesFor<IExtendedImportApiFactory>()
				.Single();

			//assert
			implementationType.Should().Be(typeof(ExtendedImportApiFactory));
		}

		[Test]
		public void IExtendedImportApiFactory_ShouldBeResolvedAndNotThrow()
		{
			//arrange
			RegisterInstallerDependencies(_sut);
			IExtendedImportApiFactory resolved = null;

			//act
			Action action = () => resolved = _sut.Resolve<IExtendedImportApiFactory>();

			//assert
			action.ShouldNotThrow();
			resolved.Should().NotBeNull();
		}

		[Test]
		public void IExtendedImportApiFacade_ShouldBeRegisteredWithProperLifestyle()
		{
			//arrange & act
			IHandler handler = _sut
				.GetHandlersFor<IExtendedImportApiFacade>()
				.Single();

			//assert
			handler.ComponentModel.LifestyleType
				.Should().Be(LifestyleType.Transient);
		}

		[Test]
		public void IExtendedImportApiFacade_ShouldBeRegisteredWithProperImplementation()
		{
			//arrange & act
			Type implementationType = _sut
				.GetImplementationTypesFor<IExtendedImportApiFacade>()
				.Single();

			//assert
			implementationType.Should().Be(typeof(ExtendedImportApiFacade));
		}

		[Test]
		public void IExtendedImportApiFacade_ShouldBeResolvedAndNotThrow()
		{
			//arrange
			RegisterInstallerDependencies(_sut);
			IExtendedImportApiFacade resolved = null;

			//act
			Action action = () => resolved = _sut.Resolve<IExtendedImportApiFacade>();

			//assert
			action.ShouldNotThrow();
			resolved.Should().NotBeNull();
		}

		[Test]
		public void IDataSourceProvider_ShouldBeRegisteredWithProperLifestyle()
		{
			//arrange & act
			IHandler handler = _sut
				.GetHandlersFor<IDataSourceProvider>()
				.Single();

			//assert
			handler.ComponentModel.LifestyleType
				.Should().Be(LifestyleType.Transient);
		}

		[Test]
		public void IDataSourceProvider_ShouldBeRegisteredWithProperName()
		{
			//arrange
			string expectedComponentName = new Guid(Domain.Constants.RELATIVITY_PROVIDER_GUID)
				.ToString();

			//act
			IHandler handler = _sut
				.GetHandlersFor<IDataSourceProvider>()
				.Single();

			//assert
			handler.ComponentModel.Name
				.Should().Be(expectedComponentName);
		}

		[Test]
		public void IDataSourceProvider_ShouldBeRegisteredWithProperImplementation()
		{
			//arrange & act
			Type implementationType = _sut
				.GetImplementationTypesFor<IDataSourceProvider>()
				.Single();

			//assert
			implementationType.Should().Be(typeof(DocumentTransferProvider));
		}

		[Test]
		public void IDataSourceProvider_ShouldBeResolvedAndNotThrow()
		{
			//arrange
			RegisterInstallerDependencies(_sut);
			IDataSourceProvider resolved = null;

			//act
			Action action = () => resolved = _sut.Resolve<IDataSourceProvider>();

			//assert
			action.ShouldNotThrow();
			resolved.Should().NotBeNull();
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
