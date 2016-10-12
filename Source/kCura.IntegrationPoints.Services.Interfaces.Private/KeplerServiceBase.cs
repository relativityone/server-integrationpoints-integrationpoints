using System.Threading.Tasks;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services
{
    public abstract class KeplerServiceBase : IKeplerService
    {
        private readonly ILog _logger;

        public ILog Logger
        {
            get { return _logger; }
        }

        public KeplerServiceBase(ILog logger)
        {
            _logger = logger;
        }

        public async Task<bool> PingAsync()
        {
            return await Task.Run(() => true).ConfigureAwait(false);
        }
    }
}