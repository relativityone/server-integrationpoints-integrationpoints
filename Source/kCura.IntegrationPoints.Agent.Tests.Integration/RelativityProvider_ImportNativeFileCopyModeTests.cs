using System.Linq;
using Castle.MicroKernel.Registration;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Agent.Validation;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.ResourcePool;
using Relativity.Testing.Identification;
using Workspace = kCura.IntegrationPoint.Tests.Core.Workspace;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class RelativityProvider_ImportNativeFileCopyModeTests : RelativityProviderTemplate
	{
		private ExportServiceManager _exportManager;
		private IIntegrationPointService _integrationPointService;
		private IJobService _jobService;
		private ResourcePool _workspaceResourcePool;
		private const int _ADMIN_USER_ID = 9;
		private const string _SOURCE_WORKSPACE_NAME = "Push_NativeFileCopy";
		private const string _TARGET_WORKSPACE_NAME = "Push_NativeFileCopy_Destination";

		public RelativityProvider_ImportNativeFileCopyModeTests() :
			base(_SOURCE_WORKSPACE_NAME, _TARGET_WORKSPACE_NAME)
		{ }

		protected override void InitializeIocContainer()
		{
			base.InitializeIocContainer();
			Container.Register(Component.For<IAgentValidator>().ImplementedBy<AgentValidator>().LifestyleTransient());
		}

		public override void TestSetup()
		{
			ISynchronizerFactory synchronizerFactory = Container.Resolve<ISynchronizerFactory>();
			IExporterFactory exporterFactory = Container.Resolve<IExporterFactory>();
			IExportServiceObserversFactory exportServiceObserversFactory = Container.Resolve<IExportServiceObserversFactory>();
			IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
			IManagerFactory managerFactory = Container.Resolve<IManagerFactory>();
			ISerializer serializer = Container.Resolve<ISerializer>();
			_jobService = Container.Resolve<IJobService>();
			IScheduleRuleFactory scheduleRuleFactory = new DefaultScheduleRuleFactory();
			IJobHistoryService jobHistoryService = Container.Resolve<IJobHistoryService>();
			IJobHistoryErrorService jobHistoryErrorService = Container.Resolve<IJobHistoryErrorService>();
			IJobStatisticsService jobStatisticsService = Container.Resolve<IJobStatisticsService>();
			IAgentValidator agentValidator = Container.Resolve<IAgentValidator>();
			IJobStatusUpdater jobStatusUpdater = Container.Resolve<IJobStatusUpdater>();
			IAPILog logger = Container.Resolve<IAPILog>();
			IDateTimeHelper dateTimeHelper = Container.Resolve<IDateTimeHelper>();
			IDocumentRepository documentRepository = Container.Resolve<IDocumentRepository>();
			IExportDataSanitizer exportDataSanitizer = Container.Resolve<IExportDataSanitizer>();

			var jobHistoryUpdater = new JobHistoryBatchUpdateStatus(
				jobStatusUpdater,
				jobHistoryService,
				_jobService,
				serializer,
				logger,
				dateTimeHelper);

			_exportManager = new ExportServiceManager(Helper,
				CaseContext,
				synchronizerFactory,
				exporterFactory,
				exportServiceObserversFactory,
				repositoryFactory,
				managerFactory,
				new[] { jobHistoryUpdater },
				serializer,
				_jobService,
				scheduleRuleFactory,
				jobHistoryService,
				jobHistoryErrorService,
				jobStatisticsService,
				null,
				agentValidator,
				IntegrationPointRepository,
				documentRepository,
				exportDataSanitizer
			);

			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_workspaceResourcePool = Workspace.GetWorkspaceResourcePoolAsync(SourceWorkspaceArtifactID)
				.GetAwaiter().GetResult();
		}

		[TearDown]
		public void TearDown()
		{
			DocumentService.DeleteAllDocuments(SourceWorkspaceArtifactID);
			DocumentService.DeleteAllDocuments(TargetWorkspaceArtifactID);
			FolderService.DeleteUnusedFolders(SourceWorkspaceArtifactID);
			FolderService.DeleteUnusedFolders(TargetWorkspaceArtifactID);
		}

		[IdentifiedTestCase("7256fb90-5742-4458-978d-94349eb287ef", ImportNativeFileCopyModeEnum.CopyFiles)]
		[IdentifiedTestCase("1f31ef1c-9917-4712-98c4-4a97a067169d", ImportNativeFileCopyModeEnum.SetFileLinks)]
		public void NativesShouldBeDeletedInTargetWorkspace_InImportNativesModes(ImportNativeFileCopyModeEnum importNativeFileCopyMode)
		{
			TestNativeFilesImport(false, true, true, importNativeFileCopyMode, false);
		}

		[IdentifiedTest("8a5590f2-31e8-4ec9-9ddc-a4e3b7b9621e")]
		[SmokeTest]
		public void NativesShouldNotBeDeletedInTargetWorkspace_InDoNotImportNativesMode()
		{
			TestNativeFilesImport(false, true, false, ImportNativeFileCopyModeEnum.DoNotImportNativeFiles, true);
		}

		private void TestNativeFilesImport(bool areNativesPresentInSource, bool areNativesPresentInTarget,
			bool importNativeFile, ImportNativeFileCopyModeEnum importNativeFileCopyMode, bool shouldBeNativePresentAfterPushInTarget)
		{
			ImportDocumentsToWorkspace(SourceWorkspaceArtifactID, areNativesPresentInSource);
			ImportDocumentsToWorkspace(TargetWorkspaceArtifactID, areNativesPresentInTarget);

			// arrange
			IntegrationPointModel integrationPointModel = CreateIntegrationPointModel(
				areNativesPresentInSource,
				areNativesPresentInTarget,
				importNativeFile,
				importNativeFileCopyMode);
			
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPointModel.ArtifactID, _ADMIN_USER_ID); // add job to schedule queue

			Job job = null;
			try
			{
				int[] resourcePools = { _workspaceResourcePool.ArtifactID };
				job = GetNextJobInScheduleQueue(resourcePools, integrationPointModel.ArtifactID, SourceWorkspaceArtifactID);

				// act
				_exportManager.Execute(job); // run the job

				// assert
				VerifyHasNativeForAllDocuments(TargetWorkspaceArtifactID, shouldBeNativePresentAfterPushInTarget);
			}
			finally
			{
				if (job != null)
				{
					_jobService.DeleteJob(job.JobId);
				}
			}
		}

		private IntegrationPointModel CreateIntegrationPointModel(bool areNativesPresentInSource,
			bool areNativesPresentInTarget,
			bool importNativeFile,
			ImportNativeFileCopyModeEnum importNativeFileCopyMode)
		{
			string serializedDestinationConfig = CreateDestinationConfig(importNativeFile, importNativeFileCopyMode);
			var integrationPointModel = new IntegrationPointModel
			{
				SourceProvider = RelativityProvider.ArtifactId,
				Name = $"PUSH - sourceNatives {areNativesPresentInSource} - targetNatives {areNativesPresentInTarget}",
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				Destination = serializedDestinationConfig,
				Map = CreateDefaultFieldMap(),
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				SelectedOverwrite = "Overlay Only",
				Type = Container.Resolve<IIntegrationPointTypeService>()
					.GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid)
					.ArtifactId
			};
			integrationPointModel = CreateOrUpdateIntegrationPoint(integrationPointModel); // create integration point
			return integrationPointModel;
		}

		private string CreateDestinationConfig(bool importNativeFile, ImportNativeFileCopyModeEnum importNativeFileCopyMode)
		{
			ImportSettings destinationConfig =
				CreateDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum.OverlayOnly, TargetWorkspaceArtifactID);
			destinationConfig.ImportNativeFileCopyMode = importNativeFileCopyMode;
			destinationConfig.ImportNativeFile = importNativeFile;

			ISerializer serializer = Container.Resolve<ISerializer>();
			string serializedDestinationConfig = serializer.Serialize(destinationConfig);
			return serializedDestinationConfig;
		}

		private static void ImportDocumentsToWorkspace(int workspaceId, bool withNatives)
		{
			var workspaceService = new WorkspaceService(new ImportHelper(withNatives));

			DocumentsTestData documentsTestData = DocumentTestDataBuilder.BuildTestData(withNatives: withNatives);
			workspaceService.ImportData(workspaceId, documentsTestData);
		}

		private static void VerifyHasNativeForAllDocuments(int workspaceID, bool expectedHasNative)
		{
			string[] documentFields = { TestConstants.FieldNames.HAS_NATIVES };

			bool allHasNativeAsExpected = DocumentService
				.GetAllDocuments(workspaceID, documentFields)
				.Select(document => document.HasNatives)
				.All(hasNative => hasNative == expectedHasNative);

			Assert.IsTrue(allHasNativeAsExpected);
		}
	}
}
