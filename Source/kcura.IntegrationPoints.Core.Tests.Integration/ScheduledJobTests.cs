using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;
using Relativity;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Core.Tests.Integration
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class ScheduledJobTests : RelativityProviderTemplate
	{

		private IJobService _jobService;
		private IJobManager _jobManager;
		private long _jobId = 0;

		public ScheduledJobTests() : base("ScheduledJob Source", null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			IntegrationPoint.Tests.Core.Agent.EnableAllIntegrationPointsAgentsAsync().GetAwaiter().GetResult();

			_jobService = Container.Resolve<IJobService>();
			_jobManager = Container.Resolve<IJobManager>();
		}

		public override void TestTeardown()
		{
			_jobManager.DeleteJob(_jobId);
		}

		[IdentifiedTestCase("2BAA15BB-05D8-4B0F-B3F7-97018024D6BB")]
		public void ShouldChangeScheduledJobStopStateForPushingProductionsFlow()
		{
			const int delayInMiliseconds = 100;
			const int maxWaitTimeInSeconds = 180;
			var stopwatch = new Stopwatch();

			try
			{
				//Arrange
				IntegrationPointModel integrationModel = GetRelativityProviderIntegrationPointModel(
					GetSourceConfigurationWithProduction(),
					GetProductionDestinationConfiguration(false),
					"Billing Test - Production push");

				integrationModel.Scheduler = GetScheduler();
				IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

				Job jobInitial = _jobService.GetJobs(integrationPoint.ArtifactID).FirstOrDefault();
				_jobId = jobInitial.JobId;

				Job jobProcessed = _jobService.GetJobs(integrationPoint.ArtifactID).FirstOrDefault();

				stopwatch.Start();
				while (stopwatch.Elapsed.TotalSeconds < maxWaitTimeInSeconds && jobInitial.StopState == jobProcessed.StopState)
				{
					Thread.Sleep(delayInMiliseconds);
					jobProcessed = _jobService.GetJobs(integrationPoint.ArtifactID).FirstOrDefault();
				}

				//Assert
				Assert.AreNotEqual(jobInitial.StopState, jobProcessed.StopState);
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}
		}


		[IdentifiedTestCase("DCD3D6A2-96D8-4F79-BD7A-D0C3049A662B")]
		public void ShouldChangeScheduledJobNextRunTime()
		{
			const int delayInMiliseconds = 500;
			const int maxWaitTimeInSeconds = 180;
			var stopwatch = new Stopwatch();

			//Arrange
			IntegrationPointModel integrationModel = CreateDefaultIntegrationPointModelScheduled(ImportOverwriteModeEnum.AppendOnly, "testing", "Append Only",
				DateTime.UtcNow.AddDays(-1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture), DateTime.UtcNow.AddDays(1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
				ScheduleQueue.Core.ScheduleRules.ScheduleInterval.Daily);
			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);
			Job jobInitial = _jobService.GetJobs(integrationPoint.ArtifactID).FirstOrDefault();
			_jobId = jobInitial.JobId;

			Job jobProcessed = _jobService.GetJobs(integrationPoint.ArtifactID).FirstOrDefault();
			stopwatch.Start();
			while (stopwatch.Elapsed.TotalSeconds < maxWaitTimeInSeconds && jobInitial.NextRunTime == jobProcessed.NextRunTime)
			{
				Thread.Sleep(delayInMiliseconds);
				jobProcessed = _jobService.GetJobs(integrationPoint.ArtifactID).FirstOrDefault();
			}

			//Assert
			Assert.AreNotEqual(jobInitial.NextRunTime, jobProcessed.NextRunTime);
		}

		private Scheduler GetScheduler()
		{
			const int offsetInSeconds = 30;
			return new Scheduler()
			{
				EnableScheduler = true,
				//Date format "MM/dd/yyyy". For testing purpose. No sanity check here
				StartDate = DateTime.UtcNow.AddDays(-1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
				EndDate = DateTime.UtcNow.AddDays(1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
				ScheduledTime = DateTime.UtcNow.AddSeconds(offsetInSeconds).ToString("HH:mm:ss"),
				Reoccur = 0,
				SelectedFrequency = ScheduleQueue.Core.ScheduleRules.ScheduleInterval.Daily.ToString(),
				TimeZoneId = TimeZoneInfo.Utc.Id
			};
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
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};
		}

		private string GetProductionDestinationConfiguration(bool copyFilesToDocumentRepository, ImportOverwriteModeEnum overwriteMode = ImportOverwriteModeEnum.AppendOverlay)
		{
			ImportSettings destinationConfiguration = new ImportSettings()
			{
				ArtifactTypeId = (int)ArtifactType.Document,
				DestinationProviderType = Constants.IntegrationPoints.DestinationProviders.RELATIVITY,
				CaseArtifactId = TargetWorkspaceArtifactID,
				Provider = RelativityProvider.Name,
				FieldOverlayBehavior = "Use Field Settings",
				ExtractedTextFileEncoding = Encoding.Unicode.EncodingName,
				ImportOverwriteMode = overwriteMode,
				ImportNativeFile = copyFilesToDocumentRepository,
				ImportNativeFileCopyMode = copyFilesToDocumentRepository ? ImportNativeFileCopyModeEnum.CopyFiles : ImportNativeFileCopyModeEnum.DoNotImportNativeFiles,
				ProductionImport = true,
				ProductionArtifactId = CreateTargetProductionSet(),
				IdentifierField = "Control Number",
				ProductionPrecedence = "0",
				ImageImport = true,
				ImagePrecedence = new List<ProductionDTO>(),
			};

			return Serializer.Serialize(destinationConfiguration);
		}



		private string GetSourceConfigurationWithProduction()
		{
			var sourceConfiguration = new SourceConfiguration
			{
				SourceWorkspaceArtifactId = SourceWorkspaceArtifactID,
				TargetWorkspaceArtifactId = TargetWorkspaceArtifactID,
				TypeOfExport = SourceConfiguration.ExportType.ProductionSet
			};



			sourceConfiguration.SourceProductionId = CreateTargetProductionSet();

			return Serializer.Serialize(sourceConfiguration);
		}

		private int CreateTargetProductionSet()
		{
			var workspaceService = new WorkspaceService(new ImportHelper());
			int targetProductionId = workspaceService
				.CreateProductionSet(
					TargetWorkspaceArtifactID,
					"Target Production");

			return targetProductionId;
		}
		#endregion
	}
}
