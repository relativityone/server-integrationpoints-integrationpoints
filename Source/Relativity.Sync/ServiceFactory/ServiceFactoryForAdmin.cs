using System;

namespace Relativity.Sync.ServiceFactory
{
	internal sealed class ServiceFactoryForAdmin : ISourceServiceFactoryForAdmin, IDestinationServiceFactoryForAdmin
	{
		private readonly IServicesMgr _servicesMgr;

		public ServiceFactoryForAdmin(IServicesMgr servicesMgr)
		{
			_servicesMgr = servicesMgr;
		}

		public T CreateProxy<T>() where T : IDisposable
		{
			return _servicesMgr.CreateProxy<T>(ExecutionIdentity.System);
		}
	}
}