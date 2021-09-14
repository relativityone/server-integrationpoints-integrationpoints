using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;
using System;
using Relativity.IntegrationPoints.SourceProviderInstaller;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    public abstract class InternalSourceProviderInstaller : IntegrationPointSourceProviderInstaller
    {
        private readonly IRipProviderInstaller _ripProviderInstaller;
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

        protected InternalSourceProviderInstaller(IRipProviderInstaller ripProviderInstaller)
	        : this()
        {
            _ripProviderInstaller = ripProviderInstaller;
        }

        internal override ISourceProviderInstaller CreateSourceProviderInstaller()
        {
            return new InProcessSourceProviderInstaller(Logger, Helper, _toggleProvider, _ripProviderInstaller );
        }
    }
}
