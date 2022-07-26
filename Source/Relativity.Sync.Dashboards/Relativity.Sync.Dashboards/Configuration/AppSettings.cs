namespace Relativity.Sync.Dashboards.Configuration
{
    public class AppSettings
    {
        public string JiraURL { get; set; } = "https://jira.kcura.com";
        public string JiraUserName { get; set; }
        public string JiraUserPassword { get; set; }

        public string SplunkURL { get; set; } = "https://relativity.splunkcloud.com:8089";
        public string SplunkUser { get; set; }
        public string SplunkPassword { get; set; }
        public string SplunkKVCollectionName { get; set; }
    }
}