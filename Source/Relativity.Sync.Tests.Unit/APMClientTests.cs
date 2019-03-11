using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Relativity.Telemetry.APM;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class APMClientTests
	{
		[Test]
		public void ItLogsCorrectData()
		{
			string metricName = "FooBar";
			var metricData = new Dictionary<string, object> { { "Blech", 1 }, { "Blorz", "Blech" } };

			Mock<IAPM> apmMock = new Mock<IAPM>();
			Mock<ICounterMeasure> counterMock = new Mock<ICounterMeasure>();
			apmMock.Setup(a => a.CountOperation(
				It.Is<string>(s => s.Equals(metricName, StringComparison.InvariantCulture)),
				It.IsAny<Guid>(),
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<bool>(),
				It.IsAny<int?>(),
				It.Is<Dictionary<string, object>>(d => Enumerable.SequenceEqual(d, metricData)),
				It.IsAny<IEnumerable<ISink>>())
			).Returns(counterMock.Object).Verifiable();

			// Act
			var client = new APMClient(apmMock.Object);
			client.Log(metricName, metricData);

			// Assert
			apmMock.Verify();
			counterMock.Verify(c => c.Write(), Times.Once);
		}
	}
}
