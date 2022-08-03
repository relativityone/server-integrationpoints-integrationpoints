using System.Collections.Generic;
using System.ComponentModel;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.FtpProvider.Helpers.Models
{
    public class SettingsViewModel
    {
        [DisplayName("Host:")]
        public string Host { get; set; } = string.Empty;

        [DisplayName("Protocol:")]
        public string Protocol { get; set; } = string.Empty;

        [DisplayName("Port:")]
        [DefaultValue(21)]
        public int Port { get; set; }

        [DisplayName("CSV Filepath:")]
        public string Filename_Prefix { get; set; } = string.Empty;

        public string ValidationMessage { get; set; } = string.Empty;

        [DefaultValue(0)]
        public int? Timezone_Offset { get; set; }

        public List<FieldEntry> ColumnList { get; set; }

        [DisplayName("Username:")]
        public string Username { get; set; } = string.Empty;

        [DisplayName("Password:")]
        public string Password { get; set; } = string.Empty;
    }
}