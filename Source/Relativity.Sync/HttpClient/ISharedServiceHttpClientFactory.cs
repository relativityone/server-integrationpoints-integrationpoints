using System.Threading.Tasks;

namespace Relativity.Sync.HttpClient
{
    public interface ISharedServiceHttpClientFactory
    {
        Task<System.Net.Http.HttpClient> GetHttpClientAsync();
    }
}
