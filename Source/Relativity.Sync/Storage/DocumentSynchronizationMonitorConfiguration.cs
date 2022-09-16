using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class DocumentSynchronizationMonitorConfiguration : IDocumentSynchronizationMonitorConfiguration
    {
        private readonly IConfiguration _cache;

        public DocumentSynchronizationMonitorConfiguration(IConfiguration cache)
        {
            _cache = cache;
        }
    }
}
