namespace Relativity.Sync.Dashboards
{
    public class SyncIssueDTO
    {
        /// <summary>
        /// Jira key, e.g. REL-12345
        /// </summary>
        public string Jira { get; set; }

        /// <summary>
        /// Search term for Splunk. May include wildcard placeholders.
        /// </summary>
        public string Exception { get; set; }
    }
}