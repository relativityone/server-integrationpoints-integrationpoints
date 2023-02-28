namespace kCura.IntegrationPoint.Tests.Core.Models
{
    using System.ComponentModel;

    public class IntegrationPointGeneralModel
    {
        public const string INTEGRATION_POINT_SOURCE_PROVIDER_FTP = "FTP (CSV File)";
        public const string INTEGRATION_POINT_SOURCE_PROVIDER_LDAP = "LDAP";
        public const string INTEGRATION_POINT_PROVIDER_LOADFILE = "Load File";
        public const string INTEGRATION_POINT_SOURCE_PROVIDER_RELATIVITY = "Relativity";
        public const string INTEGRATION_POINT_SOURCE_PROVIDER_O365 = "Office 365";
        public const string INTEGRATION_POINT_SOURCE_PROVIDER_JSON = "JSON";
        public const string INTEGRATION_POINT_SOURCE_PROVIDER_MY_FIRST_PROVIDER = "My First Provider";
        public const string INTEGRATION_POINT_DESTINATION_PROVIDER_RELATIVITY = "Relativity";

        public string Name { get; set; }

        [DefaultValue(IntegrationPointType.Export)]
        public IntegrationPointType Type { get; set; }

        [DefaultValue(INTEGRATION_POINT_SOURCE_PROVIDER_RELATIVITY)]
        public string SourceProvider { get; set; }

        [DefaultValue(INTEGRATION_POINT_DESTINATION_PROVIDER_RELATIVITY)]
        public string DestinationProvider { get; set; }

        [DefaultValue("Document")]
        public string TransferredObject { get; set; }

        public string Profile { get; set; }

        [DefaultValue("")]
        public string EmailNotifications { get; set; }

        [DefaultValue(true)]
        public bool? LogErrors { get; set; }

        public SchedulerModel Scheduler { get; set; } = new SchedulerModel(enable: false);

        public IntegrationPointGeneralModel(string name)
        {
            Name = name;
        }
    }
}
