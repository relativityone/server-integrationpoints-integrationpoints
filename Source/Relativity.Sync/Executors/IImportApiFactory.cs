using System;
using System.Threading.Tasks;
using kCura.Relativity.ImportAPI;

namespace Relativity.Sync.Executors
{
	internal interface IImportApiFactory
	{
		Task<IImportAPI> CreateImportApiAsync(string userName, string password, Uri webServiceUrl);
	}
}