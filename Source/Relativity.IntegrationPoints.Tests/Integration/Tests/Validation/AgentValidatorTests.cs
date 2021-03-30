using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Validation
{
	[IdentifiedTestFixture("26D4A411-D812-41B0-85A6-83847E942F45")]
	public class AgentValidatorTests : TestsBase
	{
		[IdentifiedTest("0895F31D-E64B-46C7-ABAC-52ECF904CD79")]
		public void Validate_ShouldNotThrow()
		{
			// Arrange
			IntegrationPoint integrationPoint = PrepareIntegrationPoint();

			IAgentValidator sut = Container.Resolve<IAgentValidator>();

			// Act
			Action validation = () => sut.Validate(integrationPoint, User.ArtifactId);

			// Assert
			validation.ShouldNotThrow();
		}

		private IntegrationPoint PrepareIntegrationPoint()
		{
			WorkspaceTest destinationWorkspace = HelperManager.WorkspaceHelper.CreateWorkspace();

			FolderTest destinationFolder =
				Database.Folders.First(x => x.WorkspaceId == destinationWorkspace.ArtifactId);

			SavedSearchTest sourceSavedSearch =
				Database.SavedSearches.First(x => x.WorkspaceId == SourceWorkspace.ArtifactId);

			IntegrationPointTypeTest integrationPointType =
				HelperManager.IntegrationPointTypeHelper.CreateIntegrationPointType(new IntegrationPointTypeTest
				{
					WorkspaceId = SourceWorkspace.ArtifactId,
					Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes
						.ImportGuid.ToString()
				});

			SourceProviderTest sourceProvider =
				HelperManager.SourceProviderHelper.CreateSourceProvider(SourceWorkspace);

			DestinationProviderTest destinationProvider =
				HelperManager.DestinationProviderHelper.CreateDestinationProvider(SourceWorkspace);

			List<FieldMap> fieldsMapping = PrepareFieldsMapping(SourceWorkspace, destinationWorkspace);

			IntegrationPointTest integrationPoint = HelperManager.IntegrationPointHelper.CreateIntegrationPoint(
				new IntegrationPointTest
				{
					WorkspaceId = SourceWorkspace.ArtifactId,
					FieldMappings = Serializer.Serialize(fieldsMapping),
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
						FieldOverlayBehavior =
							RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT,
						ArtifactTypeId = (int) ArtifactType.Document,
						DestinationFolderArtifactId = destinationFolder.ArtifactId,
						CaseArtifactId = destinationWorkspace.ArtifactId
					}),
					SourceProvider = sourceProvider.ArtifactId,
					DestinationProvider = destinationProvider.ArtifactId,
					Type = integrationPointType.ArtifactId
				});

			return integrationPoint.ToRdo();
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
