using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace Relativity.Sync.Dashboards.Api
{
	[Headers("Content-Type: application/json")]
	public interface ISplunkApi
	{
		[Delete("/servicesNS/nobody/search/storage/collections/data/{collectionName}?output_mode=json")]
		Task ClearCollectionAsync(string collectionName);

		[Post("/servicesNS/nobody/search/storage/collections/data/{collectionName}?output_mode=json")]
		Task UpdateLookupTableAsync(string collectionName, [Body] string data);
	}
}