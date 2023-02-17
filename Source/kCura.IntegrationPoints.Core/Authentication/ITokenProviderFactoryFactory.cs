using System;
using Relativity.OAuth2Client.Interfaces;

namespace kCura.IntegrationPoints.Core.Authentication
{
    public interface ITokenProviderFactoryFactory
    {
        ITokenProviderFactory Create(Uri secureTokenServiceUrl, string clientId, string clientSecret);
    }
}
