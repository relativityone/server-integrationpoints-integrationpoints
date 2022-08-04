using System;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
    public class UserMessageEventArgs : EventArgs
    {
        public UserMessageEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
    }
}