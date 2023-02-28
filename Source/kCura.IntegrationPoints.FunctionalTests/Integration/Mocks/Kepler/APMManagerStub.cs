using Moq;
using Relativity.Telemetry.Services.Interface;
using Relativity.Telemetry.Services.Metrics;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class APMManagerStub : KeplerStubBase<IAPMManager>
    {
        public void SetupAPMManagerStub()
        {
            Mock.Setup(x => x.LogCountAsync(It.IsAny<APMMetric>(), It.IsAny<long>()))
                .Returns((APMMetric metric, long count) =>
                {
                    return Task.CompletedTask;
                });

            Mock.Setup(x => x.LogTimerAsync(It.IsAny<APMMetric>(), It.IsAny<double>()))
                .Returns((APMMetric metric, double milliseconds) =>
                {
                    return Task.CompletedTask;
                });
        }
    }
}
