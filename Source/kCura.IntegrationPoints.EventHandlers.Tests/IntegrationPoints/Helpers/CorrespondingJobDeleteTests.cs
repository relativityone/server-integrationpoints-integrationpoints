using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Contracts.Helpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers
{
    [TestFixture, Category("Unit")]
    public class CorrespondingJobDeleteTests : TestBase
    {
        private IJobService _jobService;
        private CorrespondingJobDelete _correspondingJobDelete;

        public override void SetUp()
        {
            _jobService = Substitute.For<IJobService>();

            _correspondingJobDelete = new CorrespondingJobDelete(_jobService);
        }

        [Test]
        public void ItShouldDeleteAllCorrespondingJobs()
        {
            int workspaceId = 401304;
            int integrationPointId = 768943;

            var taskTypes = TaskTypeHelper.GetManagerTypes().Select(taskType => taskType.ToString()).ToList();

            var jobs = new List<Job>
            {
                new JobBuilder().WithJobId(695).Build(),
                new JobBuilder().WithJobId(315).Build()
            };

            _jobService.GetScheduledJobs(workspaceId, integrationPointId, Arg.Is<List<string>>(x => x.SequenceEqual(taskTypes))).Returns(jobs);

            _correspondingJobDelete.DeleteCorrespondingJob(workspaceId, integrationPointId);

            _jobService.Received(1).GetScheduledJobs(workspaceId, integrationPointId, Arg.Is<List<string>>(x => x.SequenceEqual(taskTypes)));
            foreach (var job in jobs)
            {
                _jobService.Received(1).DeleteJob(job.JobId);
            }
        }
    }
}