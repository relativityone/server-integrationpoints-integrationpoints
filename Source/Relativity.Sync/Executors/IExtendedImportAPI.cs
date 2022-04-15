using kCura.Relativity.ImportAPI;
using Relativity.DataExchange;

namespace Relativity.Sync.Executors
{
	public interface IExtendedImportAPI
	{
		ImportAPI CreateByTokenProvider(string webServiceUrl, IRelativityTokenProvider relativityTokenProvider);
	}
}