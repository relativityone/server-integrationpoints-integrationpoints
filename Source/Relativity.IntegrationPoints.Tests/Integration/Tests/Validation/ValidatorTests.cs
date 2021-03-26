using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Validation
{
	[IdentifiedTestFixture("0B863864-88CB-4E28-87B0-05E105E8247F")]
	[TestExecutionCategory.CI, TestLevel.L1]
	public class ValidatorTests : TestsBase
	{
		[IdentifiedTest("989D0BE8-753F-4C53-83E4-7B7296EE1CCB")]
		public void EmailValidator_ShouldValidate()
		{
			// Arrange
			object email = "relativity.admin@kcura.com";

			IValidator sut = Container.Resolve<IValidator>(nameof(EmailValidator));

			// Act
			ValidationResult result = sut.Validate(email);

			// Assert
			VerifyValidationPassed(result);
		}

		[IdentifiedTest("F4C94F16-EFD3-49B4-9C93-9ABCD3588E47")]
		public void ArtifactValidator_ShouldValidate()
		{
			// Arrange
			WorkspaceTest destinationWorkspace = HelperManager.WorkspaceHelper.CreateWorkspace();

			FolderTest folder = Database.Folders.First(x => x.WorkspaceId == destinationWorkspace.ArtifactId);

			IRelativityProviderValidatorsFactory validatorsFactory = Container.Resolve<IRelativityProviderValidatorsFactory>();

			ArtifactValidator sut = validatorsFactory.CreateArtifactValidator(
				destinationWorkspace.ArtifactId, "Folder", null, string.Empty);

			// Act
			ValidationResult result = sut.Validate(folder.ArtifactId);

			// Assert
			VerifyValidationPassed(result);
		}

		[IdentifiedTest("EA5F1E35-204F-44C4-B499-D535337DB61E")]
		public void ImportPermissionValidator_ShouldValidate()
		{
			// Arrange
			IPermissionValidator sut = Container.Resolve<IPermissionValidator>(nameof(ImportPermissionValidator));

			IntegrationPointProviderValidationModel model = new IntegrationPointProviderValidationModel()
			{
				DestinationConfiguration = Serializer.Serialize(new DestinationConfiguration
				{
					ArtifactTypeId = (int) ArtifactType.Document
				})
			};

			// Act
			ValidationResult result = sut.Validate(model);

			// Assert
			VerifyValidationPassed(result);
		}

		[IdentifiedTest("6F0ED21B-1E2A-4E87-AC7A-80E1CC639993")]
		public void PermissionValidator_ShouldValidate()
		{
			// Arrange
			IPermissionValidator sut = Container.Resolve<IPermissionValidator>(nameof(PermissionValidator));

			IntegrationPointProviderValidationModel model = new IntegrationPointProviderValidationModel()
			{
				DestinationConfiguration = Serializer.Serialize(new DestinationConfiguration
				{
					ArtifactTypeId = (int)ArtifactType.Document
				})
			};

			// Act
			ValidationResult result = sut.Validate(model);

			// Assert
			VerifyValidationPassed(result);
		}

		[IdentifiedTest("BB094878-AFAF-4C14-9306-9AE52B4360A0")]
		public void SavePermissionValidator_ShouldValidate()
		{
			// Arrange
			IPermissionValidator sut = Container.Resolve<IPermissionValidator>(nameof(SavePermissionValidator));

			IntegrationPointProviderValidationModel model = new IntegrationPointProviderValidationModel()
			{
				DestinationConfiguration = Serializer.Serialize(new DestinationConfiguration
				{
					ArtifactTypeId = (int)ArtifactType.Document
				})
			};

			// Act
			ValidationResult result = sut.Validate(model);

			// Assert
			VerifyValidationPassed(result);
		}

		[IdentifiedTest("E90DC652-94ED-49D9-A9F7-EF150405FD10")]
		public void StopJobPermissionValidator_ShouldValidate()
		{
			// Arrange
			IPermissionValidator sut = Container.Resolve<IPermissionValidator>(nameof(StopJobPermissionValidator));

			IntegrationPointProviderValidationModel model = new IntegrationPointProviderValidationModel()
			{
				DestinationConfiguration = Serializer.Serialize(new DestinationConfiguration
				{
					ArtifactTypeId = (int)ArtifactType.Document
				})
			};

			// Act
			ValidationResult result = sut.Validate(model);

			// Assert
			VerifyValidationPassed(result);
		}

		[IdentifiedTest("DF3058B5-750F-4ABD-923B-8134A3A3DB8B")]
		public void ViewErrorsPermissionValidator_ShouldValidate()
		{
			// Arrange
			IPermissionValidator sut = Container.Resolve<IPermissionValidator>(nameof(ViewErrorsPermissionValidator));

			// Act
			ValidationResult result = sut.Validate(SourceWorkspace.ArtifactId);

			// Assert
			VerifyValidationPassed(result);
		}

		[IdentifiedTest("D17C18C9-C3C3-4FAB-9AE0-60FC7C1DF9C6")]
		public void RelativityProviderPermissionValidator_ShouldValidate()
		{
			// Arrange
			WorkspaceTest destinationWorkspace = HelperManager.WorkspaceHelper.CreateWorkspace();

			IPermissionValidator sut = Container.Resolve<IPermissionValidator>(nameof(RelativityProviderPermissionValidator));
			
			IntegrationPointProviderValidationModel model = new IntegrationPointProviderValidationModel()
			{
				DestinationConfiguration = Serializer.Serialize(new DestinationConfiguration
				{
					ArtifactTypeId = (int)ArtifactType.Document
				}),
				SourceConfiguration = Serializer.Serialize(new SourceConfiguration
				{
					TargetWorkspaceArtifactId = destinationWorkspace.ArtifactId
				})
			};

			// Act
			ValidationResult result = sut.Validate(model);

			// Assert
			VerifyValidationPassed(result);
		}

		[IdentifiedTest("0CE0DD35-C2F0-4582-94C3-169EFBF6FDCA")]
		public void NativeCopyLinksValidator_ShouldValidate()
		{
			// Arrange
			Context.InstanceSettings.RestrictReferentialFileLinksOnImport = "True";

			WorkspaceTest destinationWorkspace = HelperManager.WorkspaceHelper.CreateWorkspace();

			IPermissionValidator sut = Container.Resolve<IPermissionValidator>(nameof(NativeCopyLinksValidator));

			IntegrationPointProviderValidationModel model = new IntegrationPointProviderValidationModel()
			{
				DestinationConfiguration = Serializer.Serialize(new ImportSettings
				{
					ArtifactTypeId = (int)ArtifactType.Document,
					ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.SetFileLinks
				}),
				SourceConfiguration = Serializer.Serialize(new SourceConfiguration
				{
					TargetWorkspaceArtifactId = destinationWorkspace.ArtifactId
				})
			};

			// Act
			ValidationResult result = sut.Validate(model);

			// Assert
			VerifyValidationPassed(result);
		}

		private void VerifyValidationPassed(ValidationResult result)
		{
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}
	}
}
