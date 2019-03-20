using Relativity.Services.ServiceProxy;

namespace Relativity.Sync.Tests.System.Stubs
{
	internal class ServiceFactoryByBasicCredentials
	{
		private readonly string _userName;
		private readonly string _password;

		public ServiceFactoryByBasicCredentials(string userName, string password)
		{
			_userName = userName;
			_password = password;
		}

		public ServiceFactory CreateServiceFactory()
		{
			Credentials credentials = new UsernamePasswordCredentials(_userName, _password);
			ServiceFactorySettings settings = new ServiceFactorySettings(AppSettings.RelativityServicesUrl, AppSettings.RelativityRestUrl, credentials);
			return new ServiceFactory(settings);
		}
	}
}