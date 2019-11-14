using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.KeplerFactory;

namespace kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup
{
	internal class ServiceFactoryForAdmin : ISourceServiceFactoryForAdmin
	{
		private readonly IServicesMgr _servicesMgr;

		public ServiceFactoryForAdmin(IServicesMgr servicesMgr)
		{
			_servicesMgr = servicesMgr;
		}

		public Task<T> CreateProxyAsync<T>() where T : class, IDisposable
		{
			return Task.FromResult(_servicesMgr.CreateProxy<T>(ExecutionIdentity.System));
		}
	}
}