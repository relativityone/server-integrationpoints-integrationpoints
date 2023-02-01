using System;
using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
    internal interface ILoadFilePathService
    {
        Task<string> GetJobDirectoryPathAsync();

        Task<string> GenerateBatchLoadFilePathAsync(IBatch batch);

        Task<string> GenerateLongTextFilePathAsync(Guid longTextId);

        Task<string> GetLoadFileRelativeLongTextFilePathAsync(string longTextFilePath);
    }
}
