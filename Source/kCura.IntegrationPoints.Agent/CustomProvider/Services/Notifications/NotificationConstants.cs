using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.Notifications
{
    internal class NotificationConstants
    {
        public const string _SUBJECT_CONTENT = "Relativity Job '{0}' - {1}";
        public const string _MESSAGE_CONTENT = "Relativity Integration Points Job Report";
        public const string _ERROR_DEFAULT_MSG = "Please check the Workspace Job History Error tab for more details.";

        public const string _BODY_NAME = "Integration Point Name: ";
        public const string _BODY_DESTINATION = "Destination Workspace Name: ";
        public const string _BODY_STATUS = "Status: ";
        public const string _BODY_ERROR = "Error: ";
        public const string _BODY_STATISTICS = "Job summary: ";

    }
}
