using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.API;
using Relativity.Productions.Services;
using Relativity.Testing.Identification;
using static kCura.IntegrationPoints.Core.Constants.IntegrationPoints;

namespace Relativity.IntegrationPoints.FunctionalTests.TestCasesBilling
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[NotWorkingOnTrident]
	public class BillingFlagRelativityProviderProductionTests : RelativityProviderTemplate
	{
		private IIntegrationPointService _integrationPointService;
		private int _targetWorkspaceArtifactID;

		private const int _ADMIN_USER_ID = 9;
		private const string _TARGET_WORKSPACE_NAME = "IntegrationPoints Billing - Destination";
		private int _savedSearchId;
		private int _sourceProductionId;
		private int _targetProductionId;


		public BillingFlagRelativityProviderProductionTests()
			: base(sourceWorkspaceName: "IntegrationPoints Billing - Source Productions",
				   targetWorkspaceName: null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			Agent.EnableAllIntegrationPointsAgentsAsync().GetAwaiter().GetResult();

			_integrationPointService = Container.Resolve<IIntegrationPointService>();

			var importHelper = new ImportHelper();
			importHelper.ImportData(
				SourceWorkspaceArtifactID,
				DocumentTestDataBuilder.BuildTestData(testDataType: DocumentTestDataBuilder.TestDataType.SmallWithoutFolderStructure));
			var workspaceService = new WorkspaceService(importHelper);
			_savedSearchId = SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactID, "All documents");
			_sourceProductionId = workspaceService
				.CreateAndRunProduction(
					SourceWorkspaceArtifactID,
					_savedSearchId,
					"Production",
					Productions.Services.ProductionType.ImagesAndNatives)
				.ProductionArtifactID;
		}

		public override void TestSetup()
		{
			base.TestSetup();

			_targetWorkspaceArtifactID = Workspace.CreateWorkspaceAsync(_TARGET_WORKSPACE_NAME, WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME).GetAwaiter().GetResult();
			var workspaceService = new WorkspaceService(new ImportHelper());
			_targetProductionId = workspaceService
				.CreateProductionSet(
					_targetWorkspaceArtifactID,
					"Target Production");
		}

		public override void TestTeardown()
		{
			Workspace.DeleteWorkspace(_targetWorkspaceArtifactID);

			base.TestTeardown();
		}

		[IdentifiedTest("d955fd5a-1cb3-4d0f-977a-5b4355f2bbb5")]
		public void ProductionBillingTest_ShouldHaveBilledFilesInDestinationWorkspace_WhenUserPushesProductionToProductionWithCopyFilesTrue()
		{
			// Arrange
			IntegrationPointModel integrationModel = GetRelativityProviderIntegrationPointModel(
				GetSourceConfiguration(SourceConfiguration.ExportType.ProductionSet),
				GetProductionDestinationConfiguration(true),
				"Billing Test - Production push");

			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			// Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPoint.ArtifactID);

			// Assert
			FileBillingFlagValidator documentFlagValidator = new FileBillingFlagValidator(Helper,_targetWorkspaceArtifactID);
			documentFlagValidator.AssertFiles(true);
		}

		[IdentifiedTest("d955fd5a-1cb3-4d0f-977a-5b4355f2bbb6")]
		public void ProductionBillingTest_ShouldNotHaveBilledFilesInDestinationWorkspace_WhenUserPushesProductionToProductionWithCopyFilesFalse()
		{
			// Arrange
			IntegrationPointModel integrationModel = GetRelativityProviderIntegrationPointModel(
				GetSourceConfiguration(SourceConfiguration.ExportType.ProductionSet),
				GetProductionDestinationConfiguration(false),
				"Billing Test - Production push");

			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			// Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPoint.ArtifactID);

			// Assert
			FileBillingFlagValidator documentFlagValidator = new FileBillingFlagValidator(Helper, _targetWorkspaceArtifactID);
			documentFlagValidator.AssertFiles(false);
		}


		#region Helper Methods
		private IntegrationPointModel GetRelativityProviderIntegrationPointModel(string sourceConfiguration, string destinationConfiguration, string name)
		{
			return new IntegrationPointModel
			{
				SourceConfiguration = sourceConfiguration,
				Destination = destinationConfiguration,
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				LogErrors = true,
				SelectedOverwrite = "Append/Overlay",
				Name = name,
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(IntegrationPointTypes.ExportGuid).ArtifactId
			};
		}
		
		private string GetProductionDestinationConfiguration(bool copyFilesToDocumentRepository, ImportOverwriteModeEnum overwriteMode = ImportOverwriteModeEnum.AppendOverlay)
		{
			ImportSettings destinationConfiguration = new ImportSettings()
			{
				ArtifactTypeId = (int)ArtifactType.Document,
				DestinationProviderType = DestinationProviders.RELATIVITY,
				CaseArtifactId = _targetWorkspaceArtifactID,
				Provider = RELATIVITY_PROVIDER_NAME,
				FieldOverlayBehavior = "Use Field Settings",
				ExtractedTextFileEncoding = Encoding.Unicode.EncodingName,
				ImportOverwriteMode = overwriteMode,
				ImportNativeFile = copyFilesToDocumentRepository,
				ImportNativeFileCopyMode = copyFilesToDocumentRepository ? ImportNativeFileCopyModeEnum.CopyFiles : ImportNativeFileCopyModeEnum.DoNotImportNativeFiles,
				ProductionImport = true,
				ProductionArtifactId = _targetProductionId,
				IdentifierField = "Control Number",
				ProductionPrecedence = "0",
				ImageImport = true,
				ImagePrecedence = new List<ProductionDTO>(),
			};

			return Serializer.Serialize(destinationConfiguration);
		}



		private string GetSourceConfiguration(SourceConfiguration.ExportType exportType)
		{
			var sourceConfiguration = new SourceConfiguration
			{
				SourceWorkspaceArtifactId = SourceWorkspaceArtifactID,
				TargetWorkspaceArtifactId = _targetWorkspaceArtifactID,
				TypeOfExport = exportType
			};

			if (exportType == SourceConfiguration.ExportType.SavedSearch)
			{
				sourceConfiguration.SavedSearchArtifactId = SavedSearchArtifactID;
			}
			else if (exportType == SourceConfiguration.ExportType.ProductionSet)
			{
				sourceConfiguration.SourceProductionId = _sourceProductionId;
			}

			return Serializer.Serialize(sourceConfiguration);
		}
	}
	#endregion
}