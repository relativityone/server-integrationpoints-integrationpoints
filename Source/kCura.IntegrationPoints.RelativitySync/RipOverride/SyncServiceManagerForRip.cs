using System;
using Relativity.Sync;
using Relativity.API;

namespace kCura.IntegrationPoints.RelativitySync.RipOverride
{
	public class SyncServiceManagerForRip : ISyncServiceManager
	{
		private readonly IServicesMgr _servicesMgr;

		public SyncServiceManagerForRip(IServicesMgr servicesMgr)
		{
			_servicesMgr = servicesMgr;
		}

		public Uri GetServicesURL()
		{
			return _servicesMgr.GetServicesURL();
		}

		public T CreateProxy<T>(ExecutionIdentity ident) where T : class, IDisposable
		{
			return _servicesMgr.CreateProxy<T>(ident);
		}

		public Uri GetRESTServiceUrl()
		{
			return _servicesMgr.GetRESTServiceUrl();
		}
	}
}
