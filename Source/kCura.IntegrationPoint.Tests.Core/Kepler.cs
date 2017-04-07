using System;
using System.Net.Http;
using System.Reflection;
using Relativity.Services.ServiceProxy;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Kepler
	{
		static Kepler()
		{
			var defaultHttpClientTimeoutField = typeof(HttpClient).GetField("defaultTimeout", BindingFlags.Static | BindingFlags.NonPublic);
			defaultHttpClientTimeoutField?.SetValue(null, TimeSpan.FromSeconds(SharedVariables.KeplerTimeout));
		}

		public static T CreateProxy<T>(string username, string password, bool isApiService) where T : class, IDisposable
		{
			Uri serviceUri = GetServiceUrl(isApiService);
			Uri restUri = Rest.GetRestUrl(isApiService);

			Credentials serviceCredentials = new UsernamePasswordCredentials(username, password);
			ServiceFactorySettings serviceFactorySettings = new ServiceFactorySettings(serviceUri, restUri, serviceCredentials);
			ServiceFactory serviceFactory = new ServiceFactory(serviceFactorySettings);
			T proxy = serviceFactory.CreateProxy<T>();

			return proxy;
		}

		private static Uri GetServiceUrl(bool isApiService)
		{
			string apiSegment = (isApiService) ? "api" : string.Empty;
			string url = $"{SharedVariables.RsapiClientUri}/{apiSegment}";

			Uri serviceUri = new Uri(url);
			return serviceUri;
		}
	}
}