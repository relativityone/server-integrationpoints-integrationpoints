using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
    internal interface ILoadFilePathService
    {
        Task<string> GetJobDirectoryPathAsync();

        Task<string> GenerateBatchLoadFileAsync(IBatch batch);

        Task<string> GenerateLongTextFileAsync();

        Task<string> GetLoadFileRelativeLongTextFilePathAsync(string longTextFilePath);
    }
}
