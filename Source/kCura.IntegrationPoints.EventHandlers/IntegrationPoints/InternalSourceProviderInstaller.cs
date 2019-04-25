using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.IntegrationPoints.SourceProviderInstaller;
using kCura.IntegrationPoints.SourceProviderInstaller.Internals;
using Relativity.API;
using System;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    public abstract class InternalSourceProviderInstaller : IntegrationPointSourceProviderInstaller
    {
        private readonly IRipProviderInstaller _ripProviderInstaller;
        private readonly Lazy<IAPILog> _logggerLazy;

        private IAPILog Logger => _logggerLazy.Value;

        protected InternalSourceProviderInstaller()
        {
            _logggerLazy = new Lazy<IAPILog>(
                () => Helper.GetLoggerFactory().GetLogger().ForContext<InternalSourceProviderInstaller>()
            );
        }

        protected InternalSourceProviderInstaller(IRipProviderInstaller ripProviderInstaller) : this()
        {
            _ripProviderInstaller = ripProviderInstaller;
        }

        internal override ISourceProviderInstaller CreateSourceProviderInstaller()
        {
            return new InProcessSourceProviderInstaller(Logger, Helper, _ripProviderInstaller);
        }
    }
}
