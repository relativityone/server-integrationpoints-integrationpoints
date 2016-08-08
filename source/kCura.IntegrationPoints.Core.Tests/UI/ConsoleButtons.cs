using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace kCura.IntegrationPoints.Core.Tests.UI
{
	[TestFixture]
	[Category("Integration Tests")]
	public class ConsoleButtons : RelativityProviderTemplate
	{
		private IJobHistoryService _jobHistoryService;
		private IJobService _jobService;
		private IWebDriver _webDriver;
		private IObjectTypeRepository _objectTypeRepository;
		private int _integrationPointArtifactTypeId;

		public ConsoleButtons() : base("IntegrationPointService Source", null)
		{
		}

		public override void SuiteSetup()
		{
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_jobService = Container.Resolve<IJobService>();
			_objectTypeRepository = Container.Resolve<IObjectTypeRepository>();
			_integrationPointArtifactTypeId = _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.IntegrationPoint));
		}

		[TestCase(TestBrowser.Chrome)]
		public void StopButton_SetsStopStateOnClick_Success(TestBrowser browser)
		{
			//Arrange
			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.OverlayOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "IntegrationPointServiceTest" + DateTime.Now,
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap(),
				HasErrors = false
			};

			Guid batchInstance = Guid.NewGuid();
			IntegrationModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);
			JobHistory jobHistory = CreateJobHistoryInPending(integrationPoint.ArtifactID, batchInstance);
			_jobService.CreateJob(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, TaskType.ExportService.ToString(), DateTime.MaxValue, batchInstance.ToString(), 9, null, null);
			_webDriver = Selenium.GetWebDriver(browser);

			//Act
			_webDriver.LogIntoRelativity(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
			_webDriver.GoToObjectInstance(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, _integrationPointArtifactTypeId);

			//Assert
		}

		private JobHistory CreateJobHistoryInPending(int integrationPointArtifactId, Guid batchInstance)
		{
			Data.IntegrationPoint integrationPoint = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointArtifactId);
			JobHistory jobHistory = _jobHistoryService.CreateRdo(integrationPoint, batchInstance, JobTypeChoices.JobHistoryRun, DateTime.Now);
			jobHistory.JobStatus = JobStatusChoices.JobHistoryPending;
			_jobHistoryService.UpdateRdo(jobHistory);
			return jobHistory;
		}
	}
}
