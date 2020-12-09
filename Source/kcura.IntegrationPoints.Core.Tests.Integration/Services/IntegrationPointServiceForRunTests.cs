using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;
using Relativity.Testing.Identification;
using IntegrationPointModel = kCura.IntegrationPoints.Core.Models.IntegrationPointModel;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[Parallelizable(ParallelScope.None)]
	public class IntegrationPointServiceForRunTests : RelativityProviderTemplate
	{
		private IIntegrationPointService _integrationPointService;
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryService _jobHistoryService;
		private ISavedSearchQueryRepository _savedSearchRepository;

		private const int _ADMIN_USER_ID = 9;

		public IntegrationPointServiceForRunTests() : base("IntegrationPointService Source", "IntegrationPointService Destination")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			Container.Resolve<IJobService>();
			_savedSearchRepository = _repositoryFactory.GetSavedSearchQueryRepository(SourceWorkspaceArtifactID);

			DocumentService.DeleteAllDocuments(SourceWorkspaceArtifactID);
			DocumentService.DeleteAllDocuments(TargetWorkspaceArtifactID);
		}

		[TearDown]
		public void TearDown()
		{
			DocumentService.DeleteAllDocuments(SourceWorkspaceArtifactID);
			DocumentService.DeleteAllDocuments(TargetWorkspaceArtifactID);
		}

		[IdentifiedTest("69e67a17-8b23-41a8-b120-9a4441171d16")]
		[SmokeTest]
		[Parallelizable(ParallelScope.None)]
		public void CreateAndRunIntegrationPoint_GoldFlow()
		{
			//Arrange
			Import.ImportNewDocuments(SourceWorkspaceArtifactID, Import.GetImportTable("IPTestDocument", 3));

			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"CreateAndRunIntegrationPoint_GoldFlow {DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			//Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPoint.ArtifactID);
			IntegrationPointModel integrationPointPostJob = _integrationPointService.ReadIntegrationPointModel(integrationPoint.ArtifactID);
			IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(SourceWorkspaceArtifactID);
			IList<int> jobHistoryArtifactIds = new List<int> { jobHistoryRepository.GetLastJobHistoryArtifactId(integrationPointPostJob.ArtifactID) };
			Data.JobHistory jobHistory = _jobHistoryService.GetJobHistory(jobHistoryArtifactIds)[0];

			//Assert
			Assert.AreEqual(false, integrationPointPostJob.HasErrors);
			Assert.IsNotNull(integrationPointPostJob.LastRun);
			Assert.AreEqual(3, jobHistory.ItemsTransferred);
			Assert.AreEqual(0, jobHistory.ItemsWithErrors);
			Assert.AreEqual(JobStatusChoices.JobHistoryCompleted.Name, jobHistory.JobStatus.Name);
			Assert.AreEqual(JobTypeChoices.JobHistoryRun.Name, jobHistory.JobType.Name);
		}

		[IdentifiedTest("0b87716a-5712-41a1-ab79-c98b4a07461a")]
		[Parallelizable(ParallelScope.None)]
		public void RunJobWithFailingValidation_ExpectError_SaveJobHistory()
		{
			// Arrange
			int temporarySavedSearchId = CreateTemporarySavedSearch();
			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.OverlayOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateSerializedSourceConfigWithCustomParameters(TargetWorkspaceArtifactID, temporarySavedSearchId, SourceWorkspaceArtifactID, SourceConfiguration.ExportType.SavedSearch),
				LogErrors = true,
				Name = $"RunJobWithFailingValidation_ExpectError_SaveJobHistory {DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId,
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				}
			};

			IntegrationPointModel integrationPointModel = CreateOrUpdateIntegrationPoint(integrationModel);
			DeleteSavedSearch(SourceWorkspaceArtifactID, temporarySavedSearchId);

			// Act
			Assert.Throws<IntegrationPointValidationException>(() => _integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPointModel.ArtifactID, _ADMIN_USER_ID));

			// Assert
			Data.IntegrationPoint ip = _integrationPointService.ReadIntegrationPoint(integrationPointModel.ArtifactID);
			IList<Data.JobHistory> jobHistory = _jobHistoryService.GetJobHistory(ip.JobHistory);
			Assert.NotNull(ip.JobHistory);
			Assert.AreEqual(JobStatusChoices.JobHistoryValidationFailed.Name, jobHistory[0].JobStatus.Name);
		}

		#region "Helpers"

		private void DeleteSavedSearch(int workspaceArtifactId, int savedSearchId)
		{
			SavedSearch.Delete(workspaceArtifactId, savedSearchId);
			while (_savedSearchRepository.RetrieveSavedSearch(savedSearchId) != null)
			{
			}
		}

		private int CreateTemporarySavedSearch()
		{
			return SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactID, "NewSavedSearch");
		}

		#endregion
	}
}