using System;
using System.Linq;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using Relativity.Toggles;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Validation
{
    public class ValidationExecutorTests : TestsBase
    {
        [IdentifiedTest("C9B3283F-8488-4654-9C44-46D37C15B57F")]
        public void NonDocumentSync_ValidateOnSave_ShouldValidateAndNotThrow()
        {
            // Act & Assert
            SetupNonDocumentFlowToggle(true);
            ValidateOnOperationShouldNotThrow(PrepareNonDocumentSyncIntegrationPoint(), (executor, context) => executor.ValidateOnSave(context));
        }

        [IdentifiedTest("E407A9A3-8263-4016-B2AE-0B2B793EABC8")]
        public void NonDocumentSync_ValidateOnSave_ShouldThrow_WhenToggleIsDisabled()
        {
            // Act & Assert
            ValidateOnOperationShouldThrow<IntegrationPointValidationException>(PrepareNonDocumentSyncIntegrationPoint(), (executor, context) => executor.ValidateOnSave(context));
        }

        [IdentifiedTest("36D3CD2E-E7A7-4140-B8CA-82750B680D51")]
        public void NonDocumentSync_ValidateOnRun_ShouldValidateAndNotThrow()
        {
            // Act & Assert
            SetupNonDocumentFlowToggle(true);
            ValidateOnOperationShouldNotThrow(PrepareNonDocumentSyncIntegrationPoint(), (executor, context) => executor.ValidateOnRun(context));
        }

        [IdentifiedTest("D9197049-35D2-42E4-B656-4346B3248F44")]
        public void NonDocumentSync_ValidateOnRun_ShouldThrow_WhenToggleIsDisabled()
        {
            // Act & Assert
            ValidateOnOperationShouldThrow<IntegrationPointValidationException>(PrepareNonDocumentSyncIntegrationPoint(), (executor, context) => executor.ValidateOnRun(context));
        }

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
            IntegrationPointTest integrationPoint = PrepareSavedSearchSyncIntegrationPoint();
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
            IntegrationPointTest integrationPoint = PrepareImportEntityFromLdapIntegrationPoint(true);
            ValidationContext context = PrepareValidationContext(integrationPoint);

            IValidationExecutor sut = PrepareSut();

            // Act
            Action validation = () => sut.ValidateOnSave(context);

            // Assert
            validation.ShouldThrow<IntegrationPointValidationException>($"Field: \"First Name\" should be mapped in Destination");
        }

        private void ValidateOnOperationShouldNotThrow(IntegrationPointTest integrationPoint, Action<IValidationExecutor, ValidationContext> validateAction)
        {
            // Arrange
            ValidationContext context = PrepareValidationContext(integrationPoint);

            IValidationExecutor sut = PrepareSut();

            // Act 
            Action validation = () => validateAction(sut, context);

            // Assert
            validation.ShouldNotThrow();
        }

        private void ValidateOnOperationShouldThrow<TException>(IntegrationPointTest integrationPoint, Action<IValidationExecutor, ValidationContext> validateAction) where TException : Exception
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

        private ValidationContext PrepareValidationContext(IntegrationPointTest integrationPoint)
        {
            IntegrationPointTypeTest integrationPointType = SourceWorkspace.IntegrationPointTypes.Single(x => x.ArtifactId == integrationPoint.Type);

            SourceProviderTest sourceProvider = SourceWorkspace.SourceProviders.Single(x => x.ArtifactId == integrationPoint.SourceProvider);

            DestinationProviderTest destinationProvider = SourceWorkspace.DestinationProviders.Single(x => x.ArtifactId == integrationPoint.DestinationProvider);

            ValidationContext context = new ValidationContext
            {
                Model = IntegrationPointModel.FromIntegrationPoint(integrationPoint.ToRdo()),
                SourceProvider = sourceProvider.ToRdo(),
                DestinationProvider = destinationProvider.ToRdo(),
                IntegrationPointType = integrationPointType.ToRdo(),
                ObjectTypeGuid = ObjectTypeGuids.IntegrationPointGuid,
                UserId = User.ArtifactId
            };

            return context;
        }

        private IntegrationPointTest PrepareSavedSearchSyncIntegrationPoint()
        {
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(destinationWorkspace);
            return integrationPoint;
        }

        private IntegrationPointTest PrepareNonDocumentSyncIntegrationPoint()
        {
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper.CreateNonDocumentSyncIntegrationPoint(destinationWorkspace);
            return integrationPoint;
        }

        private IntegrationPointTest PrepareImportEntityFromLdapIntegrationPoint(bool isMappingIdentifierOnly)
        {
            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportEntityFromLdapIntegrationPoint(isMappingIdentifierOnly: isMappingIdentifierOnly);
            return integrationPoint;
        }
        
        private void SetupNonDocumentFlowToggle(bool enable)
        {
            Mock<IToggleProvider> toggleProvider = new Mock<IToggleProvider>();
            toggleProvider.Setup(x => x.IsEnabled<EnableSyncNonDocumentFlowToggle>()).Returns(enable);
            Container.Register(Component.For<IToggleProvider>().Instance(toggleProvider.Object).IsDefault());
        }
    }
}
