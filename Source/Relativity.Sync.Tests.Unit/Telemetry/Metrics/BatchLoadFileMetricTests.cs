using System.Collections.Generic;
using AutoFixture;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
    internal class BatchLoadFileMetricTests : MetricTestsBase<BatchLoadFileMetric>
    {
        private BatchLoadFileMetric _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = _fxt.Create<BatchLoadFileMetric>();
        }

        protected override IMetric ArrangeTestMetric()
        {
            return _sut;
        }

        protected override IMetric EmptyTestMetric()
        {
            return new BatchLoadFileMetric();
        }

        protected override void VerifyApmSink(Mock<IAPMClient> apmMock)
        {
            apmMock.Verify(x => x.Count(_APPLICATION_NAME, It.Is<Dictionary<string, object>>(d =>
                d["Status"].Equals(_sut.Status) &&
                d["TotalRecordsRead"].Equals(_sut.TotalRecordsRead) &&
                d["TotalRecordsReadFailed"].Equals(_sut.TotalRecordsReadFailed) &&
                d["ReadMetadataBytesSize"].Equals(_sut.ReadMetadataBytesSize) &&
                d["WriteLoadFileDuration"].Equals(_sut.WriteLoadFileDuration))));
        }

        protected override void VerifySumSink(Mock<IMetricsManager> metricsManagerMock)
        {
            Assert.Pass();
        }
    }
}
