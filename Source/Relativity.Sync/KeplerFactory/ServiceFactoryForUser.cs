using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Authentication;
using Relativity.Sync.Configuration;
using Relativity.Sync.Utils;

namespace Relativity.Sync.KeplerFactory
{
	internal sealed class ServiceFactoryForUser : ServiceFactoryBase, ISourceServiceFactoryForUser, IDestinationServiceFactoryForUser
	{
		private IServiceFactory _serviceFactory;

		private readonly IUserContextConfiguration _userContextConfiguration;
		private readonly ISyncServiceManager _servicesMgr;
		private readonly IAuthTokenGenerator _tokenGenerator;
		private readonly IDynamicProxyFactory _dynamicProxyFactory;
		private readonly IServiceFactoryFactory _serviceFactoryFactory;


		internal ServiceFactoryForUser(IUserContextConfiguration userContextConfiguration, ISyncServiceManager servicesMgr, IAuthTokenGenerator tokenGenerator, IDynamicProxyFactory dynamicProxyFactory,
			IServiceFactoryFactory serviceFactoryFactory, IRandom random, ISyncLog logger)
		    : base (random, logger)
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
		internal ServiceFactoryForUser(IServiceFactory serviceFactory, IDynamicProxyFactory dynamicProxyFactory,
            IRandom random, ISyncLog logger)
			: base(random, logger)
		{
			_serviceFactory = serviceFactory;
			_dynamicProxyFactory = dynamicProxyFactory;
		}

        protected override async Task<T> CreateProxyInternalAsync<T>()
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