using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Common
{
    public class RetryHandlerFactory : IRetryHandlerFactory
    {
        private readonly IAPILog _logger;

        public RetryHandlerFactory(IAPILog logger)
        {
            _logger = logger;
        }

        public IRetryHandler Create(ushort maxNumberOfRetries = 3, ushort exponentialWaitTimeBaseInSeconds = 3)
        {
            return new RetryHandler(_logger, maxNumberOfRetries, exponentialWaitTimeBaseInSeconds);
        }
    }
}
