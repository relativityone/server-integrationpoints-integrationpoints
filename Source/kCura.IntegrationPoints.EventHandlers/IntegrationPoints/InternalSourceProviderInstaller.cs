using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;
using System;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using Relativity.IntegrationPoints.SourceProviderInstaller;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    public abstract class InternalSourceProviderInstaller : IntegrationPointSourceProviderInstaller
    {
        private readonly IRipProviderInstaller _ripProviderInstaller;
        private readonly IKubernetesMode _kubernetesMode;
        private readonly Lazy<IAPILog> _logggerLazy;
        private readonly IToggleProvider _toggleProvider;

        private IAPILog Logger => _logggerLazy.Value;

        protected InternalSourceProviderInstaller()
        {
            _logggerLazy = new Lazy<IAPILog>(
                () => Helper.GetLoggerFactory().GetLogger().ForContext<InternalSourceProviderInstaller>()
            );
            _toggleProvider = ToggleProvider.Current;
        }

        protected InternalSourceProviderInstaller(IRipProviderInstaller ripProviderInstaller, IKubernetesMode kubernetesMode)
            : this()
        {
            _ripProviderInstaller = ripProviderInstaller;
            _kubernetesMode = kubernetesMode;
        }

        internal override ISourceProviderInstaller CreateSourceProviderInstaller()
        {
            return new InProcessSourceProviderInstaller(Logger, Helper, _kubernetesMode, _toggleProvider, _ripProviderInstaller);
        }
    }
}
