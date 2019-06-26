﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Integration
{
	[TestFixture]
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

		[TestCase]
		public void ShouldChangeScheduledJobStopState()
		{
			const int delayInMiliseconds = 100;
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
			while (stopwatch.Elapsed.TotalSeconds < maxWaitTimeInSeconds && jobInitial.StopState == jobProcessed.StopState)
			{
				Thread.Sleep(delayInMiliseconds);
				jobProcessed = _jobService.GetJobs(integrationPoint.ArtifactID).FirstOrDefault();
			}

			//Assert
			Assert.AreNotEqual(jobInitial.StopState, jobProcessed.StopState);
		}


		[TestCase]
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
	}
}