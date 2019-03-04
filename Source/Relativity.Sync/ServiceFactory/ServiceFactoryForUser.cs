using System;

namespace Relativity.Sync.ServiceFactory
{
	internal sealed class ServiceFactoryForUser : ISourceServiceFactoryForUser, IDestinationServiceFactoryForUser
	{
		private readonly IServicesMgr _servicesMgr;

		public ServiceFactoryForUser(IServicesMgr servicesMgr)
		{
			_servicesMgr = servicesMgr;
		}

		public T CreateProxy<T>() where T : IDisposable
		{
			return _servicesMgr.CreateProxy<T>(ExecutionIdentity.CurrentUser);
		}
	}
}