using System;
using System.Threading.Tasks;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.System.Core.Stubs
{
	public class SourceServiceFactoryStub : ISourceServiceFactoryForAdmin, ISourceServiceFactoryForUser
	{
		public Uri GetServicesURL()
		{
			return AppSettings.RsapiServicesUrl;
		}

		public Uri GetRESTServiceUrl()
		{
			return AppSettings.RelativityRestUrl;
		}

		public Task<T> CreateProxyAsync<T>() where T : class, IDisposable
		{
			var userCredential = new UsernamePasswordCredentials(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword);
			var userSettings = new ServiceFactorySettings(AppSettings.RelativityRestUrl, userCredential);
			var userServiceFactory = new ServiceFactory(userSettings);
			return Task.FromResult(userServiceFactory.CreateProxy<T>());
		}
	}
}