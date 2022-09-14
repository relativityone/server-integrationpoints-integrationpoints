using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class ConfigureDocumentSynchronizationConfiguration : IConfigureDocumentSynchronizationConfiguration
    {
        private readonly IConfiguration _cache;

        public ConfigureDocumentSynchronizationConfiguration(IConfiguration cache)
        {
            _cache = cache;
        }
    }
}
