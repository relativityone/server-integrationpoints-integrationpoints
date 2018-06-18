using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
	[TestFixture]
	class ImportProductionValidatorTests
	{
		private IProductionManager _productionManager;
		private IPermissionManager _permissionManager;
		private const int _WORKSPACE_ARTIFACT_ID = 1;
		private const int _PRODUCTION_ARTIFACT_ID = 2;
		private const int _FEDERATED_INSTANCE_ID = 3;
		private const string _CREDENTIALS = "";

		[SetUp]
		public void SetUp()
		{
			_productionManager = Substitute.For<IProductionManager>();
			_permissionManager = Substitute.For<IPermissionManager>();
			SwitchArtifactInstancePermissionValue(true);
		}

		[Test]
		public void ItShouldValidateProduction()
		{
			// arrange
			var production = new ProductionDTO() { ArtifactID = _PRODUCTION_ARTIFACT_ID.ToString() };
			_productionManager.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID, _FEDERATED_INSTANCE_ID, _CREDENTIALS).Returns(new[] { production });

			var validator = new ImportProductionValidator(_WORKSPACE_ARTIFACT_ID, _productionManager, _permissionManager, _FEDERATED_INSTANCE_ID, _CREDENTIALS);

			// act
			ValidationResult actual = validator.Validate(_PRODUCTION_ARTIFACT_ID);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.MessageTexts.Count(), Is.EqualTo(0));
		}

		[Test]
		public void ItShouldFailForNotFoundProduction()
		{
			// arrange
			_productionManager.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID, _FEDERATED_INSTANCE_ID, _CREDENTIALS).Returns(Enumerable.Empty<ProductionDTO>());

			var validator = new ImportProductionValidator(_WORKSPACE_ARTIFACT_ID, _productionManager, _permissionManager, _FEDERATED_INSTANCE_ID, _CREDENTIALS);

			// act
			ValidationResult actual = validator.Validate(_PRODUCTION_ARTIFACT_ID);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.Messages.Any(m=>m.Equals(ValidationMessages.MissingDestinationProductionPermissions)));
		}

		[Test]
		public void ItShouldFailForEmptyProduction()
		{
			// arrange
			_productionManager.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID, _FEDERATED_INSTANCE_ID, _CREDENTIALS).Returns(new[] { (ProductionDTO)null });

			var validator = new ImportProductionValidator(_PRODUCTION_ARTIFACT_ID, _productionManager, _permissionManager, _FEDERATED_INSTANCE_ID, _CREDENTIALS);

			// act
			ValidationResult actual = validator.Validate(_PRODUCTION_ARTIFACT_ID);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.Messages.Any(m => m.Equals(ValidationMessages.MissingDestinationProductionPermissions)));
		}
		
		[Test]
		public void ItShouldNotValidate_ProductionDataSource_WhenNoAccessToProduction()
		{
			// arrange
			_productionManager.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID, _FEDERATED_INSTANCE_ID, _CREDENTIALS).Returns(new[] { (ProductionDTO)null });

			var validator = new ImportProductionValidator(_PRODUCTION_ARTIFACT_ID, _productionManager, _permissionManager, _FEDERATED_INSTANCE_ID, _CREDENTIALS);

			// act
			ValidationResult actual = validator.Validate(_PRODUCTION_ARTIFACT_ID);

			// assert
			_permissionManager.DidNotReceiveWithAnyArgs()
				.UserHasArtifactInstancePermission(Arg.Any<int>(), Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<ArtifactPermission>());
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ItShouldValidate_ProductionDataSource_WhenProductionAvailable(bool hasAccessToCreate)
		{
			// arrange
			SwitchArtifactInstancePermissionValue(hasAccessToCreate);
			var production = new ProductionDTO() { ArtifactID = _PRODUCTION_ARTIFACT_ID.ToString() };
			_productionManager.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID, _FEDERATED_INSTANCE_ID, _CREDENTIALS).Returns(new[] { production });

			var validator = new ImportProductionValidator(_WORKSPACE_ARTIFACT_ID, _productionManager, _permissionManager, _FEDERATED_INSTANCE_ID, _CREDENTIALS);

			// act
			ValidationResult actual = validator.Validate(_PRODUCTION_ARTIFACT_ID);

			// assert
			_permissionManager.Received().UserHasArtifactInstancePermission(_WORKSPACE_ARTIFACT_ID, Constants.ObjectTypeArtifactTypesGuid.ProductionDataSource, _PRODUCTION_ARTIFACT_ID, ArtifactPermission.Create);
			Assert.AreEqual(hasAccessToCreate, actual.IsValid);
		}

		private void SwitchArtifactInstancePermissionValue(bool value)
		{
			_permissionManager
				.UserHasArtifactInstancePermission(Arg.Any<int>(), Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<ArtifactPermission>())
				.Returns(value);
		}
	}
}

