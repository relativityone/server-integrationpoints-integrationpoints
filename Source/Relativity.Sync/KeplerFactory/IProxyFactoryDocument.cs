using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.KeplerFactory
{
    internal interface IProxyFactoryDocument
    {
        Task<T> CreateProxyDocumentAsync<T>(Identity identity) where T : class, IDisposable;
    }
}
