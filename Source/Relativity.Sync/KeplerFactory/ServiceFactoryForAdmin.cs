using System;
using System.Threading.Tasks;
using Relativity.API;

namespace Relativity.Sync.KeplerFactory
{
	internal sealed class ServiceFactoryForAdmin : ISourceServiceFactoryForAdmin, IDestinationServiceFactoryForAdmin
	{
		private readonly IServicesMgr _servicesMgr;

		public ServiceFactoryForAdmin(IServicesMgr servicesMgr)
		{
			_servicesMgr = servicesMgr;
		}

		public async Task<T> CreateProxyAsync<T>() where T : IDisposable
		{
			await Task.Yield();
			return _servicesMgr.CreateProxy<T>(ExecutionIdentity.System);
		}
	}
}