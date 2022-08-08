using System;
using Relativity.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.Core.Services.Domain
{
    public interface IProviderFactoryLifecycleStrategy
    {
        IProviderFactory CreateProviderFactory(Guid applicationId);
        void OnReleaseProviderFactory(Guid applicationId);
    }
}