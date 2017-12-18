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
	public class AlertStuckJobsTaskTests : TestBase
	{
		private AlertStuckJobsTask _instance;
		private IStuckJobs _stuckJobs;
		private IAPM _apm;


		public override void SetUp()
		{
			_stuckJobs = Substitute.For<IStuckJobs>();
			_apm = Substitute.For<IAPM>();

			_instance = new AlertStuckJobsTask(_stuckJobs, _apm);
		}

		[Test]
		public void ItShouldAlertStuckJobs()
		{
			var workspaceArtifactIds = new List<int> {843775, 758413};

			var stuckJobs = new Dictionary<int, IList<JobHistory>>
			{
				{workspaceArtifactIds[0], new List<JobHistory>
					{
						new JobHistory()
						{
							JobID = "1"
						}
					}
				}
			};

			_stuckJobs.FindStuckJobs(workspaceArtifactIds).Returns(stuckJobs);

			// ACT
			_instance.Run(workspaceArtifactIds);

			// ASSERT
			_apm.Received(1).HealthCheckOperation(Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK,
				Arg.Is<Func<HealthCheckOperationResult>>(x => ValidateHealthCheckResult(x(), stuckJobs)));
		}

		private bool ValidateHealthCheckResult(HealthCheckOperationResult healthCheckOperationResult, Dictionary<int, IList<JobHistory>> invalidJobs)
		{
			return !healthCheckOperationResult.IsHealthy
					&& healthCheckOperationResult.CustomData.Keys.SequenceEqual(invalidJobs.Keys.Select(x => $"Workspace {x}"))
					&& healthCheckOperationResult.Message == "Stuck jobs found.";
		}

		[Test]
		public void ItShouldSkipAlertForEmptyResult()
		{
			var workspaceArtifactIds = new List<int> {61343, 923886};

			_stuckJobs.FindStuckJobs(workspaceArtifactIds).Returns(new Dictionary<int, IList<JobHistory>>());

			// ACT
			_instance.Run(workspaceArtifactIds);

			// ASSERT
			_apm.DidNotReceive().HealthCheckOperation(Arg.Any<string>(), Arg.Any<Func<HealthCheckOperationResult>>());
		}
	}
}