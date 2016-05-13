using System;
using Relativity.Services.ServiceProxy;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Kepler
	{
		public static T CreateProxy<T>(string username, string password, bool isHttp, bool isApiService ) where T : class, IDisposable
		{
			Uri serviceUri = GetServiceUrl(isHttp, isApiService);
			Uri restUri = Rest.GetRestUrl(isHttp, isApiService);

			Credentials serviceCredentials = new UsernamePasswordCredentials(username, password);
			ServiceFactorySettings serviceFactorySettings = new ServiceFactorySettings(serviceUri, restUri, serviceCredentials);
			ServiceFactory serviceFactory = new ServiceFactory(serviceFactorySettings);
			T proxy = serviceFactory.CreateProxy<T>();

			return proxy;
		}

		private static Uri GetServiceUrl(bool isHttp, bool isApiService)
		{
			string serverBinding = (isHttp) ? "http" : "https";
			string apiSegment = (isApiService) ? "api" : string.Empty;
			string url = $"{serverBinding}://{SharedVariables.TargetHost}/relativity.services/{apiSegment}";

			Uri serviceUri = new Uri(url);
			return serviceUri;
		}
	}
}
