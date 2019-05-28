using System;
using FluentAssertions;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers.Factories;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.SharedLibrary
{
	public class CoreServiceFactoryTests
	{
		private CoreServiceFactory _sut;
		private ExportFile _exportFile;
		private Mock<IServiceFactory> _webApiServiceFactoryMock;
		private Mock<Func<IAuditManager>> _auditManagerFactoryMock;
		private Mock<Func<IFieldManager>> _fieldManagerFactoryMock;
		private Mock<Func<ISearchManager>> _searchManagerFactoryMock;
		private Mock<IExportFileDownloaderFactory> _exportFileDownloaderFactoryMock;

		private const int _EXPORT_ARTIFACT_TYPE_ID = 123;

		[SetUp]
		public void SetUp()
		{
			_exportFile = new ExportFile(_EXPORT_ARTIFACT_TYPE_ID);

			_webApiServiceFactoryMock = new Mock<IServiceFactory>();
			_auditManagerFactoryMock = new Mock<Func<IAuditManager>>();
			_fieldManagerFactoryMock = new Mock<Func<IFieldManager>>();
			_searchManagerFactoryMock = new Mock<Func<ISearchManager>>();
			_exportFileDownloaderFactoryMock = new Mock<IExportFileDownloaderFactory>();

			_sut = CreateCoreServiceFactory();
		}

		[Test]
		public void CreateAuditManager_ShouldReturnProperObject()
		{
			// arrange
			var auditManagerMock = new Mock<IAuditManager>();
			_auditManagerFactoryMock.Setup(x => x()).Returns(auditManagerMock.Object);

			// act
			IAuditManager actualAuditManager = _sut.CreateAuditManager();

			// assert
			actualAuditManager.Should().Be(auditManagerMock.Object);
		}

		[Test]
		public void CreateAuditManager_ShouldCallFactoryForEachCall()
		{
			// arrange
			int numberOfCalls = 2;

			// act
			for (int i = 0; i < numberOfCalls; i++)
			{
				_sut.CreateAuditManager();
			}

			// assert
			_auditManagerFactoryMock.Verify(x => x(), Times.Exactly(numberOfCalls));
		}

		[Test]
		public void CreateFieldManager_ShouldReturnProperObject()
		{
			// arrange
			var fieldManagerMock = new Mock<IFieldManager>();
			_fieldManagerFactoryMock.Setup(x => x()).Returns(fieldManagerMock.Object);

			// act
			IFieldManager actualFieldManager = _sut.CreateFieldManager();

			// assert
			actualFieldManager.Should().Be(fieldManagerMock.Object);
		}

		[Test]
		public void CreateFieldManager_ShouldCallFactoryForEachCall()
		{
			// arrange
			int numberOfCalls = 2;

			// act
			for (int i = 0; i < numberOfCalls; i++)
			{
				_sut.CreateFieldManager();
			}

			// assert
			_fieldManagerFactoryMock.Verify(x => x(), Times.Exactly(numberOfCalls));
		}

		[Test]
		public void CreateSearchManager_ShouldReturnProperObject()
		{
			// arrange
			var searchManagerMock = new Mock<ISearchManager>();
			_searchManagerFactoryMock.Setup(x => x()).Returns(searchManagerMock.Object);

			// act
			ISearchManager actualSearchManager = _sut.CreateSearchManager();

			// assert
			actualSearchManager.Should().Be(searchManagerMock.Object);
		}

		[Test]
		public void CreateSearchManager_ShouldCallFactoryForEachCall()
		{
			// arrange
			int numberOfCalls = 2;

			// act
			for (int i = 0; i < numberOfCalls; i++)
			{
				_sut.CreateSearchManager();
			}

			// assert
			_searchManagerFactoryMock.Verify(x => x(), Times.Exactly(numberOfCalls));
		}

		[Test]
		public void CreateExportManager_ShouldReturnProperImplementation()
		{
			//arrange
			var exportManagerMock = new Mock<IExportManager>();
			_webApiServiceFactoryMock
				.Setup(x => x.CreateExportManager())
				.Returns(exportManagerMock.Object);

			//act & assert
			IExportManager actualExportManager = _sut.CreateExportManager();

			// assert
			actualExportManager.Should().Be(exportManagerMock.Object);
		}

		[Test]
		public void CreateExportManager_ShouldCallFactoryForEachCall()
		{
			// arrange
			int numberOfCalls = 2;

			// act
			for (int i = 0; i < numberOfCalls; i++)
			{
				_sut.CreateExportManager();
			}

			// assert
			_webApiServiceFactoryMock.Verify(
				x => x.CreateExportManager(), 
				Times.Exactly(numberOfCalls)
			);
		}

		[Test]
		public void CreateProductionManager_ShouldReturnProperImplementation()
		{
			//arrange
			var productionManagerMock = new Mock<IProductionManager>();
			_webApiServiceFactoryMock.Setup(x => x.CreateProductionManager()).Returns(productionManagerMock.Object);

			//act & assert
			IProductionManager actualProductionManager = _sut.CreateProductionManager();

			// assert
			actualProductionManager.Should().Be(productionManagerMock.Object);
		}

		[Test]
		public void CreateProductionManager_ShouldCallFactoryForEachCall()
		{
			// arrange
			int numberOfCalls = 2;

			// act
			for (int i = 0; i < numberOfCalls; i++)
			{
				_sut.CreateProductionManager();
			}

			// assert
			_webApiServiceFactoryMock.Verify(x => x.CreateProductionManager(), Times.Exactly(numberOfCalls));
		}

		[Test]
		public void CreateExportFileDownloader_ShouldReturnProperObject()
		{
			// arrange
			var exportFileDownloaderMock = new Mock<IExportFileDownloader>();
			_exportFileDownloaderFactoryMock
				.Setup(x => x.Create(_exportFile))
				.Returns(exportFileDownloaderMock.Object);

			// act
			IExportFileDownloader actualExportFileDownloader = _sut.CreateExportFileDownloader();

			// assert
			actualExportFileDownloader.Should().Be(exportFileDownloaderMock.Object);
		}

		[Test]
		public void CreateExportFileDownloader_ShouldCallFactoryForEachCall()
		{
			// arrange
			int numberOfCalls = 2;

			// act
			for (int i = 0; i < numberOfCalls; i++)
			{
				_sut.CreateExportFileDownloader();
			}

			// assert
			_exportFileDownloaderFactoryMock.Verify(
				x => x.Create(_exportFile),
				Times.Exactly(numberOfCalls));
		}

		private CoreServiceFactory CreateCoreServiceFactory()
		{
			return new CoreServiceFactory(
				_auditManagerFactoryMock.Object,
				_fieldManagerFactoryMock.Object,
				_searchManagerFactoryMock.Object,
				_exportFileDownloaderFactoryMock.Object,
				_exportFile,
				_webApiServiceFactoryMock.Object);
		}
	}
}
