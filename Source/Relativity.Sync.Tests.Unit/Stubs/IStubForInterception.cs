using System;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.Unit.Stubs
{
    public interface IStubForInterception : IDisposable
    {
        Task<int> ExecuteAndReturnValueAsync();

        Task ExecuteAsync();
    }
}
