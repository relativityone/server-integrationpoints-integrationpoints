using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.KeplerFactory
{
    internal class ProxyFactoryDocument : IProxyFactoryDocument
    {
        private readonly IServiceFactoryForAdmin _serviceFactoryForAdmin;
        private readonly IServiceFactoryForUser _serviceFactoryForUser;

        public ProxyFactoryDocument(
            IServiceFactoryForAdmin serviceFactoryForAdmin,
            IServiceFactoryForUser serviceFactoryForUser)
        {
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
            _serviceFactoryForUser = serviceFactoryForUser;
        }

        public async Task<T> CreateProxyDocumentAsync<T>(Identity identity)
            where T : class, IDisposable
        {
            return identity == Identity.CurrentUser
                ? await _serviceFactoryForUser.CreateProxyAsync<T>().ConfigureAwait(false)
                : await _serviceFactoryForAdmin.CreateProxyAsync<T>().ConfigureAwait(false);
        }
    }
}
