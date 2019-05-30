using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using kCura.Relativity.ImportAPI;

namespace Relativity.Sync.Executors
{
	// We need to exclude this from code coverage, because we are using here concrete class from IAPI.
	[ExcludeFromCodeCoverage]
	internal sealed class ImportApiFactory : IImportApiFactory
	{
		public async Task<IImportAPI> CreateImportApiAsync(Uri webServiceUrl)
		{
			return await Task.Run(() => new ImportAPI(webServiceUrl.AbsoluteUri)).ConfigureAwait(false);
		}
	}
}