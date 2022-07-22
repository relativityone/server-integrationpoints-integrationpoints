using System;
using Relativity.OAuth2Client.Interfaces;

namespace Relativity.Sync.Authentication
{
    internal interface ITokenProviderFactoryFactory
    {
        ITokenProviderFactory Create(Uri secureTokenServiceUrl, string clientId, string clientSecret);
    }
}