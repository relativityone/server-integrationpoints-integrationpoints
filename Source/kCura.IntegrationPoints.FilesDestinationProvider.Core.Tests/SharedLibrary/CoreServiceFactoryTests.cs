using System;
using FluentAssertions;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.SharedLibrary
{
	[TestFixture, Category("Unit")]
	public class CoreServiceFactoryTests
	{
		private CoreServiceFactory _sut;
		private ExportFile _exportFile;
		private Mock<IServiceFactory> _webApiServiceFactoryMock;
		private Mock<Func<IAuditManager>> _auditManagerFactoryMock;

		private const int _EXPORT_ARTIFACT_TYPE_ID = 123;

		[SetUp]
		public void SetUp()
		{
			_exportFile = new ExportFile(_EXPORT_ARTIFACT_TYPE_ID);

			_webApiServiceFactoryMock = new Mock<IServiceFactory>();
			_auditManagerFactoryMock = new Mock<Func<IAuditManager>>();

			_sut = CreateCoreServiceFactory();
		}

		[Test]
		public void CreateAuditManager_ShouldReturnProperObject()
		{
			// arrange
			var auditManagerMock = new Mock<IAuditManager>();
			_auditManagerFactoryMock.Setup(x => x()).Returns(auditManagerMock.Object);

			// act
			//[REL-838809]: Resolve correlationIdFunc
			IAuditManager actualAuditManager = _sut.CreateAuditManager(() => string.Empty);

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
				
				_sut.CreateAuditManager(() => string.Empty);
			}

			// assert
			_auditManagerFactoryMock.Verify(x => x(), Times.Exactly(numberOfCalls));
		}

		[Test]
		public void CreateFieldManager_ShouldReturnProperObject()
		{
			// arrange
			var fieldManagerMock = new Mock<IFieldManager>();
			_webApiServiceFactoryMock.Setup(x => x.CreateFieldManager(It.IsAny<Func<string>>())).Returns(fieldManagerMock.Object);

			// act
			
			IFieldManager actualFieldManager = _sut.CreateFieldManager(() => string.Empty);

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
				
				_sut.CreateFieldManager(() => string.Empty);
			}

			// assert
			_webApiServiceFactoryMock.Verify(x => x.CreateFieldManager(It.IsAny<Func<string>>()), Times.Exactly(numberOfCalls));
		}

		[Test]
		public void CreateSearchManager_ShouldReturnProperObject()
		{
			// arrange
			var searchManagerMock = new Mock<ISearchManager>();
			_webApiServiceFactoryMock.Setup(x => x.CreateSearchManager(It.IsAny<Func<string>>())).Returns(searchManagerMock.Object);

			// act
			
			ISearchManager actualSearchManager = _sut.CreateSearchManager(() => string.Empty);

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
				
				_sut.CreateSearchManager(() => string.Empty);
			}

			// assert
			_webApiServiceFactoryMock.Verify(x => x.CreateSearchManager(It.IsAny<Func<string>>()), Times.Exactly(numberOfCalls));
		}

		[Test]
		public void CreateExportManager_ShouldReturnProperImplementation()
		{
			//arrange
			var exportManagerMock = new Mock<IExportManager>();
			_webApiServiceFactoryMock
				.Setup(x => x.CreateExportManager(It.IsAny<Func<string>>()))
				.Returns(exportManagerMock.Object);

			//act & assert
			
			IExportManager actualExportManager = _sut.CreateExportManager(() => string.Empty);

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
				
				_sut.CreateExportManager(() => string.Empty);
			}

			// assert
			_webApiServiceFactoryMock.Verify(
				x => x.CreateExportManager(It.IsAny<Func<string>>()), 
				Times.Exactly(numberOfCalls)
			);
		}

		[Test]
		public void CreateProductionManager_ShouldReturnProperImplementation()
		{
			//arrange
			var productionManagerMock = new Mock<IProductionManager>();
			_webApiServiceFactoryMock.Setup(x => x.CreateProductionManager(It.IsAny<Func<string>>())).Returns(productionManagerMock.Object);

			//act & assert
			
			IProductionManager actualProductionManager = _sut.CreateProductionManager(() => string.Empty);

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
				
				_sut.CreateProductionManager(() => string.Empty);
			}

			// assert
			_webApiServiceFactoryMock.Verify(x => x.CreateProductionManager(It.IsAny<Func<string>>()), Times.Exactly(numberOfCalls));
		}

		private CoreServiceFactory CreateCoreServiceFactory()
		{
			return new CoreServiceFactory(
				_auditManagerFactoryMock.Object,
				_exportFile,
				_webApiServiceFactoryMock.Object);
		}
	}
}
