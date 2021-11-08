using Moq;
using Relativity.Telemetry.DataContracts.Shared;
using Relativity.Telemetry.Services.Metrics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class MetricsManagerStub: KeplerStubBase<IMetricsManager>
    {
        public void SetupMetricsManagerStub()
        {
            Mock.Setup(x => x.LogCountAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<long>()))
                .Returns((string bucket, Guid workspaceGuid, long count) =>
                {
                    return Task.CompletedTask;
                });

            Mock.Setup(x => x.LogCountAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns((string bucket, Guid workspaceGuid, string workflowID, long count) =>
                {
                    return Task.CompletedTask;
                });

            Mock.Setup(x => x.LogCountAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<long>()))
                .Returns((string bucket, Guid workspaceGuid, Guid clientDomainGuid, long count) =>
                {
                    return Task.CompletedTask;
                });

            Mock.Setup(x => x.LogCountAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns((string bucket, Guid workspaceGuid, Guid clientDomainGuid, string workflowID, long count) =>
                {
                    return Task.CompletedTask;
                });

            Mock.Setup(x => x.LogGaugeAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<long>()))
                .Returns((string bucket, Guid workspaceGuid, long value) =>
                {
                    return Task.CompletedTask;
                });

            Mock.Setup(x => x.LogGaugeAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns((string bucket, Guid workspaceGuid, string workflowID, long value) =>
                {
                    return Task.CompletedTask;
                });

            Mock.Setup(x => x.LogGaugeAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<long>()))
                .Returns((string bucket, Guid workspaceGuid, Guid clientDomainGuid, long value) =>
                {
                    return Task.CompletedTask;
                });

            Mock.Setup(x => x.LogGaugeAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns((string bucket, Guid workspaceGuid, Guid clientDomainGuid, string workflowID, long value) =>
                {
                    return Task.CompletedTask;
                });

            Mock.Setup(x => x.LogMetricsAsync(It.IsAny<List<MetricRef>>()))
                .Returns((List<MetricRef> metrics) =>
                {
                    return Task.CompletedTask;
                });
        }
    }
}
