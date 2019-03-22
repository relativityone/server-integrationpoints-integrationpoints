using Castle.MicroKernel.Registration;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Agent.Validation;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;
using Relativity.API;
using Workspace = kCura.IntegrationPoint.Tests.Core.Workspace;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	[TestFixture]
	public class RelativityProvider_ImportNativeFileCopyModeTests : RelativityProviderTemplate
	{
		private ExportServiceManager _exportManager;
		private IIntegrationPointService _integrationPointService;
		private IJobService _jobService;
		private Relativity.Client.DTOs.Workspace _sourceWorkspaceDto;
		private const int _ADMIN_USER_ID = 9;
		private const string _SOURCE_WORKSPACE_NAME = "Push_NativeFileCopy";
		private const string _TARGET_WORKSPACE_NAME = "Push_NativeFileCopy_Destination";

		public RelativityProvider_ImportNativeFileCopyModeTests() :
			base(_SOURCE_WORKSPACE_NAME, _TARGET_WORKSPACE_NAME)
		{ }

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			ControlIntegrationPointAgents(false);
		}

		protected override void Install()
		{
			base.Install();
			Container.Register(Component.For<IAgentValidator>().ImplementedBy<AgentValidator>().LifestyleTransient());
		}

		public override void SuiteTeardown()
		{
			ControlIntegrationPointAgents(true);
			base.SuiteTeardown();
		}

		public override void TestSetup()
		{
			IHelperFactory helperFactory = Container.Resolve<IHelperFactory>();
			IContextContainerFactory contextContainerFactory = Container.Resolve<IContextContainerFactory>();
			ISynchronizerFactory synchronizerFactory = Container.Resolve<ISynchronizerFactory>();
			IExporterFactory exporterFactory = Container.Resolve<IExporterFactory>();
			IOnBehalfOfUserClaimsPrincipalFactory onBehalfOfUserClaimsPrincipalFactory = Container.Resolve<IOnBehalfOfUserClaimsPrincipalFactory>();
			IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
			IManagerFactory managerFactory = Container.Resolve<IManagerFactory>();
			ISerializer serializer = Container.Resolve<ISerializer>();
			_jobService = Container.Resolve<IJobService>();
			IScheduleRuleFactory scheduleRuleFactory = new DefaultScheduleRuleFactory();
			IJobHistoryService jobHistoryService = Container.Resolve<IJobHistoryService>();
			IJobHistoryErrorService jobHistoryErrorService = Container.Resolve<IJobHistoryErrorService>();
			JobStatisticsService jobStatisticsService = Container.Resolve<JobStatisticsService>();
			IAgentValidator agentValidator = Container.Resolve<IAgentValidator>();
			IJobStatusUpdater jobStatusUpdater = Container.Resolve<IJobStatusUpdater>();
			IAPILog logger = Container.Resolve<IAPILog>();
			IDateTimeHelper dateTimeHelper = Container.Resolve<IDateTimeHelper>();
			var jobHistoryUpdater = new JobHistoryBatchUpdateStatus(
				jobStatusUpdater, 
				jobHistoryService, 
				_jobService, 
				serializer, 
				logger,
				dateTimeHelper);

			_exportManager = new ExportServiceManager(Helper, helperFactory,
				CaseContext,
				contextContainerFactory,
				synchronizerFactory,
				exporterFactory,
				onBehalfOfUserClaimsPrincipalFactory,
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
				IntegrationPointRepository
			);

			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_sourceWorkspaceDto = Workspace.GetWorkspaceDto(SourceWorkspaceArtifactId);
		}

		[TearDown]
		public void TearDown()
		{
			DocumentService.DeleteAllDocuments(SourceWorkspaceArtifactId);
			DocumentService.DeleteAllDocuments(TargetWorkspaceArtifactId);
			FolderService.DeleteUnusedFolders(SourceWorkspaceArtifactId);
			FolderService.DeleteUnusedFolders(TargetWorkspaceArtifactId);
		}

		[TestCase(ImportNativeFileCopyModeEnum.CopyFiles)]
		[TestCase(ImportNativeFileCopyModeEnum.SetFileLinks)]
		[Ignore("This test fails randomly. Internal Defect was created in Jira: REL-268242")]
		public void NativesShouldBeDeletedInTargetWorkspace_InImportNativesModes(ImportNativeFileCopyModeEnum importNativeFileCopyMode)
		{
			TestNativeFilesImport(false, true, true, importNativeFileCopyMode, false);
		}

		[Test]
		[Category(IntegrationPoint.Tests.Core.Constants.SMOKE_TEST)]
		[Ignore("Test to be fixed, Jira: REL-280718")]
		public void NativesShouldNotBeDeletedInTargetWorkspace_InDoNotImportNativesMode()
		{
			TestNativeFilesImport(false, true, false, ImportNativeFileCopyModeEnum.DoNotImportNativeFiles, true);
		}

		private void TestNativeFilesImport(bool areNativesPresentInSource, bool areNativesPresentInTarget,
			bool importNativeFile, ImportNativeFileCopyModeEnum importNativeFileCopyMode, bool shouldBeNativePresentAfterPushInTarget)
		{
			ImportDocumentsToWorkspace(SourceWorkspaceArtifactId, areNativesPresentInSource);
			ImportDocumentsToWorkspace(TargetWorkspaceArtifactId, areNativesPresentInTarget);

			// arrange
			IntegrationPointModel integrationPointModel = CreateIntegrationPointModel(
				areNativesPresentInSource, 
				areNativesPresentInTarget, 
				importNativeFile, 
				importNativeFileCopyMode);

			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPointModel.ArtifactID, _ADMIN_USER_ID); // run now
			Job job = null;
			try
			{
				job = GetNextJobInScheduleQueue(new[] { _sourceWorkspaceDto.ResourcePoolID.Value }, integrationPointModel.ArtifactID); // pick up job
				// act
				_exportManager.Execute(job); // run the job

				// assert
				VerifyHasNativeForAllDocuments(TargetWorkspaceArtifactId, shouldBeNativePresentAfterPushInTarget);
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
				DestinationProvider = DestinationProvider.ArtifactId,
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
				CreateDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum.OverlayOnly, TargetWorkspaceArtifactId);
			destinationConfig.ImportNativeFileCopyMode = importNativeFileCopyMode;
			destinationConfig.ImportNativeFile = importNativeFile;

			ISerializer serializer = Container.Resolve<ISerializer>();
			string serializedDestinationConfig = serializer.Serialize(destinationConfig);
			return serializedDestinationConfig;
		}

		private void ImportDocumentsToWorkspace(int workspaceId, bool withNatives)
		{
			var workspaceService = new WorkspaceService(new ImportHelper(withNatives));

			DocumentsTestData documentsTestData = DocumentTestDataBuilder.BuildTestData(null, withNatives);
			workspaceService.ImportData(workspaceId, documentsTestData);
		}

		private void VerifyHasNativeForAllDocuments(int workspaceId, bool expectedHasNative)
		{
			string[] documentFields = { TestConstants.FieldNames.HAS_NATIVES };
			foreach (Result<Relativity.Client.DTOs.Document> docResult in DocumentService.GetAllDocuments(workspaceId, documentFields))
			{
				bool hasNative = docResult.Artifact.HasNative.Value;
				Assert.AreEqual(expectedHasNative, hasNative);
			}
		}
	}
}
