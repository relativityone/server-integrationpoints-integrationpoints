using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
    internal class TestLoggingMediator : ILoggingMediator
    {
        public void RegisterEventHandlers(IUserMessageNotification userMessageNotification,
            ICoreExporterStatusNotification exporterStatusNotification)
        {
            exporterStatusNotification.FatalErrorEvent += OnFatalErrorEvent;
        }

        private void OnFatalErrorEvent(string message, Exception ex)
        {
            Console.WriteLine(message);
            Console.WriteLine(ex.ToString());
        }
    }
}
