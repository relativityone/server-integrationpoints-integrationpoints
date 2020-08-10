using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[Parallelizable(ParallelScope.None)]
	public class IntegrationPointServiceForSchedulerTests : RelativityProviderTemplate
	{
		private IIntegrationPointService _integrationPointService;
		private IJobService _jobService;

		public IntegrationPointServiceForSchedulerTests() : base("IntegrationPointService Scheduler Source",
			"IntegrationPointService Scheduler Destination")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_jobService = Container.Resolve<IJobService>();

			DocumentService.DeleteAllDocuments(SourceWorkspaceArtifactID);
			DocumentService.DeleteAllDocuments(TargetWorkspaceArtifactID);
		}

		[SetUp]
		public void Setup()
		{
			Import.ImportNewDocuments(SourceWorkspaceArtifactID, Import.GetImportTable("IPTestDocument", 3));
		}

		[TearDown]
		public void TearDown()
		{
			DocumentService.DeleteAllDocuments(SourceWorkspaceArtifactID);
			DocumentService.DeleteAllDocuments(TargetWorkspaceArtifactID);
		}

		[IdentifiedTest("7d479f10-b1fc-4a52-a081-18332ed91fc0")]
		[Parallelizable(ParallelScope.None)]
		public void CreateAndRunIntegrationPoint_ScheduledIntegrationPoint_GoldFlow()
		{
			//Arrange
			DateTime utcNow = DateTime.UtcNow;
			const int schedulerRunTimeDelayMinutes = 3;

			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"CreateAndRunIntegrationPoint_ScheduledIntegrationPoint_GoldFlow {DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = true,
					StartDate = utcNow.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
					EndDate = utcNow.AddDays(1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
					ScheduledTime = utcNow.AddMinutes(schedulerRunTimeDelayMinutes).ToString("HH:mm"),
					SelectedFrequency = ScheduleInterval.Daily.ToString(),
					TimeZoneId = TimeZoneInfo.Utc.Id
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			IntegrationPointModel integrationPointPreJobExecution = CreateOrUpdateIntegrationPoint(integrationModel);

			//Act

			//Create Errors by using Append Only
			Status.WaitForScheduledJobToComplete(Container, SourceWorkspaceArtifactID, integrationPointPreJobExecution.ArtifactID, timeoutInSeconds: 600);
			IntegrationPointModel integrationPointPostRun = _integrationPointService.ReadIntegrationPointModel(integrationPointPreJobExecution.ArtifactID);

			//Assert
			Assert.AreEqual(null, integrationPointPreJobExecution.LastRun);
			Assert.AreEqual(false, integrationPointPreJobExecution.HasErrors);
			Assert.AreEqual(false, integrationPointPostRun.HasErrors);
			Assert.IsNotNull(integrationPointPostRun.LastRun);
			Assert.IsNotNull(integrationPointPostRun.NextRun);

			//We need to clean ScheduleAgentQueue table because this Integration Point is scheduled and RIP Application and workspace 
			//will be deleted after test is finished
			CleanScheduleAgentQueueFromAllRipJobs(integrationPointPreJobExecution.ArtifactID);
		}

		private void CleanScheduleAgentQueueFromAllRipJobs(int integrationPointArtifactId)
		{
			IList<Job> jobs = _jobService.GetJobs(integrationPointArtifactId);
			foreach (Job job in jobs)
			{
				_jobService.DeleteJob(job.JobId);
			}
		}
	}
}
