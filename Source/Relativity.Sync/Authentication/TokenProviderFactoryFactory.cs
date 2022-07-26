using System;
using Relativity.OAuth2Client.Interfaces;
using Relativity.OAuth2Client.TokenProviders.ProviderFactories;

namespace Relativity.Sync.Authentication
{
    internal sealed class TokenProviderFactoryFactory : ITokenProviderFactoryFactory
    {
        public ITokenProviderFactory Create(Uri secureTokenServiceUrl, string clientId, string clientSecret)
        {
            return new ClientTokenProviderFactory(secureTokenServiceUrl, clientId, clientSecret);
        }
    }
}