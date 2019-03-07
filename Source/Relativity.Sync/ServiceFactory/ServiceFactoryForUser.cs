using System;
using Relativity.API;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Authentication;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ServiceFactory
{
	internal sealed class ServiceFactoryForUser : ISourceServiceFactoryForUser, IDestinationServiceFactoryForUser
	{
		private readonly IUserContextConfiguration _userContextConfiguration;
		private readonly IServicesMgr _servicesMgr;
		private readonly IAuthTokenGenerator _tokenGenerator;

		public ServiceFactoryForUser(IUserContextConfiguration userContextConfiguration, IServicesMgr servicesMgr, IAuthTokenGenerator tokenGenerator)
		{
			_userContextConfiguration = userContextConfiguration;
			_servicesMgr = servicesMgr;
			_tokenGenerator = tokenGenerator;
		}

		public T CreateProxy<T>() where T : IDisposable
		{
			string authToken = _tokenGenerator.GetAuthToken(_userContextConfiguration.ExecutingUserId);
			Credentials credentials = new BearerTokenCredentials(authToken);
			ServiceFactorySettings settings = new ServiceFactorySettings(_servicesMgr.GetServicesURL(), _servicesMgr.GetRESTServiceUrl(), credentials);
			Services.ServiceProxy.ServiceFactory serviceFactory = new Services.ServiceProxy.ServiceFactory(settings);
			return serviceFactory.CreateProxy<T>();
		}
	}
}