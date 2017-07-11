using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Management.Tasks;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Management.Tests
{
	[TestFixture]
	public class IntegrationPointsManagerTests : TestBase
	{
		private IntegrationPointsManager _instance;
		private IAPILog _logger;
		private IList<IManagementTask> _monitoring;
		private readonly IList<int> _workspaceIds = new List<int> {278930};

		public override void SetUp()
		{
			_logger = Substitute.For<IAPILog>();
			_monitoring = new List<IManagementTask>();

			var applicationRepository = Substitute.For<IApplicationRepository>();
			applicationRepository.GetWorkspaceArtifactIdsWhereApplicationInstalled(Guid.Parse(Constants.IntegrationPoints.APPLICATION_GUID_STRING)).Returns(_workspaceIds);

			_instance = new IntegrationPointsManager(_logger, _monitoring, applicationRepository);
		}

		[Test]
		public void ItShouldRunAllTasksIndependently()
		{
			var monitoringValid1 = Substitute.For<IManagementTask>();
			var monitoringThrowingException = Substitute.For<IManagementTask>();
			var monitoringValid2 = Substitute.For<IManagementTask>();

			_monitoring.Add(monitoringValid1);
			_monitoring.Add(monitoringThrowingException);
			_monitoring.Add(monitoringValid2);

			monitoringThrowingException.When(x => x.Run(_workspaceIds)).Throw<Exception>();

			// ACT
			_instance.Start();

			// ASSERT
			foreach (var monitoring in _monitoring)
			{
				monitoring.Received(1).Run(_workspaceIds);
			}

			_logger.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object>());
		}
	}
}