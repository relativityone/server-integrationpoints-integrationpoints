using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Monitoring;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Core.Tests.Monitoring
{
    [TestFixture, Category("Unit")]
    public class HealthCheckTests : TestBase
    {

        private Dictionary<int, IList<JobHistory>> _wkspToJobHistoryRecs;

        private const int _WKSP_ID = 1234;
        private const long _JOB_ID_1 = 1;
        private const long _JOB_ID_2 = 2;

        public override void SetUp()
        {
            _wkspToJobHistoryRecs = new Dictionary<int, IList<JobHistory>>();

            var jobHistory = new List<JobHistory>()
            {
                new JobHistory()
                {
                    JobID = _JOB_ID_1.ToString()
                },
                new JobHistory()
                {
                    JobID = _JOB_ID_2.ToString()
                }
            };

            _wkspToJobHistoryRecs[_WKSP_ID] = jobHistory;
        }

        [Test]
        public void ItShouldCreateStuckJobsMetric()
        {
            // Arrange

            // Act
            HealthCheckOperationResult results = HealthCheck.CreateStuckJobsMetric(_WKSP_ID, _wkspToJobHistoryRecs[_WKSP_ID]);

            // Assert
            string jobsMessagePart = $"{_JOB_ID_1}, {_JOB_ID_2}";
            string message = string.Format(HealthCheck.StuckJobMessage, _WKSP_ID, jobsMessagePart);
            ValidateHealthCheck(results, message);
        }

        [Test]
        public void ItShouldCreateInavlidJobsMetric()
        {
            // Arrange

            // Act
            HealthCheckOperationResult results = HealthCheck.CreateJobsWithInvalidStatusMetric(_wkspToJobHistoryRecs);

            // Assert
            ValidateHealthCheck(results, HealthCheck.InvalidJobMessage);
            
        }

        private void ValidateHealthCheck(HealthCheckOperationResult results, string expectedMsg)
        {
            Assert.That(results.Message, Is.EqualTo(expectedMsg));
            Assert.That(results.CustomData.Count, Is.EqualTo(1));
            Assert.That(results.CustomData.Keys.First(), Is.EqualTo($"Workspace {_WKSP_ID}"));
            Assert.That(results.CustomData.Values.Count, Is.EqualTo(1));
            Assert.That(results.CustomData.Values.First().ToString().Contains(_JOB_ID_1.ToString()));
            Assert.That(results.CustomData.Values.First().ToString().Contains(_JOB_ID_2.ToString()));
        }
    }
}
