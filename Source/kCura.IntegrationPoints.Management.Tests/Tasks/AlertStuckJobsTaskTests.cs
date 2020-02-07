﻿using System;
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
	[TestFixture, Category("Unit")]
	public class AlertStuckJobsTaskTests : TestBase
	{
		private const string _JOB_ID_1 = "1";
		private const int _WORKSPACE_ID_1 = 843775;
		private const int _WORKSPACE_ID_2 = 758413;

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
			var workspaceArtifactIds = new List<int> { _WORKSPACE_ID_1, _WORKSPACE_ID_2 };

			var stuckJobs = new Dictionary<int, IList<JobHistory>>
			{
				{workspaceArtifactIds[0], new List<JobHistory>
					{
						new JobHistory
						{
							JobID = _JOB_ID_1
						}
					}
				}
			};

			_stuckJobs.FindStuckJobs(workspaceArtifactIds).Returns(stuckJobs);

			// ACT
			_instance.Run(workspaceArtifactIds);

			// ASSERT
			_apm.Received(1).HealthCheckOperation(Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK,
				Arg.Is<Func<HealthCheckOperationResult>>(x => ValidateHealthCheckResult(x(), workspaceArtifactIds[0])));
		}

		private bool ValidateHealthCheckResult(HealthCheckOperationResult healthCheckOperationResult, int workspaceId)
		{
			bool notHealthy = !healthCheckOperationResult.IsHealthy;
			bool sequenceMatching = healthCheckOperationResult.CustomData.Keys.SequenceEqual(new List<string> { $"Workspace {workspaceId}" });
			bool messageMatching = healthCheckOperationResult.Message == $"Integration Points Job Id {_JOB_ID_1} stuck in Workspace {_WORKSPACE_ID_1}.";

			return notHealthy && sequenceMatching && messageMatching;
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