﻿using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Authentication;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.KeplerFactory
{
	internal sealed class ServiceFactoryForUser : ISourceServiceFactoryForUser, IDestinationServiceFactoryForUser
	{
		private IServiceFactory _serviceFactory;

		private readonly IUserContextConfiguration _userContextConfiguration;
		private readonly IServicesMgr _servicesMgr;
		private readonly IAuthTokenGenerator _tokenGenerator;
		private readonly IDynamicProxyFactory _dynamicProxyFactory;
		private readonly IServiceFactoryFactory _serviceFactoryFactory;

		public ServiceFactoryForUser(IUserContextConfiguration userContextConfiguration, IServicesMgr servicesMgr, IAuthTokenGenerator tokenGenerator, IDynamicProxyFactory dynamicProxyFactory,
			IServiceFactoryFactory serviceFactoryFactory)
		{
			_userContextConfiguration = userContextConfiguration;
			_servicesMgr = servicesMgr;
			_tokenGenerator = tokenGenerator;
			_dynamicProxyFactory = dynamicProxyFactory;
			_serviceFactoryFactory = serviceFactoryFactory;
		}

		/// <summary>
		///     For testing purposes
		/// </summary>
		internal ServiceFactoryForUser(IServiceFactory serviceFactory, IDynamicProxyFactory dynamicProxyFactory)
		{
			_serviceFactory = serviceFactory;
			_dynamicProxyFactory = dynamicProxyFactory;
		}

		public async Task<T> CreateProxyAsync<T>() where T : IDisposable
		{
			if (_serviceFactory == null)
			{
				_serviceFactory = await CreateServiceFactoryAsync().ConfigureAwait(false);
			}

			T keplerService = _serviceFactory.CreateProxy<T>();
			return keplerService;
			//return _dynamicProxyFactory.WrapKeplerService(keplerService);
		}

		private async Task<IServiceFactory> CreateServiceFactoryAsync()
		{
			string authToken = await _tokenGenerator.GetAuthTokenAsync(_userContextConfiguration.ExecutingUserId).ConfigureAwait(false);
			Credentials credentials = new BearerTokenCredentials(authToken);
			ServiceFactorySettings settings = new ServiceFactorySettings(_servicesMgr.GetServicesURL(), _servicesMgr.GetRESTServiceUrl(), credentials);
			return _serviceFactoryFactory.Create(settings);
		}
	}
}