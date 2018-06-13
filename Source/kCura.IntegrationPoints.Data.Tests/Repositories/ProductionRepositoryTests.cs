using System;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.Productions.Services;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Repositories
{
	[TestFixture]
	public class ProductionRepositoryTests
	{
		private const int _WORKSPACE_ARTIFACT_ID = 1016868;
		private const int _PRODUCTION_ARTIFACT_ID = 1234;
		private IServicesMgr _servicesMgr;
		private IProductionManager _productionManager;
		private IProductionRepository _instance;

		[SetUp]
		public void SetUp()
		{
			_productionManager = Substitute.For<IProductionManager>();

			_servicesMgr = Substitute.For<IServicesMgr>();
			_servicesMgr.CreateProxy<IProductionManager>(ExecutionIdentity.CurrentUser).Returns(_productionManager);

			_instance = new ProductionRepository(_servicesMgr);
		}

		[Test]
		public void ItShouldRetrieveExistingProduction()
		{
			// Arrange 
			const int expectedArtifactId = 4321;
			const string expectedName = "productionName";
			var production = new Production()
			{
				ArtifactID = expectedArtifactId,
				Name = expectedName
			};
			_productionManager.ReadSingleAsync(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Returns(production);

			// Act
			ProductionDTO actualResult = _instance.RetrieveProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID);

			// Assert
			Assert.That(actualResult.ArtifactID, Is.EqualTo(expectedArtifactId.ToString()));
			Assert.That(actualResult.DisplayName, Is.EqualTo(expectedName));
		}

		[Test]
		public void ItShouldNoRetrieveAndThrowException()
		{
			// Arrange
			_productionManager.ReadSingleAsync(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Result.Throws(new Exception());

			// Act & Assert
			Assert.Throws<Exception>(() => _instance.RetrieveProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID), "Unable to retrieve production");
		}

		[Test]
		public void ItShouldCreateSingleProduction()
		{
			// Arrange
			const int expectedResult = 1000;
			var production = new Production(); 
			_productionManager.CreateSingleAsync(_WORKSPACE_ARTIFACT_ID, production).Returns(expectedResult);

			// Act
			int actualResult = _instance.CreateSingle(_WORKSPACE_ARTIFACT_ID, production);

			//Assert
			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void ItShouldNotCreateSingleProduction()
		{
			// Arrange
			var production = new Production();
			_productionManager.CreateSingleAsync(_WORKSPACE_ARTIFACT_ID, production).Result.Throws(new Exception());

			// Act & Assert
			Assert.Throws<Exception>(() => _instance.CreateSingle(_WORKSPACE_ARTIFACT_ID, production),
				"Unable to create production");
		}
	}
}
