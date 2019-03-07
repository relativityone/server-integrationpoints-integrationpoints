using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Management.Tasks;
using kCura.IntegrationPoints.Management.Tasks.Helpers;
using NSubstitute;
using NUnit.Framework;
using Relativity.Telemetry.APM;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Management.Tests.Tasks
{
	[TestFixture]
	public class AlertJobsWithInvalidStatusTaskTests : TestBase
	{
		private AlertJobsWithInvalidStatusTask _instance;
		private IJobsWithInvalidStatus _jobsWithInvalidStatus;
		private IAPM _apm;

		public override void SetUp()
		{
			_jobsWithInvalidStatus = Substitute.For<IJobsWithInvalidStatus>();
			_apm = Substitute.For<IAPM>();

			_instance = new AlertJobsWithInvalidStatusTask(_jobsWithInvalidStatus, _apm);
		}

		[Test]
		public void ItShouldAlertJobsWithInvalidStatus()
		{
			var workspaceArtifactIds = new List<int> {679692, 529573};

			var invalidJobs = new Dictionary<int, IList<JobHistory>>
			{
				{workspaceArtifactIds[0], new List<JobHistory>
					{
						new JobHistory
						{
							JobID = "1"
						}
					}
				}
			};

			_jobsWithInvalidStatus.Find(workspaceArtifactIds).Returns(invalidJobs);

			// ACT
			_instance.Run(workspaceArtifactIds);

			// ASSERT
			// temporary solution to disable invalid status job alert (REL-205785)
			_apm.DidNotReceive().HealthCheckOperation(Arg.Any<string>(), Arg.Any<Func<HealthCheckOperationResult>>());
			// _apm.Received(1).HealthCheckOperation(Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK,
			//	Arg.Is<Func<HealthCheckOperationResult>>(x => ValidateHealthCheckResult(x(), invalidJobs)));
		}

		private bool ValidateHealthCheckResult(HealthCheckOperationResult healthCheckOperationResult, Dictionary<int, IList<JobHistory>> invalidJobs)
		{
			return !healthCheckOperationResult.IsHealthy
					&& healthCheckOperationResult.CustomData.Keys.SequenceEqual(invalidJobs.Keys.Select(x => $"Workspace {x}"))
					&& healthCheckOperationResult.Message == "Jobs with invalid status found.";
		}

		[Test]
		public void ItShouldSkipAlertForEmptyResult()
		{
			var workspaceArtifactIds = new List<int> {61343, 923886};

			_jobsWithInvalidStatus.Find(workspaceArtifactIds).Returns(new Dictionary<int, IList<JobHistory>>());

			// ACT
			_instance.Run(workspaceArtifactIds);

			// ASSERT
			_apm.DidNotReceive().HealthCheckOperation(Arg.Any<string>(), Arg.Any<Func<HealthCheckOperationResult>>());
		}
	}
}