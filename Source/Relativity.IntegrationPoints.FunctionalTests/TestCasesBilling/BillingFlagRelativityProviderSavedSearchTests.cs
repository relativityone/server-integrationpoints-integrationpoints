using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.Testing.Identification;
using static kCura.IntegrationPoints.Core.Constants.IntegrationPoints;

namespace Relativity.IntegrationPoints.FunctionalTests.TestCasesBilling
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[NotWorkingOnTrident]
	public class BillingFlagRelativityProviderSavedSearchTests : RelativityProviderTemplate
	{
		private IIntegrationPointService _integrationPointService;
		private int _targetWorkspaceArtifactID;

		private const int _ADMIN_USER_ID = 9;
		private const string _TARGET_WORKSPACE_NAME = "IntegrationPoints Billing - Destination";

		public BillingFlagRelativityProviderSavedSearchTests()
			: base(sourceWorkspaceName: "IntegrationPoints Billing - Source",
				   targetWorkspaceName: null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			_integrationPointService = Container.Resolve<IIntegrationPointService>();

			var importHelper = new ImportHelper();
			importHelper.ImportData(
				SourceWorkspaceArtifactID,
				DocumentTestDataBuilder.BuildTestData(testDataType: DocumentTestDataBuilder.TestDataType.SmallWithoutFolderStructure));
		}

		public override void TestSetup()
		{
			base.TestSetup();

			_targetWorkspaceArtifactID = Workspace.CreateWorkspaceAsync(_TARGET_WORKSPACE_NAME, WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME)
				.GetAwaiter().GetResult().ArtifactID;
		}

		public override void TestTeardown()
		{
			Workspace.DeleteWorkspaceAsync(_targetWorkspaceArtifactID).GetAwaiter().GetResult();

			base.TestTeardown();
		}

		[IdentifiedTest("4b57b5dd-3217-4419-9325-5b3bdf657f97")]
		public void SavedSearchBillingTest_ShouldNotHaveBilledFilesInDestinationWorkspace_WhenUserPushesNativesWithLinksOnly()
		{
			// Arrange
			IntegrationPointModel integrationModel = GetRelativityProviderIntegrationPointModel(
				GetSourceConfiguration(SourceConfiguration.ExportType.SavedSearch),
				GetNativesDestinationConfiguration(ImportNativeFileCopyModeEnum.SetFileLinks),
				"Billing Test - Natives Links Only");

			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			// Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPoint.ArtifactID);

			// Assert
			FileBillingFlagValidator documentFlagValidator = new FileBillingFlagValidator(Helper, _targetWorkspaceArtifactID);
			documentFlagValidator.AssertFiles(false);
		}

		[IdentifiedTest("84839955-de26-40d9-8e54-57af24454a31")]
		public void SavedSearchBillingTest_ShouldHaveBilledFilesInDestinationWorkspace_WhenUserPushesNativesWithCopyFiles()
		{
			// Arrange
			IntegrationPointModel integrationModel = GetRelativityProviderIntegrationPointModel(
				GetSourceConfiguration(SourceConfiguration.ExportType.SavedSearch),
				GetNativesDestinationConfiguration(ImportNativeFileCopyModeEnum.CopyFiles),
				"Billing Test - Natives Copy Files");

			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			// Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPoint.ArtifactID);

			// Assert
			FileBillingFlagValidator documentFlagValidator = new FileBillingFlagValidator(Helper, _targetWorkspaceArtifactID);
			documentFlagValidator.AssertFiles(true);
		}

		[IdentifiedTest("996eb28c-979b-4761-b98d-73c541652b7d")]
		public void SavedSearchBillingTest_ShouldNotHaveBilledFilesInDestinationWorkspace_WhenUserPushesNativesWithLinksOnlyAndThenOverlayMetadata()
		{
			//<--- Step 1 --->//

			// Arrange
			IntegrationPointModel integrationModelWithLinksOnly = GetRelativityProviderIntegrationPointModel(
				GetSourceConfiguration(SourceConfiguration.ExportType.SavedSearch),
				GetNativesDestinationConfiguration(ImportNativeFileCopyModeEnum.SetFileLinks),
				"Billing Test - Natives Links Only");

			IntegrationPointModel integrationPointWithLinksOnly = CreateOrUpdateIntegrationPoint(integrationModelWithLinksOnly);

			// Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPointWithLinksOnly.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPointWithLinksOnly.ArtifactID);

			// Assert
			FileBillingFlagValidator documentFlagValidatorStep1 = new FileBillingFlagValidator(Helper, _targetWorkspaceArtifactID);
			documentFlagValidatorStep1.AssertFiles(false);

			//<--- Step 2 --->//

			// Arrange
			IntegrationPointModel integrationModelOverlayMetadata = GetRelativityProviderIntegrationPointModel(
				GetSourceConfiguration(SourceConfiguration.ExportType.SavedSearch),
				GetNativesDestinationConfiguration(ImportNativeFileCopyModeEnum.DoNotImportNativeFiles, ImportOverwriteModeEnum.OverlayOnly),
				"Billing Test - Metadata Only");

			IntegrationPointModel integrationPointOverlayMetadata = CreateOrUpdateIntegrationPoint(integrationModelOverlayMetadata);
			
			// Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPointOverlayMetadata.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPointOverlayMetadata.ArtifactID);

			// Assert
			FileBillingFlagValidator documentFlagValidatorStep2 = new FileBillingFlagValidator(Helper, _targetWorkspaceArtifactID);
			documentFlagValidatorStep2.AssertFiles(false);
		}

		[IdentifiedTest("d4152780-f863-4e49-b342-675d8dd4c9d8")]
		public void SavedSearchBillingTest_ShouldHaveBilledFilesInDestinationWorkspace_WhenUserPushesNativesWithCopyFilesAndThenOverlayMetadata()
		{
			//<--- Step 1 --->//

			// Arrange
			IntegrationPointModel integrationModelWithCopyFiles = GetRelativityProviderIntegrationPointModel(
				GetSourceConfiguration(SourceConfiguration.ExportType.SavedSearch),
				GetNativesDestinationConfiguration(ImportNativeFileCopyModeEnum.CopyFiles),
				"Billing Test - Natives Copy Files");

			IntegrationPointModel integrationPointWithCopyFiles = CreateOrUpdateIntegrationPoint(integrationModelWithCopyFiles);

			// Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPointWithCopyFiles.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPointWithCopyFiles.ArtifactID);

			// Assert
			FileBillingFlagValidator documentFlagValidatorStep1 = new FileBillingFlagValidator(Helper, _targetWorkspaceArtifactID);
			documentFlagValidatorStep1.AssertFiles(true);

			//<--- Step 2 --->//

			// Arrange
			IntegrationPointModel integrationModelOverlayMetadata = GetRelativityProviderIntegrationPointModel(
				GetSourceConfiguration(SourceConfiguration.ExportType.SavedSearch),
				GetNativesDestinationConfiguration(ImportNativeFileCopyModeEnum.DoNotImportNativeFiles, ImportOverwriteModeEnum.OverlayOnly),
				"Billing Test - Metadata Only");

			IntegrationPointModel integrationPointOverlayMetadata = CreateOrUpdateIntegrationPoint(integrationModelOverlayMetadata);

			// Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPointOverlayMetadata.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPointOverlayMetadata.ArtifactID);

			// Assert
			FileBillingFlagValidator documentFlagValidatorStep2 = new FileBillingFlagValidator(Helper, _targetWorkspaceArtifactID);
			documentFlagValidatorStep2.AssertFiles(true);
		}

		[IdentifiedTest("f23afa70-0ad7-46c3-95aa-af4e6b3d3389")]
		public void SavedSearchBillingTest_ShouldNotHaveBilledFilesInDestinationWorkspace_WhenUserPushesNativesWithCopyFilesToDocumentRepositoryAndThenOverlayWithLinksOnly()
		{
			//<--- Step 1 --->//

			// Arrange
			IntegrationPointModel integrationModelWithCopyFiles = GetRelativityProviderIntegrationPointModel(
				GetSourceConfiguration(SourceConfiguration.ExportType.SavedSearch),
				GetNativesDestinationConfiguration(ImportNativeFileCopyModeEnum.CopyFiles, ImportOverwriteModeEnum.AppendOnly),
				"Billing Test - Natives Copy Files");

			IntegrationPointModel integrationPointWithCopyFiles = CreateOrUpdateIntegrationPoint(integrationModelWithCopyFiles);

			// Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPointWithCopyFiles.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPointWithCopyFiles.ArtifactID);

			// Assert
			FileBillingFlagValidator documentFlagValidatorStep1 = new FileBillingFlagValidator(Helper, _targetWorkspaceArtifactID);
			documentFlagValidatorStep1.AssertFiles(true);

			//<--- Step 2 --->//

			// Arrange
			IntegrationPointModel integrationModelOverlayMetadata = GetRelativityProviderIntegrationPointModel(
				GetSourceConfiguration(SourceConfiguration.ExportType.SavedSearch),
				GetNativesDestinationConfiguration(ImportNativeFileCopyModeEnum.SetFileLinks, ImportOverwriteModeEnum.OverlayOnly),
				"Billing Test - Natives Links Only");

			IntegrationPointModel integrationPointOverlayMetadata = CreateOrUpdateIntegrationPoint(integrationModelOverlayMetadata);

			// Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPointOverlayMetadata.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPointOverlayMetadata.ArtifactID);

			// Assert
			FileBillingFlagValidator documentFlagValidatorStep2 = new FileBillingFlagValidator(Helper, _targetWorkspaceArtifactID);
			documentFlagValidatorStep2.AssertFiles(false);
		}

		[IdentifiedTest("8a68f657-74e6-4a1a-91df-b34c5f80f765")]
		public void SavedSearchBillingTest_ShouldHaveBilledFilesInDestinationWorkspace_WhenUserPushesImagesWithCopyFilesToDocumentRepository()
		{
			// Arrange
			IntegrationPointModel integrationModel = GetRelativityProviderIntegrationPointModel(
				GetSourceConfiguration(SourceConfiguration.ExportType.SavedSearch),
				GetImagesDestinationConfiguration(true),
				"Billing Test - Images Copy Files");

			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			// Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPoint.ArtifactID);

			// Assert
			FileBillingFlagValidator documentFlagValidator = new FileBillingFlagValidator(Helper, _targetWorkspaceArtifactID);
			documentFlagValidator.AssertFiles(true);
		}

		[IdentifiedTest("f3ac208e-0047-44df-b25f-58a5acd544bb")]
		public void SavedSearchBillingTest_ShouldNotHaveBilledFilesInDestinationWorkspace_WhenUserPushesImagesWithoutCopyFilesToDocumentRepository()
		{
			// Arrange
			IntegrationPointModel integrationModel = GetRelativityProviderIntegrationPointModel(
				GetSourceConfiguration(SourceConfiguration.ExportType.SavedSearch),
				GetImagesDestinationConfiguration(false),
				"Billing Test - Images Do Not Copy Files");

			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			// Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPoint.ArtifactID);

			// Assert
			FileBillingFlagValidator documentFlagValidator = new FileBillingFlagValidator(Helper, _targetWorkspaceArtifactID);
			documentFlagValidator.AssertFiles(false);
		}

		[IdentifiedTest("317f66e9-8c03-46ee-bc38-9fb3510340e7")]
		public void SavedSearchBillingTest_ShouldNotHaveBilledFilesInDestinationWorkspace_WhenUserPushesImagesWithCopyFilesToDocumentRepositoryAndThenOverlayWithoutCopy()
		{
			//<--- Step 1 --->//

			// Arrange
			IntegrationPointModel integrationModelWithCopyFiles = GetRelativityProviderIntegrationPointModel(
				GetSourceConfiguration(SourceConfiguration.ExportType.SavedSearch),
				GetImagesDestinationConfiguration(true, ImportOverwriteModeEnum.AppendOnly),
				"Billing Test - Images Copy Files");

			IntegrationPointModel integrationPointWithCopyFiles = CreateOrUpdateIntegrationPoint(integrationModelWithCopyFiles);

			// Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPointWithCopyFiles.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPointWithCopyFiles.ArtifactID);

			// Assert
			FileBillingFlagValidator documentFlagValidatorStep1 = new FileBillingFlagValidator(Helper, _targetWorkspaceArtifactID);
			documentFlagValidatorStep1.AssertFiles(true);

			//<--- Step 2 --->//

			// Arrange
			IntegrationPointModel integrationModelOverlayMetadata = GetRelativityProviderIntegrationPointModel(
				GetSourceConfiguration(SourceConfiguration.ExportType.SavedSearch),
				GetImagesDestinationConfiguration(false, ImportOverwriteModeEnum.OverlayOnly),
				"Billing Test - Images Without Copy Files");

			IntegrationPointModel integrationPointOverlayMetadata = CreateOrUpdateIntegrationPoint(integrationModelOverlayMetadata);

			// Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPointOverlayMetadata.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPointOverlayMetadata.ArtifactID);

			// Assert
			FileBillingFlagValidator documentFlagValidatorStep2 = new FileBillingFlagValidator(Helper, _targetWorkspaceArtifactID);
			documentFlagValidatorStep2.AssertFiles(false);
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
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};
		}

		private string GetNativesDestinationConfiguration(ImportNativeFileCopyModeEnum fileCopyMode, ImportOverwriteModeEnum overwriteMode = ImportOverwriteModeEnum.AppendOverlay)
		{
			ImportSettings destinationConfiguration = new ImportSettings()
			{
				ArtifactTypeId = (int)ArtifactType.Document,
				DestinationProviderType = DestinationProviders.RELATIVITY,
				CaseArtifactId = _targetWorkspaceArtifactID,
				DestinationFolderArtifactId = GetRootFolder(Helper, _targetWorkspaceArtifactID),
				Provider = RELATIVITY_PROVIDER_NAME,
				FieldOverlayBehavior = "Use Field Settings",
				ExtractedTextFileEncoding = "UTF-8",
				ImportOverwriteMode = overwriteMode,
				ImportNativeFile = fileCopyMode == ImportNativeFileCopyModeEnum.CopyFiles ? true : false,
				ImportNativeFileCopyMode = fileCopyMode,
			};

			return Serializer.Serialize(destinationConfiguration);
		}

		private string GetImagesDestinationConfiguration(bool copyFilesToDocumentRepository, ImportOverwriteModeEnum overwriteMode = ImportOverwriteModeEnum.AppendOverlay)
		{
			ImportSettings destinationConfiguration = new ImportSettings()
			{
				ArtifactTypeId = (int)ArtifactType.Document,
				DestinationProviderType = DestinationProviders.RELATIVITY,
				CaseArtifactId = _targetWorkspaceArtifactID,
				DestinationFolderArtifactId = GetRootFolder(Helper, _targetWorkspaceArtifactID),
				Provider = RELATIVITY_PROVIDER_NAME,
				FieldOverlayBehavior = "Use Field Settings",
				ExtractedTextFileEncoding = "UTF-8",
				ImageImport = true,
				CopyFilesToDocumentRepository = copyFilesToDocumentRepository,
				ImportNativeFileCopyMode = copyFilesToDocumentRepository ? ImportNativeFileCopyModeEnum.CopyFiles : ImportNativeFileCopyModeEnum.DoNotImportNativeFiles,
				ImportOverwriteMode = overwriteMode,
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
				//To Update
				sourceConfiguration.SourceProductionId = default(int);
			}

			return Serializer.Serialize(sourceConfiguration);
		}
	}
	#endregion
}
