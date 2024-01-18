using System;

namespace Relativity.IntegrationPoints.Services
{
    public class JobHistoryModel
    {
        public int ItemsTransferred { get; set; }

        public DateTime EndTimeUTC { get; set; }

        public string DestinationWorkspace { get; set; }

        public string DestinationInstance { get; set; }

        public string FilesSize { get; set; }

        public string Overwrite { get; set; }
    }
}
