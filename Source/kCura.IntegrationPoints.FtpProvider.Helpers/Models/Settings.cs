using System;
using System.Collections.Generic;
using System.ComponentModel;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.FtpProvider.Helpers.Models
{
    public class Settings
    {
        public string Host { get; set; } = string.Empty;

        public string Protocol { get; set; } = string.Empty;

        [DefaultValue(21)]
        public int Port { get; set; }

        public string Filename_Prefix { get; set; } = string.Empty;

        public string ValidationMessage { get; set; } = string.Empty;

        [DefaultValue(0)]
        public int? Timezone_Offset { get; set; }

        public List<FieldEntry> ColumnList { get; set; }


        /// <summary>
        /// Validates that the host value is correctly formatted
        /// </summary>
        /// <returns></returns>
        public bool ValidateHost()
        {
            return Uri.CheckHostName(Host) != UriHostNameType.Unknown;
        }

        public void UpdatePort()
        {
            if (Port == 0)
            {
                if (Protocol.Equals(ProtocolName.FTP))
                {
                    Port = 21;
                }
                else
                {
                    Port = 22;
                }
            }
        }
        public bool ValidateCSVName()
        {
            return !string.IsNullOrWhiteSpace(this.Filename_Prefix);
        }
    }
}