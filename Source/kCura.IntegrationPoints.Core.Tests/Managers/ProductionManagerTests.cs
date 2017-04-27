using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.Productions.Services;
using IProductionManager = kCura.IntegrationPoints.Core.Managers.IProductionManager;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class ProductionManagerTests
	{
		private const int _WORKSPACE_ARTIFACT_ID = 101810;
		private const int _PRODUCTION_ARTIFACT_ID = 987654;
		private IRepositoryFactory _repositoryFactory;
		private IProductionRepository _productionRepository;
		private IProductionManager _instance;
		private IServiceManagerProvider _serviceManagerProvider;
		private WinEDDS.Service.Export.IProductionManager _productionManagerService;

		[SetUp]
		public void SetUp()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_productionRepository = Substitute.For<IProductionRepository>();
			_productionManagerService = Substitute.For<WinEDDS.Service.Export.IProductionManager>();
			_serviceManagerProvider = Substitute.For<IServiceManagerProvider>();
			
			_instance = new ProductionManager(_repositoryFactory, _serviceManagerProvider, Substitute.For<IntegrationPoints.Domain.Managers.IFederatedInstanceManager>());
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
			ProductionDTO actual = _instance.RetrieveProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

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
			Assert.Throws<Exception>(() => _instance.RetrieveProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID));
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
			int actualResult = _instance.CreateSingle(_WORKSPACE_ARTIFACT_ID, production);

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
			Assert.Throws<Exception>(() => _instance.CreateSingle(_WORKSPACE_ARTIFACT_ID, production),
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
			List<ProductionDTO> actualProductionDto = _instance.GetProductionsForExport(_WORKSPACE_ARTIFACT_ID).ToList();

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
				.Create<WinEDDS.Service.Export.IProductionManager, ProductionManagerFactory>(Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<IntegrationPoints.Domain.Managers.IFederatedInstanceManager>())
				.Returns(_productionManagerService);

			// Act
			List<ProductionDTO> actualProductionDto = _instance.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID).ToList();

			// Assert
			Assert.That(actualProductionDto.Count, Is.EqualTo(1));
			Assert.That(actualProductionDto.First().ArtifactID, Is.EqualTo(expectedArtifactId));
			Assert.That(actualProductionDto.First().DisplayName, Is.EqualTo(expectedDisplayName));
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
	}
}
