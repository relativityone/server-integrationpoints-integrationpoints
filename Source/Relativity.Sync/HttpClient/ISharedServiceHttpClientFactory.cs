using System.Threading.Tasks;

namespace Relativity.Sync.HttpClient
{
    internal interface ISharedServiceHttpClientFactory
    {
        Task<System.Net.Http.HttpClient> GetHttpClientAsync();
    }
}
