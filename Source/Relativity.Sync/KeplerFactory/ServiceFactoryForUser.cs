using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Authentication;
using Relativity.Sync.Configuration;
using Relativity.Sync.Extensions;

namespace Relativity.Sync.KeplerFactory
{
	internal sealed class ServiceFactoryForUser : ISourceServiceFactoryForUser, IDestinationServiceFactoryForUser
	{
		private IServiceFactory _serviceFactory;

		private readonly IUserContextConfiguration _userContextConfiguration;
		private readonly ISyncServiceManager _servicesMgr;
		private readonly IAuthTokenGenerator _tokenGenerator;
		private readonly IDynamicProxyFactory _dynamicProxyFactory;
		private readonly IServiceFactoryFactory _serviceFactoryFactory;

		public ServiceFactoryForUser(IUserContextConfiguration userContextConfiguration, ISyncServiceManager servicesMgr, IAuthTokenGenerator tokenGenerator, IDynamicProxyFactory dynamicProxyFactory,
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
		[ExcludeFromCodeCoverage]
		internal ServiceFactoryForUser(IServiceFactory serviceFactory, IDynamicProxyFactory dynamicProxyFactory)
		{
			_serviceFactory = serviceFactory;
			_dynamicProxyFactory = dynamicProxyFactory;
		}

		public async Task<T> CreateProxyAsync<T>() where T : class, IDisposable
		{
            T proxy = await this.CreateProxyWithRetriesAsync(null, executionIdentity =>
                GetKeplerServiceWrapperAsync<T>(null))
                .ConfigureAwait(false);
            return proxy;
        }

        private async Task<T> GetKeplerServiceWrapperAsync<T>(ExecutionIdentity? executionIdentity) where T : class, IDisposable
        {
			if (_serviceFactory == null)
            {
                _serviceFactory = await CreateServiceFactoryAsync().ConfigureAwait(false);
            }

            return _dynamicProxyFactory.WrapKeplerService(_serviceFactory.CreateProxy<T>(), async() =>
            {
                _serviceFactory = await CreateServiceFactoryAsync().ConfigureAwait(false);
                return _serviceFactory.CreateProxy<T>();
            });
        }

		private async Task<IServiceFactory> CreateServiceFactoryAsync()
		{
			string authToken = await _tokenGenerator.GetAuthTokenAsync(_userContextConfiguration.ExecutingUserId).ConfigureAwait(false);
			Credentials credentials = new BearerTokenCredentials(authToken);
			ServiceFactorySettings settings = new ServiceFactorySettings(_servicesMgr.GetRESTServiceUrl(), credentials);
			return _serviceFactoryFactory.Create(settings);
		}
	}
}