using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry
{
	[TestFixture]
	public class NewRelicSyncMetricsSinkTests
	{
		private Mock<IAPMClient> _apmClientMock;

		private NewRelicSyncMetricsSink _sut;

		private const string _APPLICATION_NAME = "Relativity.Sync";

		[SetUp]
		public void SetUp()
		{
			_apmClientMock = new Mock<IAPMClient>();

			_sut = new NewRelicSyncMetricsSink(_apmClientMock.Object);
		}

		[Test]
		public void Send_ShouldSendMetric_WhenValuesHasBeenFound()
		{
			// Arrange
			IMetric metric = new TestMetric {Value = 1};

			Dictionary<string, object> expectedCustomData = new Dictionary<string, object>
			{
				{"Value", 1}
			};

			// Act
			_sut.Send(metric);

			// Assert
			_apmClientMock.Verify(x => x.Log(_APPLICATION_NAME, expectedCustomData));
		}

		[Test]
		public void Send_ShouldNotSendMetric_WhenMetricIsEmpty()
		{
			// Arrange
			IMetric metric = new TestMetric();

			// Act
			_sut.Send(metric);

			// Assert
			_apmClientMock.VerifyNoOtherCalls();
		}

		internal class TestMetric : MetricBase
		{
			public int? Value { get; set; }
		}
	}
}
