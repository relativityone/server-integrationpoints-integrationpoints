using System;
using System.Collections.Generic;
using System.Linq;
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
	[IdentifiedTestFixture("1B7BBD4A-83D1-4BDC-A9BD-EF7C1C4E1546")]
	public class ValidationExecutorTests : TestsBase
	{
		[IdentifiedTest("E185F16C-6645-4B14-AD30-D26A98D265F5")]
		public void ValidateOnRun_ShouldValidateAndNotThrow()
		{
			// Act & Assert
			ValidateOnOperationShouldNotThrow(
				(executor, context) => executor.ValidateOnRun(context));
		}

		[IdentifiedTest("486B0390-AF71-45CA-AC81-5830D00BF00C")]
		public void ValidateOnSave_ShouldValidateAndNotThrow()
		{
			// Act & Assert
			ValidateOnOperationShouldNotThrow(
				(executor, context) => executor.ValidateOnSave(context));
		}

		[IdentifiedTest("DE07279A-1F14-4F82-9981-B93108B04763")]
		public void ValidateOnStop_ShouldValidateAndNotThrow()
		{
			// Act & Assert
			ValidateOnOperationShouldNotThrow(
				(executor, context) => executor.ValidateOnStop(context));
		}

		[IdentifiedTest("69E2F16A-7B05-4C63-B0CC-825B4BBB5F32")]
		public void ValidateOnProfile_ShouldValidate()
		{
			// Arrange
			ValidationContext context = PrepareValidationContext();

			IValidationExecutor sut = PrepareSut();

			// Act 
			ValidationResult result = sut.ValidateOnProfile(context);

			// Assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		private void ValidateOnOperationShouldNotThrow(Action<IValidationExecutor, ValidationContext> validateAction)
		{
			// Arrange
			ValidationContext context = PrepareValidationContext();

			IValidationExecutor sut = PrepareSut();

			// Act 
			Action validation = () => validateAction(sut, context);

			// Assert
			validation.ShouldNotThrow();
		}

		private IValidationExecutor PrepareSut()
		{
			return Container.Resolve<IValidationExecutor>();
		}

		private ValidationContext PrepareValidationContext()
		{
			WorkspaceTest destinationWorkspace = HelperManager.WorkspaceHelper.CreateWorkspace();

			FolderTest destinationFolder = Database.Folders.First(x => x.WorkspaceId == destinationWorkspace.ArtifactId);

			SavedSearchTest sourceSavedSearch = Database.SavedSearches.First(x => x.WorkspaceId == SourceWorkspace.ArtifactId);

			IntegrationPointTypeTest integrationPointType =
				HelperManager.IntegrationPointTypeHelper.CreateIntegrationPointType(new IntegrationPointTypeTest
				{
					WorkspaceId = SourceWorkspace.ArtifactId,
					Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString()
				});

			SourceProviderTest sourceProvider = HelperManager.SourceProviderHelper.CreateSourceProvider(SourceWorkspace);

			DestinationProviderTest destinationProvider = HelperManager.DestinationProviderHelper.CreateDestinationProvider(SourceWorkspace);

			List<FieldMap> fieldsMapping = PrepareFieldsMapping(SourceWorkspace, destinationWorkspace);

			IntegrationPointTest integrationPoint =
				HelperManager.IntegrationPointHelper.CreateIntegrationPoint(new IntegrationPointTest
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
						FieldOverlayBehavior = RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT,
						ArtifactTypeId = (int)ArtifactType.Document,
						DestinationFolderArtifactId = destinationFolder.ArtifactId,
						CaseArtifactId = destinationWorkspace.ArtifactId
					}),
					SourceProvider = sourceProvider.ArtifactId,
					EnableScheduler = true,
					ScheduleRule = ScheduleRuleTest.CreateWeeklyRule(
							new DateTime(2021, 3, 20), new DateTime(2021, 3, 30), TimeZoneInfo.Utc, DaysOfWeek.Friday)
						.Serialize(),
					DestinationProvider = destinationProvider.ArtifactId,
					Type = integrationPointType.ArtifactId
				});

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
