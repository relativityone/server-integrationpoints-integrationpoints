using System;
using System.Collections.Generic;
using System.Linq;
using kCura.ScheduleQueue.Core;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.RelativitySync.Tests
{
	[TestFixture]
	public class SyncMetricsTests
	{
		private Mock<IAPM> _apmMetrics;
		private Mock<IAPILog> _logger;

		private readonly Guid _correlationId = new Guid("057FB3A4-EFE8-4D21-9540-84DE344C78A5");
		private readonly TaskResult _taskResult = new TaskResult {Status = TaskStatusEnum.Success};
		private readonly TimeSpan _duration = TimeSpan.FromSeconds(42);
		private readonly CommandExecutionStatus _executionStatus = CommandExecutionStatus.Completed;

		private SyncMetrics _instance;

		[SetUp]
		public void SetUp()
		{
			_apmMetrics = new Mock<IAPM>();
			_logger = new Mock<IAPILog>();

			_instance = new SyncMetrics(_apmMetrics.Object, _logger.Object);
		}

		[Test]
		public void ItShouldSendProperlyFilledMetric()
		{
			string name1 = "Test1";
			string name2 = "Test2";
			string name3 = "Test3";
			TimeSpan duration1 = TimeSpan.FromSeconds(42);
			TimeSpan duration2 = TimeSpan.FromSeconds(7);
			TimeSpan duration3 = TimeSpan.FromSeconds(13);

			_instance.MarkStartTime();
			_instance.TimedOperation(name1, duration1, _executionStatus);
			_instance.TimedOperation(name2, duration2, _executionStatus);
			_instance.TimedOperation(name3, duration3, _executionStatus);
			_instance.SendMetric(_correlationId, _taskResult);
			
			_apmMetrics.Verify(m => m.TimedOperation(
					SyncMetrics.RELATIVITY_SYNC_APM_METRIC_NAME,
					It.IsAny<double>(),
					It.IsAny<Guid>(),
					It.Is<string>(correlationId => _correlationId == Guid.Parse(correlationId)),
					It.Is<Dictionary<string, object>>(obj => new[]{name1, name2, name3, SyncMetrics.JOB_RESULT_KEY_NAME, SyncMetrics.TOTAL_ELAPSED_TIME_MS, SyncMetrics.ALL_STEPS_ELAPSED_TIME_MS}.All(obj.ContainsKey)),
					It.IsAny<IEnumerable<ISink>>()), 
				Times.Once);
		}

		[Test]
		public void ItShouldThrowArgumentNullExceptionWhenNameIsNull()
		{
			Assert.Throws<ArgumentNullException>(() =>_instance.TimedOperation(null, _duration, _executionStatus));
		}

		[Test]
		public void ItShouldThrowArgumentExceptionWhenNameIsEmpty()
		{
			Assert.Throws<ArgumentException>(() =>_instance.TimedOperation("", _duration, _executionStatus));
		}

		[Test]
		public void ItShouldNotThrowWhenTaskResultIsNull()
		{
			Assert.DoesNotThrow(() => _instance.SendMetric(_correlationId, null));
		}

		[Test]
		public void ItShouldNotThrowWhenApmThrows()
		{
			_apmMetrics.Setup(m => m.TimedOperation(
				It.IsAny<string>(),
				It.IsAny<double>(),
				It.IsAny<Guid>(),
				It.IsAny<string>(),
				It.IsAny<Dictionary<string, object>>(),
				It.IsAny<IEnumerable<ISink>>())).Throws<Exception>();

			Assert.DoesNotThrow(() => _instance.SendMetric(_correlationId, _taskResult));
		}
	}
}
