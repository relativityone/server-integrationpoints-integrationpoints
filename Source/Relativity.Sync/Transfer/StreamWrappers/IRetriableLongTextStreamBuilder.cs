using System;
using System.IO;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.StreamWrappers
{
    internal interface IRetriableStreamBuilder : IDisposable
    {
        Task<Stream> GetStreamAsync();
    }
}
