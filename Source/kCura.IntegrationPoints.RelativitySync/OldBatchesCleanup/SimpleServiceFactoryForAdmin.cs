using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.KeplerFactory;

namespace kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup
{
	internal class SimpleServiceFactoryForAdmin : ISourceServiceFactoryForAdmin
	{
		private readonly IServicesMgr _servicesMgr;

		public SimpleServiceFactoryForAdmin(IServicesMgr servicesMgr)
		{
			_servicesMgr = servicesMgr;
		}

		public async Task<T> CreateProxyAsync<T>() where T : class, IDisposable
		{
			Task<T> KeplerServiceFactory() => Task.FromResult(_servicesMgr.CreateProxy<T>(ExecutionIdentity.System));
			T keplerService = await KeplerServiceFactory().ConfigureAwait(false);
			return keplerService;
		}
	}
}