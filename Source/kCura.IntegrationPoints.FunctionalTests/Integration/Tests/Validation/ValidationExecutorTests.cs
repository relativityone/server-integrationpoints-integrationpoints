using System;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Validation
{
    public class ValidationExecutorTests : TestsBase
    {
        [IdentifiedTest("486B0390-AF71-45CA-AC81-5830D00BF00C")]
        public void ValidateOnSave_ShouldValidateAndNotThrow()
        {
            // Act & Assert
            ValidateOnOperationShouldNotThrow(PrepareSavedSearchSyncIntegrationPoint(), (executor, context) => executor.ValidateOnSave(context));
        }

        [IdentifiedTest("E185F16C-6645-4B14-AD30-D26A98D265F5")]
        public void ValidateOnRun_ShouldValidateAndNotThrow()
        {
            // Act & Assert
            ValidateOnOperationShouldNotThrow(PrepareSavedSearchSyncIntegrationPoint(), (executor, context) => executor.ValidateOnRun(context));
        }

        [IdentifiedTest("DE07279A-1F14-4F82-9981-B93108B04763")]
        public void ValidateOnStop_ShouldValidateAndNotThrow()
        {
            // Act & Assert
            ValidateOnOperationShouldNotThrow(PrepareSavedSearchSyncIntegrationPoint(), (executor, context) => executor.ValidateOnStop(context));
        }

        [IdentifiedTest("69E2F16A-7B05-4C63-B0CC-825B4BBB5F32")]
        public void ValidateOnProfile_ShouldValidate()
        {
            // Arrange
            IntegrationPointFake integrationPoint = PrepareSavedSearchSyncIntegrationPoint();
            ValidationContext context = PrepareValidationContext(integrationPoint);

            IValidationExecutor sut = PrepareSut();

            // Act
            ValidationResult result = sut.ValidateOnProfile(context);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Messages.Should().BeEmpty();
        }

        [IdentifiedTest("794CAA0A-64D9-42A1-A3CA-0564014EF53C")]
        public void ValidateOnSave_ShouldThrow_WhenMappingIdentifierOnly()
        {
            // Arrange
            IntegrationPointFake integrationPoint = PrepareImportEntityFromLdapIntegrationPoint(true);
            ValidationContext context = PrepareValidationContext(integrationPoint);

            IValidationExecutor sut = PrepareSut();

            // Act
            Action validation = () => sut.ValidateOnSave(context);

            // Assert
            validation.ShouldThrow<IntegrationPointValidationException>($"Field: \"First Name\" should be mapped in Destination");
        }

        private void ValidateOnOperationShouldNotThrow(IntegrationPointFake integrationPoint, Action<IValidationExecutor, ValidationContext> validateAction)
        {
            // Arrange
            ValidationContext context = PrepareValidationContext(integrationPoint);

            IValidationExecutor sut = PrepareSut();

            // Act
            Action validation = () => validateAction(sut, context);

            // Assert
            validation.ShouldNotThrow();
        }

        private void ValidateOnOperationShouldThrow<TException>(IntegrationPointFake integrationPoint, Action<IValidationExecutor, ValidationContext> validateAction) where TException : Exception
        {
            // Arrange
            ValidationContext context = PrepareValidationContext(integrationPoint);

            IValidationExecutor sut = PrepareSut();

            // Act
            Action validation = () => validateAction(sut, context);

            // Assert
            validation.ShouldThrow<TException>();
        }

        private IValidationExecutor PrepareSut()
        {
            return Container.Resolve<IValidationExecutor>();
        }

        private ValidationContext PrepareValidationContext(IntegrationPointFake integrationPoint)
        {
            IntegrationPointTypeFake integrationPointType = SourceWorkspace.IntegrationPointTypes.Single(x => x.ArtifactId == integrationPoint.Type);

            SourceProviderFake sourceProvider = SourceWorkspace.SourceProviders.Single(x => x.ArtifactId == integrationPoint.SourceProvider);

            DestinationProviderFake destinationProvider = SourceWorkspace.DestinationProviders.Single(x => x.ArtifactId == integrationPoint.DestinationProvider);

            ValidationContext context = new ValidationContext
            {
                Model = integrationPoint.ToDto(),
                SourceProvider = sourceProvider.ToRdo(),
                DestinationProvider = destinationProvider.ToRdo(),
                IntegrationPointType = integrationPointType.ToRdo(),
                ObjectTypeGuid = ObjectTypeGuids.IntegrationPointGuid,
                UserId = User.ArtifactId
            };

            return context;
        }

        private IntegrationPointFake PrepareSavedSearchSyncIntegrationPoint()
        {
            WorkspaceFake destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointFake integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(destinationWorkspace);
            return integrationPoint;
        }

        private IntegrationPointFake PrepareNonDocumentSyncIntegrationPoint()
        {
            WorkspaceFake destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointFake integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper.CreateNonDocumentSyncIntegrationPoint(destinationWorkspace);
            return integrationPoint;
        }

        private IntegrationPointFake PrepareImportEntityFromLdapIntegrationPoint(bool isMappingIdentifierOnly)
        {
            IntegrationPointFake integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportEntityFromLdapIntegrationPoint(isMappingIdentifierOnly: isMappingIdentifierOnly);
            return integrationPoint;
        }
    }
}
