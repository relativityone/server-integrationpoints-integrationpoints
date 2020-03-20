using System;
using Relativity.API;
using Relativity.Services.ServiceProxy;

namespace Relativity.Sync.Tests.System.Core.Stubs
{
	public class ServicesManagerStub : ISyncServiceManager
	{
		public Uri GetServicesURL()
		{
			return AppSettings.RelativityServicesUrl;
		}

		public Uri GetRESTServiceUrl()
		{
			return AppSettings.RelativityRestUrl;
		}

		public T CreateProxy<T>(ExecutionIdentity ident) where T : class, IDisposable
		{
			var userCredential = new UsernamePasswordCredentials(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword);
			var userSettings = new ServiceFactorySettings(AppSettings.RelativityServicesUrl, AppSettings.RelativityRestUrl, userCredential);
			var userServiceFactory = new ServiceFactory(userSettings);
			return userServiceFactory.CreateProxy<T>();
		}
	}
}