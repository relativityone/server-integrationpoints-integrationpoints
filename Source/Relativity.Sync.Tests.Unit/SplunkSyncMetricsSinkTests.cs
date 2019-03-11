using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class SplunkSyncMetricsSinkTests
	{
		private Mock<ISyncLog> _logger;
		private Mock<IEnvironmentPropertyProvider> _envProp;

		private SplunkSyncMetricsSink _sut;

		[SetUp]
		public void SetUp()
		{
			_logger = new Mock<ISyncLog>();
			_envProp = new Mock<IEnvironmentPropertyProvider>();

			_sut = new SplunkSyncMetricsSink(_logger.Object, _envProp.Object);
		}

		[Test]
		public void ItShouldLogMetricWithEmptyMessageTemplate()
		{
			// act
			_sut.Log(Metric.TimedOperation(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CommandExecutionStatus>(), It.IsAny<string>()));

			// assert
			_logger.Verify(x => x.LogInformation(string.Empty, It.IsAny<object[]>()));
		}

		[Test]
		public void ItShouldLogMetricWithValidParameters()
		{
			const string metricName = "metricName";
			TimeSpan duration = TimeSpan.MaxValue;
			CommandExecutionStatus executionStatus = CommandExecutionStatus.Completed;
			const string correlationId = "correlationId";
			const string instanceName = "instance name";
			const string callingAssembly = "Calling.Assembly";
			Dictionary<string, object> metadata = new Dictionary<string, object>();

			Dictionary<string, object> expectedDictionary = new Dictionary<string, object>
			{
				{"Name", metricName},
				{ "Type", MetricType.TimedOperation},
				{ "CorrelationId", correlationId},
				{ "ExecutionStatus", executionStatus },
				{ "Metadata", metadata },
				{ "Value", duration.TotalMilliseconds },
				{ "InstanceName", instanceName },
				{ "CallingAssembly", callingAssembly }
			};

			_envProp.Setup(x => x.CallingAssembly).Returns(callingAssembly);
			_envProp.Setup(x => x.InstanceName).Returns(instanceName);
			
			// act
			Metric metric = Metric.TimedOperation(metricName, duration, executionStatus, correlationId);
			metric.Metadata = metadata;
			_sut.Log(metric);

			// assert
			_logger.Verify(x => x.LogInformation(It.IsAny<string>(), It.Is<object[]>(objects => VerifyParameters(objects, expectedDictionary))));
		}

		private bool VerifyParameters(object[] parameters, Dictionary<string, object> expectedDictionary)
		{
			Dictionary<string, object> dict = parameters[0] as Dictionary<string, object>;

			if (dict == null)
			{
				return false;
			}

			return
				dict["Name"].Equals(expectedDictionary["Name"]) &&
				dict["Type"].Equals(expectedDictionary["Type"]) &&
				dict["CorrelationId"].Equals(expectedDictionary["CorrelationId"]) &&
				dict["ExecutionStatus"].Equals(expectedDictionary["ExecutionStatus"]) &&
				dict["Value"].Equals(expectedDictionary["Value"]) &&
				dict["Metadata"].Equals(expectedDictionary["Metadata"]) &&
				dict["InstanceName"].Equals(expectedDictionary["InstanceName"]) &&
				dict["CallingAssembly"].Equals(expectedDictionary["CallingAssembly"])
				;
		}
	}
}