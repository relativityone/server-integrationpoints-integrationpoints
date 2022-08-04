using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.FtpProvider.Helpers
{
    public class Constants
    {
        public const Char WildCard = '*';
        public const Int32 RetyCount = 2;
        public const Int32 Timeout = 5;
        public const String DefaultPassword = "anonymous@anonymous.com";

        public const String DefaultUsername = "anonymous";
        public const String Delimiter = ",";
        public class Guids
        {
            public static readonly Guid RelativityInterationPoints_Relativity_Application = new Guid("DCF6E9D1-22B6-4DA3-98F6-41381E93C30C");
            public const String FtpProviderEventHandler = "85120BC8-B2B9-4F05-99E9-DE37BB6C0E15";
        }
    }

    public static class ProtocolName
    {
        public const String FTP = "FTP";
        public const String SFTP = "SFTP";
        public static readonly List<string> All = new List<String>() { FTP, SFTP };
    }

    public struct ErrorMessage
    {
        public const String INVALID_HOST_NAME = "Please enter a valid host name. e.g. 172.31.24.97";
        public const string MISSING_CSV_FILE_NAME = "Please enter a CSV file path. e.g. /export/nightlyexport/*yyyy*-*MM*-*dd*_HRIS_export";
    }
}