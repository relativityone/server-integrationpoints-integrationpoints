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
				//[REL-838809]: Resolve correlationIdFunc
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
			_webApiServiceFactoryMock.Setup(x => x.CreateFieldManager()).Returns(fieldManagerMock.Object);

			// act
			//[REL-838809]: Resolve correlationIdFunc
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
				//[REL-838809]: Resolve correlationIdFunc
				_sut.CreateFieldManager();
			}

			// assert
			_webApiServiceFactoryMock.Verify(x => x.CreateFieldManager(), Times.Exactly(numberOfCalls));
		}

		[Test]
		public void CreateSearchManager_ShouldReturnProperObject()
		{
			// arrange
			var searchManagerMock = new Mock<ISearchManager>();
			_webApiServiceFactoryMock.Setup(x => x.CreateSearchManager()).Returns(searchManagerMock.Object);

			// act
			//[REL-838809]: Resolve correlationIdFunc
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
				//[REL-838809]: Resolve correlationIdFunc
				_sut.CreateSearchManager();
			}

			// assert
			_webApiServiceFactoryMock.Verify(x => x.CreateSearchManager(), Times.Exactly(numberOfCalls));
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
			//[REL-838809]: Resolve correlationIdFunc
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
				//[REL-838809]: Resolve correlationIdFunc
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
			//[REL-838809]: Resolve correlationIdFunc
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
				//[REL-838809]: Resolve correlationIdFunc
				_sut.CreateProductionManager();
			}

			// assert
			_webApiServiceFactoryMock.Verify(x => x.CreateProductionManager(), Times.Exactly(numberOfCalls));
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
