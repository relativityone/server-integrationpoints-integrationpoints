using System.Threading.Tasks;
using kCura.WinEDDS.Service.Export;

namespace Relativity.Sync.Transfer
{
    internal interface ISearchManagerFactory
    {
        Task<ISearchManager> CreateSearchManagerAsync();
    }
}