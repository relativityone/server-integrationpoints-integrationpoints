using System.Collections.Generic;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
    public interface ICompositeLoggingMediator : ILoggingMediator
    {
        List<ILoggingMediator> LoggingMediators { get; }
        void AddLoggingMediator(ILoggingMediator loggingMediator);
    }
}
