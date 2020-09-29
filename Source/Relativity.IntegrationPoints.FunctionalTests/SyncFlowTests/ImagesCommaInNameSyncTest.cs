using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.IntegrationPoints.FunctionalTests.SyncFlowTests
{
	public class ImagesCommaInNameSyncTest : RelativityProviderTemplate
	{
		private IIntegrationPointService _integrationPointService;
		private IHelper _helper;

		private const int _ADMIN_USER_ID = 9;

		public ImagesCommaInNameSyncTest()
			: base(sourceWorkspaceName: "Image Comma Workspace - Source",
				   targetWorkspaceName: "Image Comma Workspace - Destination")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_helper = Container.Resolve<IHelper>();

			var importHelper = new ImportHelper();
			importHelper.ImportData(
				SourceWorkspaceArtifactID,
				DocumentTestDataBuilder.BuildTestData(testDataType: DocumentTestDataBuilder.TestDataType.TextWithoutFolderStructure));
		}

		[Test]
		public void SyncImages_ShouldRunSuccessfully_WhenCommaIsInControlNumber()
		{
			// Arrange
			AddPrefixToDocumentControlNumber("pre,");

			IntegrationPointModel integrationModel = GetRelativityProviderIntegrationPointModel(
				GetSavedSearchSourceConfiguration(),
				GetImagesDestinationConfiguration(true),
				"Images Sync - Comma in Control Number");

			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			// Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPoint.ArtifactID);

			//Assert
			IntegrationPointModel integrationPointPostJob = _integrationPointService.ReadIntegrationPointModel(integrationPoint.ArtifactID);
			integrationPointPostJob.HasErrors.Should().BeFalse("Integration Point should not have errors after run");
			integrationPointPostJob.LastRun.Should().NotBeNull("Integration Point should have last run date");
		}

		#region Helper Methods

		private void AddPrefixToDocumentControlNumber(string prefix)
		{
			try
			{
				_helper.GetDBContext(SourceWorkspaceArtifactID)
					.ExecuteNonQuerySQLStatement($"UPDATE [Document] SET ControlNumber = CONCAT('{prefix}', ControlNumber)");
			}
			catch(Exception ex)
			{
				throw new InvalidOperationException("Failed to prepend prefix to ControlNumber in all documents", ex);
			}
		}

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

		private string GetImagesDestinationConfiguration(bool copyFilesToDocumentRepository, ImportOverwriteModeEnum overwriteMode = ImportOverwriteModeEnum.AppendOverlay)
		{
			ImportSettings destinationConfiguration = new ImportSettings()
			{
				ArtifactTypeId = (int)kCura.Relativity.Client.ArtifactType.Document,
				DestinationProviderType = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY,
				CaseArtifactId = TargetWorkspaceArtifactID,
				DestinationFolderArtifactId = GetRootFolder(Helper, TargetWorkspaceArtifactID),
				Provider = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME,
				FieldOverlayBehavior = "Use Field Settings",
				ExtractedTextFileEncoding = "UTF-8",
				ImageImport = true,
				CopyFilesToDocumentRepository = copyFilesToDocumentRepository,
				ImportNativeFileCopyMode = copyFilesToDocumentRepository ? ImportNativeFileCopyModeEnum.CopyFiles : ImportNativeFileCopyModeEnum.DoNotImportNativeFiles,
				ImportOverwriteMode = overwriteMode,
			};

			return Serializer.Serialize(destinationConfiguration);
		}

		private string GetSavedSearchSourceConfiguration()
		{
			var sourceConfiguration = new SourceConfiguration
			{
				SourceWorkspaceArtifactId = SourceWorkspaceArtifactID,
				TargetWorkspaceArtifactId = TargetWorkspaceArtifactID,
				TypeOfExport = SourceConfiguration.ExportType.SavedSearch,
				SavedSearchArtifactId = SavedSearchArtifactID
			};

			return Serializer.Serialize(sourceConfiguration);
		}
		
		#endregion
	}
}
