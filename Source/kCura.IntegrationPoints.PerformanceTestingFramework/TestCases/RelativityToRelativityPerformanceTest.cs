using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data.Factories;
using NUnit.Framework;
using System;
using System.Diagnostics;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.PerformanceTestingFramework
{
	[TestFixture]
	public class RelativityToRelativityPerformanceTest : RelativityProviderTemplate
	{
		private IIntegrationPointService _integrationPointService;
		private const int _ADMIN_USER_ID = 9;

		private readonly string _fieldMappingsJson;
		
		public RelativityToRelativityPerformanceTest() : base(Convert.ToInt32(TestContext.Parameters["SourceWorkspaceArtifactID"]), $"RipPushPerfTest {DateTime.Now:yyyy-MM-dd HH-mm}")
		{
			_fieldMappingsJson = TestContext.Parameters["FieldMappingsJSON"];
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_integrationPointService = Container.Resolve<IIntegrationPointService>();
		}

		/// <summary>
		/// This very test is meant to be run via NUnit-Console runner version 3+. It is expecting a number of parameters, which are:
		/// 1) SourceWorkspaceArtifactID,
		/// 2) FieldMappingsJSON,
		/// 3) ImportNativeFileCopyModeEnum - DoNotImportNativeFiles, SetFileLinks, CopyFiles
		/// 4) ImageImport - True, False
		/// 5) ImportNativeFile - True, False
		/// </summary>
		[Category("PerformanceTest")]
		[Test]
		public void PerformanceTest()
		{
			//Arrange
			IntegrationPointModel integrationPointModel = PrepareIntegrationPointsModel(TargetWorkspaceArtifactId);
			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationPointModel);
			var testDurationStopWatch = new Stopwatch();

			//Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointToLeavePendingState(Container, SourceWorkspaceArtifactId, integrationPoint.ArtifactID);

			//start the timer only after the job is picked up by an agent
			testDurationStopWatch.Start();
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, integrationPoint.ArtifactID);
			testDurationStopWatch.Stop();

			//it is yet to be decided on how to return the time it took for the job to finish back to the Performance Testing Framework.
			Console.WriteLine($"PerformanceTest - RIP job duration -> {Math.Round(testDurationStopWatch.Elapsed.TotalSeconds, 2)}s");
		}

		private IntegrationPointModel PrepareIntegrationPointsModel(int targetWorkspaceId)
		{
			return new IntegrationPointModel
			{
				Map = _fieldMappingsJson,
				SourceConfiguration = SourceConfiguration(targetWorkspaceId, SavedSearchArtifactId),
				Destination = DestinationConfiguration(targetWorkspaceId),

				SourceProvider = RelativityProvider.ArtifactId,
				DestinationProvider = RelativityDestinationProviderArtifactId,
				LogErrors = true,
				Name = $"JobHistoryErrors{DateTime.Now:yyyy-MM-dd HH-mm}",
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				HasErrors = false,
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};
		}

		private string DestinationConfiguration(int targetWorkspaceId)
		{
			ImportNativeFileCopyModeEnum importNativeFileCopyMode;
			Enum.TryParse(TestContext.Parameters["ImportNativeFileCopyModeEnum"], out importNativeFileCopyMode);

			bool imageImport;
			bool.TryParse(TestContext.Parameters["ImageImport"], out imageImport);

			bool importNativeFile;
			bool.TryParse(TestContext.Parameters["ImportNativeFile"], out importNativeFile);

			var destinationConfiguration = new ImportSettings()
			{
				ArtifactTypeId = 10,
				DestinationProviderType = "74A863B9-00EC-4BB7-9B3E-1E22323010C6",
				CaseArtifactId = targetWorkspaceId,
				FederatedInstanceArtifactId = null,
				CreateSavedSearchForTagging = false,
				DestinationFolderArtifactId = 1003697,
				ProductionImport = false,
				Provider = "Relativity",
				ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
				UseDynamicFolderPath = false,
				ImagePrecedence = new Domain.Models.ProductionDTO[0],
				ProductionPrecedence = null,
				IncludeOriginalImages = false,
				IdentifierField = "Control Number",
				MoveExistingDocuments = false,
				ExtractedTextFieldContainsFilePath = false,
				ExtractedTextFileEncoding = "utf-16",
				EntityManagerFieldContainsLink = true,
				FieldOverlayBehavior = "Use Field Settings",
				ImportNativeFile = importNativeFile,
				ImportNativeFileCopyMode = importNativeFileCopyMode,
				ImageImport = imageImport
			};

			var serializer = new JSONSerializer();
			return serializer.Serialize(destinationConfiguration);

		}

		private string SourceConfiguration(int targetWorkspaceId, int savedSearchArtifactId)
		{
			int sourceWorkspaceId;
			int.TryParse(TestContext.Parameters["SourceWorkspaceArtifactID"], out sourceWorkspaceId);

			var sourceConfiguration = new SourceConfiguration()
			{
				SourceWorkspaceArtifactId = sourceWorkspaceId,
				TargetWorkspaceArtifactId = targetWorkspaceId,
				SavedSearchArtifactId = savedSearchArtifactId,
				FederatedInstanceArtifactId = null,
				TypeOfExport = Core.Contracts.Configuration.SourceConfiguration.ExportType.SavedSearch
			};

			var serializer = new JSONSerializer();
			return serializer.Serialize(sourceConfiguration);
		}
	}
}