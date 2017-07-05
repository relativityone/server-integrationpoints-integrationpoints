using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Management.Monitoring;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Management.Tests
{
	[TestFixture]
	public class IntegrationPointsManagerTests : TestBase
	{
		private IntegrationPointsManager _instance;
		private IAPILog _logger;
		private IList<IMonitoring> _monitoring;

		public override void SetUp()
		{
			_logger = Substitute.For<IAPILog>();
			_monitoring = new List<IMonitoring>();

			_instance = new IntegrationPointsManager(_logger, _monitoring);
		}

		[Test]
		public void ItShouldRunAllMonitoringIndependently()
		{
			var monitoringValid1 = Substitute.For<IMonitoring>();
			var monitoringThrowingException = Substitute.For<IMonitoring>();
			var monitoringValid2 = Substitute.For<IMonitoring>();

			_monitoring.Add(monitoringValid1);
			_monitoring.Add(monitoringThrowingException);
			_monitoring.Add(monitoringValid2);

			monitoringThrowingException.When(x => x.Run()).Throw<Exception>();

			// ACT
			_instance.Execute();

			// ASSERT
			foreach (var monitoring in _monitoring)
			{
				monitoring.Received(1).Run();
			}

			_logger.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object>());
		}
	}
}