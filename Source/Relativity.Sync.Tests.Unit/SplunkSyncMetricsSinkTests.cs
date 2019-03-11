using System;
using System.Collections.Generic;
using System.Linq;
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
			_sut.Log(Metric.TimedOperation(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<ExecutionStatus>(), It.IsAny<string>()));

			// assert
			_logger.Verify(x => x.LogInformation(string.Empty, It.IsAny<object[]>()));
		}

		[Test]
		public void ItShouldLogMetricWithValidParameters()
		{
			const string metricName = "metricName";
			TimeSpan duration = TimeSpan.MaxValue;
			ExecutionStatus executionStatus = ExecutionStatus.Completed;
			const string correlationId = "correlationId";
			const string instanceName = "instance name";
			const string callingAssembly = "Calling.Assembly";
			Dictionary<string, object> expectedCustomData = new Dictionary<string, object>
			{
				{ "InstanceName", instanceName },
				{ "CallingAssembly", callingAssembly }
			};
			Dictionary<string, object> expectedDictionary = new Dictionary<string, object>
			{
				{ "Name", metricName },
				{ "Type", MetricType.TimedOperation},
				{ "Value", duration.TotalMilliseconds },
				{ "CorrelationId", correlationId},
				{ "ExecutionStatus", executionStatus },
				{ "CustomData", expectedCustomData },
			};

			_envProp.Setup(x => x.CallingAssembly).Returns(callingAssembly);
			_envProp.Setup(x => x.InstanceName).Returns(instanceName);
			
			// act
			Metric metric = Metric.TimedOperation(metricName, duration, executionStatus, correlationId);
			_sut.Log(metric);

			// assert
			_logger.Verify(x => x.LogInformation(It.IsAny<string>(), It.Is<object[]>(objects => VerifyParameters(objects, expectedDictionary))));
		}

		[Test]
		public void ItDoesNotOverrideExistingCustomData()
		{
			const string metricName = "metricName";
			TimeSpan duration = TimeSpan.MaxValue;
			ExecutionStatus executionStatus = ExecutionStatus.Completed;
			const string correlationId = "correlationId";
			const string instanceNameFromProvider = "instance name";
			const string callingAssemblyFromProvider = "Calling.Assembly";
			const string instanceNameOriginal = "instance name orig";
			const string callingAssemblyOriginal = "Calling.Assembly.Orig";
			Dictionary<string, object> expectedCustomData = new Dictionary<string, object>
			{
				{ "InstanceName", instanceNameOriginal },
				{ "CallingAssembly", callingAssemblyOriginal }
			};
			Dictionary<string, object> expectedDictionary = new Dictionary<string, object>
			{
				{ "Name", metricName },
				{ "Type", MetricType.TimedOperation},
				{ "Value", duration.TotalMilliseconds },
				{ "CorrelationId", correlationId},
				{ "ExecutionStatus", executionStatus },
				{ "CustomData", expectedCustomData },
			};
			_envProp.Setup(x => x.CallingAssembly).Returns(callingAssemblyFromProvider);
			_envProp.Setup(x => x.InstanceName).Returns(instanceNameFromProvider);

			// act
			Metric metric = Metric.TimedOperation(metricName, duration, executionStatus, correlationId);
			metric.CustomData.Add("InstanceName", instanceNameOriginal);
			metric.CustomData.Add("CallingAssembly", callingAssemblyOriginal);
			_sut.Log(metric);

			// assert
			_logger.Verify(x => x.LogInformation(It.IsAny<string>(), It.Is<object[]>(objects => VerifyParameters(objects, expectedDictionary))));
		}

		private static bool VerifyParameters(object[] parameters, Dictionary<string, object> expectedDictionary)
		{
			KeyValuePair<string, object>[] keyValuePairs = parameters.Select(p => (KeyValuePair<string, object>)p).ToArray();
			return
				keyValuePairs.Count(p => p.Key == "Name" && p.Value.Equals(expectedDictionary["Name"])) == 1 &&
				keyValuePairs.Count(p => p.Key == "Type" && p.Value.Equals(expectedDictionary["Type"])) == 1 &&
				keyValuePairs.Count(p => p.Key == "Value" && p.Value.Equals(expectedDictionary["Value"])) == 1 &&
				keyValuePairs.Count(p => p.Key == "CorrelationId" && p.Value.Equals(expectedDictionary["CorrelationId"])) == 1 &&
				keyValuePairs.Count(p => p.Key == "ExecutionStatus" && p.Value.Equals(expectedDictionary["ExecutionStatus"])) == 1 &&
				keyValuePairs.Count(p => p.Key == "CustomData" && AreEqualDictionaries(p.Value, expectedDictionary["CustomData"])) == 1;
		}

		private static bool AreEqualDictionaries(object me, object you)
		{
			if (me is Dictionary<string, object> meAsDict && you is Dictionary<string, object> youAsDict)
			{
				return Enumerable.SequenceEqual(meAsDict, youAsDict);
			}

			return false;
		}
	}
}