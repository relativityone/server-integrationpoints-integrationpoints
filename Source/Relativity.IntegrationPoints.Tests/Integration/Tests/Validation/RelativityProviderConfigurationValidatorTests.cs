using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Validation
{
	[IdentifiedTestFixture("B629C641-BE85-41D9-BE17-4FB095AE96F7")]
	[TestExecutionCategory.CI, TestLevel.L1]
	public class RelativityProviderConfigurationValidatorTests : TestsBase
	{
		[IdentifiedTest("2FF7622C-3DFB-4F07-9C96-255200F4AC5E")]
		public void RelativityProviderConfigurationValidator_ShouldValidate()
		{
			// Arrange
			WorkspaceTest destinationWorkspace = HelperManager.WorkspaceHelper.CreateWorkspace();

			FolderTest destinationFolder = Database.Folders.First(x => x.WorkspaceId == destinationWorkspace.ArtifactId);

			SavedSearchTest sourceSavedSearch = Database.SavedSearches.First(x => x.WorkspaceId == SourceWorkspace.ArtifactId);

			List<FieldMap> fieldsMapping = PrepareFieldsMapping(SourceWorkspace, destinationWorkspace);

			object value = new IntegrationPointProviderValidationModel()
			{
				SourceConfiguration = Serializer.Serialize(new SourceConfiguration
				{
					SourceWorkspaceArtifactId = SourceWorkspace.ArtifactId,
					TargetWorkspaceArtifactId = destinationWorkspace.ArtifactId,
					TypeOfExport = SourceConfiguration.ExportType.SavedSearch,
					SavedSearchArtifactId = sourceSavedSearch.ArtifactId,
				}),
				DestinationConfiguration = Serializer.Serialize(new ImportSettings
				{
					ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
					FieldOverlayBehavior = RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT,
					ArtifactTypeId = (int)ArtifactType.Document,
					DestinationFolderArtifactId = destinationFolder.ArtifactId,
					CaseArtifactId = destinationWorkspace.ArtifactId
				}),
				FieldsMap = Serializer.Serialize(fieldsMapping)
			};

			IValidator sut = Container.Resolve<IValidator>(nameof(RelativityProviderConfigurationValidator));

			// Act
			ValidationResult result = sut.Validate(value);

			// Assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		private List<FieldMap> PrepareFieldsMapping(WorkspaceTest sourceWorkspace, WorkspaceTest destinationWorkspace)
		{
			FieldTest sourceControlNumber = Database.Fields.First(x =>
				x.WorkspaceId == sourceWorkspace.ArtifactId &&
				x.Name == "Control Number");
			FieldTest destinationControlNumber = Database.Fields.First(x =>
				x.WorkspaceId == destinationWorkspace.ArtifactId &&
				x.Name == "Control Number");

			return new List<FieldMap>
			{
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = sourceControlNumber.Name,
						FieldIdentifier = sourceControlNumber.ArtifactId.ToString(),
						FieldType = FieldType.String,
						IsIdentifier = true,
						IsRequired = true,
						Type = ""
					},
					DestinationField = new FieldEntry
					{
						DisplayName = destinationControlNumber.Name,
						FieldIdentifier = destinationControlNumber.ArtifactId.ToString(),
						FieldType = FieldType.String,
						IsIdentifier = true,
						IsRequired = true,
						Type = ""
					},
					FieldMapType = FieldMapTypeEnum.Identifier
				}
			};
		}
	}
}
