using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Integration
{
	[TestFixture]
	public class ScheduledJobTests : RelativityProviderTemplate
	{
		
		private IJobService _jobService;
		private IObjectTypeRepository _objectTypeRepository;
		private IJobManager _jobManager;
		private long _jobId = 0; 

		public ScheduledJobTests() : base("ScheduledJob Source", null)
		{
		}

	    public override void SuiteSetup()
	    {
	        base.SuiteSetup();
	        _jobService = Container.Resolve<IJobService>();
	        _objectTypeRepository = Container.Resolve<IObjectTypeRepository>();
	        _jobManager = Container.Resolve<IJobManager>();
	    }

	    public override void TestTeardown()
		{
            _jobManager.DeleteJob(_jobId);
		}

		[TestCase]
		public void ShouldChangeScheduledJobStopState()
		{
		    const int second = 1000;
		    const int maxWaitTimeInSeconds = 180;
		    var stopwatch = new Stopwatch();
            
            //Arrange
            IntegrationPointModel integrationModel = CreateDefaultIntegrationPointModelScheduled(ImportOverwriteModeEnum.AppendOnly, "testing", "Append Only", DateTime.UtcNow.AddDays(-1).ToShortDateString(), DateTime.UtcNow.AddDays(1).ToShortDateString(), kCura.ScheduleQueue.Core.ScheduleRules.ScheduleInterval.Daily);
			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);
		    Job jobInitial = _jobService.GetJobs(integrationPoint.ArtifactID).FirstOrDefault();
		    _jobId = jobInitial.JobId;

		    Job jobProcessed = _jobService.GetJobs(integrationPoint.ArtifactID).FirstOrDefault();

		    stopwatch.Start(); 
            while (stopwatch.Elapsed.TotalSeconds < maxWaitTimeInSeconds && jobInitial.StopState == jobProcessed.StopState)
            {
			    Thread.Sleep(second);
                jobProcessed = _jobService.GetJobs(integrationPoint.ArtifactID).FirstOrDefault();
            }

            //Assert
            Assert.AreNotEqual(jobInitial.StopState, jobProcessed.StopState);
		}


	    [TestCase]
	    public void ShouldChangeScheduledJobNextRunTime()
	    {
	        const int second = 1000;
	        const int maxWaitTimeInSeconds = 180;
	        var stopwatch = new Stopwatch();

            //Arrange
            IntegrationPointModel integrationModel = CreateDefaultIntegrationPointModelScheduled(ImportOverwriteModeEnum.AppendOnly, "testing", "Append Only", DateTime.UtcNow.AddDays(-1).ToShortDateString(), DateTime.UtcNow.AddDays(1).ToShortDateString(), kCura.ScheduleQueue.Core.ScheduleRules.ScheduleInterval.Daily);
	        IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);
	        Job jobInitial = _jobService.GetJobs(integrationPoint.ArtifactID).FirstOrDefault();
	        _jobId = jobInitial.JobId;

	        Job jobProcessed = _jobService.GetJobs(integrationPoint.ArtifactID).FirstOrDefault();
	        stopwatch.Start();
            while (stopwatch.Elapsed.TotalSeconds < maxWaitTimeInSeconds && jobInitial.NextRunTime == jobProcessed.NextRunTime)
	        {
	            Thread.Sleep(second);
	            jobProcessed = _jobService.GetJobs(integrationPoint.ArtifactID).FirstOrDefault();
	        }

            //Assert
	        Assert.AreNotEqual(jobInitial.NextRunTime, jobProcessed.NextRunTime);
        }
    }
}