using System;
using Relativity.API;
using Relativity.Services.ServiceProxy;

namespace Relativity.Sync.Tests.System.Stubs
{
	public class ServicesManagerStub : IServicesMgr
	{
#pragma warning disable CA1024 // Use properties where appropriate
		public Uri GetServicesURL()
		{
			return AppSettings.RelativityServicesUrl;
		}

		public Uri GetRESTServiceUrl()
		{
			return AppSettings.RelativityRestUrl;
		}

		public T CreateProxy<T>(ExecutionIdentity ident) where T : IDisposable
		{
			var userCredential = new UsernamePasswordCredentials(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword);
			var userSettings = new ServiceFactorySettings(AppSettings.RelativityServicesUrl, AppSettings.RelativityRestUrl, userCredential);
			var userServiceFactory = new ServiceFactory(userSettings);
			return userServiceFactory.CreateProxy<T>();
		}
	}
#pragma warning restore CA1024 // Use properties where appropriate

}