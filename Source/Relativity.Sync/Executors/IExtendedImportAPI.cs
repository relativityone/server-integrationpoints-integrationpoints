using kCura.Relativity.ImportAPI;
using Relativity.DataExchange;

namespace Relativity.Sync.Executors
{
	internal interface IExtendedImportAPI
	{
		ImportAPI CreateByTokenProvider(string webServiceUrl, IRelativityTokenProvider relativityTokenProvider);
	}
}