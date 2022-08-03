using System.Collections.Generic;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
    public class CompositeLoggingMediator : ICompositeLoggingMediator
    {
        public CompositeLoggingMediator()
        {
            LoggingMediators = new List<ILoggingMediator>();
        }

        public void RegisterEventHandlers(IUserMessageNotification userMessageNotification,
            ICoreExporterStatusNotification exporterStatusNotification)
        {
            foreach (var loggingMediator in LoggingMediators)
            {
                loggingMediator.RegisterEventHandlers(userMessageNotification, exporterStatusNotification);
            }
        }

        public void AddLoggingMediator(ILoggingMediator loggingMediator)
        {
            LoggingMediators.Add(loggingMediator);
        }

        public List<ILoggingMediator> LoggingMediators { get; }
    }
}