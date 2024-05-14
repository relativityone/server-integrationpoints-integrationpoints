using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.KeplerFactory
{
    internal interface IServiceFactoryForUser
    {
        Task<T> CreateProxyAsync<T>() where T : class, IDisposable;
    }
}
