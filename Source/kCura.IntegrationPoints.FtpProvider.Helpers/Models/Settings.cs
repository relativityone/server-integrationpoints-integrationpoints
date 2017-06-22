using kCura.IntegrationPoints.Contracts.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace kCura.IntegrationPoints.FtpProvider.Helpers.Models
{
    public class Settings
    {
        internal String _host;
        internal String _protocol;
        internal String _username;
        internal String _password;
        internal String _filename;
        internal String _validationMessage;

        [DisplayName("Host:")]
        public String Host
        {
            get { return (_host ?? String.Empty); }
            set { _host = value; }
        }

        [DisplayName("Protocol:")]
        public String Protocol
        {
            get { return (_protocol ?? String.Empty); }
            set { _protocol = value; }
        }

        [DisplayName("Port:")]
        [DefaultValue(21)]
        public Int32 Port { get; set; }

        [DisplayName("Username:")]
        public String Username
        {
            get { return (_username ?? String.Empty); }
            set { _username = value; }
        }

        [DisplayName("Password:")]
        public String Password
        {
            get { return (_password ?? String.Empty); }
            set { _password = value; }
        }

        [DisplayName("CSV Filepath:")]
        public String Filename_Prefix
        {
            get { return (_filename ?? String.Empty); }
            set { _filename = value; }
        }

        public String ValidationMessage
        {
            get { return (_validationMessage ?? String.Empty); }
            set { _validationMessage = value; }
        }

        [DefaultValue(0)]
        public Int32? Timezone_Offset { get; set; }

        public List<FieldEntry> ColumnList { get; set; }


        /// <summary>
        /// Validates that the host value is correctly formatted
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public Boolean ValidateHost()
        {
            var valid = false;

            if (Uri.CheckHostName(Host) != UriHostNameType.Unknown)
            {
                valid = true;
            }

            return valid;
        }

        public Boolean ValidatePort()
        {
            var valid = true;

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

            return valid;
        }
        public Boolean ValidateCSVName()
        {
            return !string.IsNullOrWhiteSpace(this._filename);
        }
    }
}