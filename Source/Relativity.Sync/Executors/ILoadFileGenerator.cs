using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
    internal interface ILoadFileGenerator
    {
        Task<ILoadFile> GenerateAsync(IBatch batch);
    }
}
