using Relativity.API;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Transfer.StreamWrappers
{
    internal sealed class RetriableLongTextStreamBuilderFactory : IRetriableStreamBuilderFactory
    {
        private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
        private readonly IStreamRetryPolicyFactory _streamRetryPolicyFactory;
        private readonly ISyncMetrics _syncMetrics;
        private readonly IAPILog _logger;

        public RetriableLongTextStreamBuilderFactory(ISourceServiceFactoryForUser serviceFactoryForUser,
            IStreamRetryPolicyFactory streamRetryPolicyFactory, ISyncMetrics syncMetrics, IAPILog logger)
        {
            _serviceFactoryForUser = serviceFactoryForUser;
            _streamRetryPolicyFactory = streamRetryPolicyFactory;
            _syncMetrics = syncMetrics;
            _logger = logger;
        }

        public IRetriableStreamBuilder Create(int workspaceArtifactId, int relativityObjectArtifactId, string fieldName)
        {
            return new RetriableLongTextStreamBuilder(workspaceArtifactId, relativityObjectArtifactId, fieldName, _serviceFactoryForUser, _streamRetryPolicyFactory, _syncMetrics, _logger);
        }
    }
}
