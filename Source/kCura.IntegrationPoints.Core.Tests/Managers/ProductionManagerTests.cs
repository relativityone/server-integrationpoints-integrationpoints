using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.EDDS.WebAPI.ProductionManagerBase;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Productions.Services;
using Relativity.Toggles;
using IProductionManager = kCura.IntegrationPoints.Core.Managers.IProductionManager;
using ProductionManager = kCura.IntegrationPoints.Core.Managers.Implementations.ProductionManager;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture, Category("Unit")]
	public class ProductionManagerTests
	{
		private IRepositoryFactory _repositoryFactory;
		private IProductionRepository _productionRepository;
		private IProductionManager _sut;
		private IServiceManagerProvider _serviceManagerProvider;
		private WinEDDS.Service.Export.IProductionManager _productionManagerService;
        private IProductionManagerWrapper _productionManagerWrapper;

		private const int _WORKSPACE_ARTIFACT_ID = 101810;
		private const int _PRODUCTION_ARTIFACT_ID = 987654;

		[SetUp]
		public void SetUp()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_productionRepository = Substitute.For<IProductionRepository>();
			_productionManagerService = Substitute.For<WinEDDS.Service.Export.IProductionManager>();
			_serviceManagerProvider = Substitute.For<IServiceManagerProvider>();
            _productionManagerWrapper = Substitute.For<IProductionManagerWrapper>();
			IAPILog logger = Substitute.For<IAPILog>();

			_sut = new ProductionManager(logger, _repositoryFactory, _productionManagerWrapper);
		}

		[Test]
		public void ItShouldRetrieveProduction()
		{
			// Arange 
			const string expectedArtifactId = "123456";
			const string expectedDisplayName = "expectedDisplayName";
			var production = new ProductionDTO()
			{
				ArtifactID = expectedArtifactId,
				DisplayName = expectedDisplayName
			};
			_productionRepository.RetrieveProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Returns(production);
			_repositoryFactory.GetProductionRepository(_WORKSPACE_ARTIFACT_ID).Returns(_productionRepository);

			// Act 
			ProductionDTO actual = _sut.RetrieveProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

			// Assert
			Assert.That(actual.ArtifactID, Is.EqualTo(expectedArtifactId));
			Assert.That(actual.DisplayName, Is.EqualTo(expectedDisplayName));
		}


		[Test]
		public void ItShouldNotRetrieveAndThrowException()
		{
			// Arrange
			_productionRepository.RetrieveProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Throws(new Exception());
			_repositoryFactory.GetProductionRepository(_WORKSPACE_ARTIFACT_ID).Returns(_productionRepository);

			// Act & Assert
			Assert.Throws<Exception>(() => _sut.RetrieveProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID));
		}

		[Test]
		public void ItShouldCreateSingleProduction()
		{
			// Arrange
			var production = new Production();
			const int expectedResult = 99;
			_productionRepository.CreateSingle(_WORKSPACE_ARTIFACT_ID, production).Returns(expectedResult);
			_repositoryFactory.GetProductionRepository(_WORKSPACE_ARTIFACT_ID).Returns(_productionRepository);

			// Act
			int actualResult = _sut.CreateSingle(_WORKSPACE_ARTIFACT_ID, production);

			// Assert
			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void ItShouldNotCreateSingleProductionAndThrowException()
		{
			// Arrange
			var production = new Production();
			_productionRepository.CreateSingle(_WORKSPACE_ARTIFACT_ID, production).Throws(new Exception());
			_repositoryFactory.GetProductionRepository(_WORKSPACE_ARTIFACT_ID).Returns(_productionRepository);

			// Act & Assert
			Assert.Throws<Exception>(() => _sut.CreateSingle(_WORKSPACE_ARTIFACT_ID, production),
				"Unable to create production");
		}

		[Test]
		public void ItShouldGetProductionsForExport()
		{
			// Arrange
			const string expectedArtifactId = "123456";
			const string expectedDisplayName = "expectedDisplayName";
			DataSet expectedResult = CreateNewProductionDataTable(expectedArtifactId, expectedDisplayName);

			_productionManagerService.RetrieveProducedByContextArtifactID(_WORKSPACE_ARTIFACT_ID).Returns(expectedResult);
			_serviceManagerProvider.Create<WinEDDS.Service.Export.IProductionManager, ProductionManagerFactory>()
				.Returns(_productionManagerService);

			// Act
			List<ProductionDTO> actualProductionDto = _sut.GetProductionsForExport(_WORKSPACE_ARTIFACT_ID).ToList();

			// Assert
			Assert.That(actualProductionDto.Count, Is.EqualTo(1));
			Assert.That(actualProductionDto.First().ArtifactID, Is.EqualTo(expectedArtifactId));
			Assert.That(actualProductionDto.First().DisplayName, Is.EqualTo(expectedDisplayName));
		}

		[Test]
		public void ItShouldGetProductionsForImport()
		{
			// Arrange
			const string expectedArtifactId = "123456";
			const string expectedDisplayName = "expectedDisplayName";
			DataSet expectedResult = CreateNewProductionDataTable(expectedArtifactId, expectedDisplayName);

			_productionManagerService.RetrieveImportEligibleByContextArtifactID(_WORKSPACE_ARTIFACT_ID).Returns(expectedResult);
			_serviceManagerProvider
				.Create<WinEDDS.Service.Export.IProductionManager, ProductionManagerFactory>()
				.Returns(_productionManagerService);

			// Act
			List<ProductionDTO> actualProductionDto = _sut.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID).ToList();

			// Assert
			Assert.That(actualProductionDto.Count, Is.EqualTo(1));
			Assert.That(actualProductionDto.First().ArtifactID, Is.EqualTo(expectedArtifactId));
			Assert.That(actualProductionDto.First().DisplayName, Is.EqualTo(expectedDisplayName));
		}

		[Test]
		public void IsProductionInDestinationWorkspaceAvailable_ShouldReturnTrue_WhenProductionManagerReturnsNonNullObject()
		{
			// arrange
			var productionInfo = new ProductionInfo();
			_productionManagerService.Read(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Returns(productionInfo);
			_serviceManagerProvider
				.Create<WinEDDS.Service.Export.IProductionManager, ProductionManagerFactory>()
				.Returns(_productionManagerService);

			// act
			bool result = _sut.IsProductionInDestinationWorkspaceAvailable(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

			// assert
			Assert.IsTrue(result);
		}

		[Test]
		public void IsProductionInDestinationWorkspaceAvailable_ShouldReturnFalse_WhenProductionManagerReturnsNonNullObject()
		{
			// arrange
			ProductionInfo productionInfo = null;
			_productionManagerService.Read(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Returns(productionInfo);
			_serviceManagerProvider
				.Create<WinEDDS.Service.Export.IProductionManager, ProductionManagerFactory>()
				.Returns(_productionManagerService);

			// act
			bool result = _sut.IsProductionInDestinationWorkspaceAvailable(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

			// assert
			Assert.IsFalse(result);
		}

		[Test]
		public void IsProductionInDestinationWorkspaceAvailable_ShouldReturnFalse_WhenProductionManagerThrowsException()
		{
			_productionManagerService.Read(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Throws<Exception>();
			_serviceManagerProvider
				.Create<WinEDDS.Service.Export.IProductionManager, ProductionManagerFactory>()
				.Returns(_productionManagerService);

			// act
			bool result = _sut.IsProductionInDestinationWorkspaceAvailable(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

			// assert
			Assert.IsFalse(result);
		}

		[Test]
		public void IsProductionEligibleForImport_ShouldReturnTrue_WhenProductionManagerReturnsThisProduction()
		{
			// Arrange
			DataSet expectedResult = CreateNewProductionDataTable(_PRODUCTION_ARTIFACT_ID);
			_productionManagerService.RetrieveImportEligibleByContextArtifactID(_WORKSPACE_ARTIFACT_ID).Returns(expectedResult);
			_serviceManagerProvider
				.Create<WinEDDS.Service.Export.IProductionManager, ProductionManagerFactory>()
				.Returns(_productionManagerService);

			// Act
			bool result = _sut.IsProductionEligibleForImport(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

			// Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void IsProductionEligibleForImport_ShouldReturnFalse_WhenProductionManagerReturnsEmptyList()
		{
			// Arrange
			DataSet expectedResult = CreateEmptyProductionDataSet();
			_productionManagerService.RetrieveImportEligibleByContextArtifactID(_WORKSPACE_ARTIFACT_ID).Returns(expectedResult);
			_serviceManagerProvider
				.Create<WinEDDS.Service.Export.IProductionManager, ProductionManagerFactory>()
				.Returns(_productionManagerService);

			// Act
			bool result = _sut.IsProductionEligibleForImport(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

			// Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void IsProductionEligibleForImport_ShouldReturnFalse_WhenProductionManagerReturnsListWithoutThisProduction()
		{
			// Arrange
			DataSet expectedResult = CreateNewProductionDataTable(_PRODUCTION_ARTIFACT_ID + 1);
			_productionManagerService.RetrieveImportEligibleByContextArtifactID(_WORKSPACE_ARTIFACT_ID).Returns(expectedResult);
			_serviceManagerProvider
				.Create<WinEDDS.Service.Export.IProductionManager, ProductionManagerFactory>()
				.Returns(_productionManagerService);

			// Act
			bool result = _sut.IsProductionEligibleForImport(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

			// Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void IsProductionEligibleForImport_ShouldReturnFalse_WhenProductionManagerThrowsException()
		{
			_productionManagerService.RetrieveImportEligibleByContextArtifactID(_WORKSPACE_ARTIFACT_ID)
				.Throws<Exception>();
			_serviceManagerProvider
				.Create<WinEDDS.Service.Export.IProductionManager, ProductionManagerFactory>()
				.Returns(_productionManagerService);

			// Act
			bool result = _sut.IsProductionEligibleForImport(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

			// Assert
			Assert.IsFalse(result);
		}

		private DataSet CreateNewProductionDataTable(int expectedArtifactId, string expectedDisplayName = null)
		{
			return CreateNewProductionDataTable(expectedArtifactId.ToString(), expectedDisplayName ?? string.Empty);
		}

		private DataSet CreateNewProductionDataTable(string expectedArtifactId, string expectedDisplayName)
		{
			var dataTable = new DataTable();
			dataTable.Columns.Add("ArtifactID", typeof(string));
			dataTable.Columns.Add("Name", typeof(string));
			DataRow row = dataTable.NewRow();
			row["ArtifactID"] = expectedArtifactId;
			row["Name"] = expectedDisplayName;
			dataTable.Rows.Add(row);
			var dataSet = new DataSet();
			dataSet.Tables.Add(dataTable);

			return dataSet;
		}

		private DataSet CreateEmptyProductionDataSet()
		{
			var dataTable = new DataTable();
			dataTable.Columns.Add("ArtifactID", typeof(string));
			dataTable.Columns.Add("Name", typeof(string));
			var dataSet = new DataSet();
			dataSet.Tables.Add(dataTable);

			return dataSet;
		}
	}
}
