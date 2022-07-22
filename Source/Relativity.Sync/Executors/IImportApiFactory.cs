using System;
using System.Threading.Tasks;
using kCura.Relativity.ImportAPI;

namespace Relativity.Sync.Executors
{
    internal interface IImportApiFactory
    {
        Task<IImportAPI> CreateImportApiAsync(Uri webServiceUrl);
    }
}