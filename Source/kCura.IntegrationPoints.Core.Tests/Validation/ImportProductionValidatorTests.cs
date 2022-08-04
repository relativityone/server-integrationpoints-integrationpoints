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
    [TestFixture, Category("Unit")]
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
        }

        [Test]
        public void ItShouldValidateProduction()
        {
            // arrange
            SwitchIsProductionInDestinationWorkspaceAvailable(true);
            SwitchIsProductionEligibleForImport(true);
            SwitchArtifactInstancePermissionValue(true);

            var validator = new ImportProductionValidator(_WORKSPACE_ARTIFACT_ID, _productionManager, _permissionManager, _FEDERATED_INSTANCE_ID, _CREDENTIALS);

            // act
            ValidationResult actual = validator.Validate(_PRODUCTION_ARTIFACT_ID);

            // assert
            Assert.IsTrue(actual.IsValid);
            Assert.That(actual.MessageTexts.Count(), Is.EqualTo(0));
        }

        [Test]
        public void ItShouldFail_WhenProductionDoesNotExist()
        {
            // arrange
            SwitchIsProductionInDestinationWorkspaceAvailable(false);
            SwitchIsProductionEligibleForImport(true);
            SwitchArtifactInstancePermissionValue(true);

            var validator = new ImportProductionValidator(_WORKSPACE_ARTIFACT_ID, _productionManager, _permissionManager, _FEDERATED_INSTANCE_ID, _CREDENTIALS);

            // act
            ValidationResult actual = validator.Validate(_PRODUCTION_ARTIFACT_ID);

            // assert
            Assert.IsFalse(actual.IsValid);
            Assert.IsTrue(actual.Messages.Any(m=>m.Equals(ValidationMessages.MissingDestinationProductionPermissions)));
        }

        [Test]
        public void ItShouldFail_WhenProductionNotEligibleForImport()
        {
            // arrange
            SwitchIsProductionInDestinationWorkspaceAvailable(true);
            SwitchIsProductionEligibleForImport(false);
            SwitchArtifactInstancePermissionValue(true);

            var validator = new ImportProductionValidator(_WORKSPACE_ARTIFACT_ID, _productionManager, _permissionManager, _FEDERATED_INSTANCE_ID, _CREDENTIALS);

            // act
            ValidationResult actual = validator.Validate(_PRODUCTION_ARTIFACT_ID);

            // assert
            Assert.IsFalse(actual.IsValid);
            Assert.IsTrue(actual.Messages.Any(m => m.Equals(ValidationMessages.DestinationProductionNotEligibleForImport)));
        }
        
        [Test]
        public void ItShouldNotValidate_ProductionDataSource_WhenNoAccessToProduction()
        {
            // arrange
            SwitchIsProductionInDestinationWorkspaceAvailable(false);
            SwitchIsProductionEligibleForImport(true);
            SwitchArtifactInstancePermissionValue(true);

            var validator = new ImportProductionValidator(_PRODUCTION_ARTIFACT_ID, _productionManager, _permissionManager, _FEDERATED_INSTANCE_ID, _CREDENTIALS);

            // act
            ValidationResult actual = validator.Validate(_PRODUCTION_ARTIFACT_ID);

            // assert
            _permissionManager.DidNotReceiveWithAnyArgs()
                .UserHasArtifactInstancePermission(Arg.Any<int>(), Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<ArtifactPermission>());
        }

        [Test]
        public void ItShouldNotValidate_ProductionDataSource_WhenProductionInWrongState()
        {
            // arrange
            SwitchIsProductionInDestinationWorkspaceAvailable(true);
            SwitchIsProductionEligibleForImport(false);
            SwitchArtifactInstancePermissionValue(true);
            var validator = new ImportProductionValidator(_PRODUCTION_ARTIFACT_ID, _productionManager, _permissionManager, _FEDERATED_INSTANCE_ID, _CREDENTIALS);

            // act
            ValidationResult actual = validator.Validate(_PRODUCTION_ARTIFACT_ID);

            // assert
            _permissionManager.DidNotReceiveWithAnyArgs()
                .UserHasArtifactInstancePermission(Arg.Any<int>(), Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<ArtifactPermission>());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ItShouldValidate_ProductionDataSource_WhenProductionAvailable_AndInProperState(bool hasAccessToCreate)
        {
            // arrange
            SwitchIsProductionInDestinationWorkspaceAvailable(true);
            SwitchIsProductionEligibleForImport(true);
            SwitchArtifactInstancePermissionValue(hasAccessToCreate);

            var validator = new ImportProductionValidator(_WORKSPACE_ARTIFACT_ID, _productionManager, _permissionManager, _FEDERATED_INSTANCE_ID, _CREDENTIALS);

            // act
            ValidationResult actual = validator.Validate(_PRODUCTION_ARTIFACT_ID);

            // assert
            _permissionManager.Received().UserHasArtifactInstancePermission(_WORKSPACE_ARTIFACT_ID, Constants.ObjectTypeArtifactTypesGuid.ProductionDataSource, _PRODUCTION_ARTIFACT_ID, ArtifactPermission.Create);
            Assert.AreEqual(hasAccessToCreate, actual.IsValid);
        }

        private void SwitchArtifactInstancePermissionValue(bool hasPermission)
        {
            _permissionManager
                .UserHasArtifactInstancePermission(Arg.Any<int>(), Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<ArtifactPermission>())
                .Returns(hasPermission);
        }

        private void SwitchIsProductionInDestinationWorkspaceAvailable(bool isAvailable)
        {
            _productionManager.IsProductionInDestinationWorkspaceAvailable(Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<int?>(), Arg.Any<string>()).Returns(isAvailable);
        }

        private void SwitchIsProductionEligibleForImport(bool isInProperState)
        {
            _productionManager.IsProductionEligibleForImport(Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<int?>(), Arg.Any<string>()).Returns(isInProperState);
        }
    }
}

