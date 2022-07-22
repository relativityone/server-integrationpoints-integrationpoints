namespace Relativity.Sync.Dashboards.Api
{
    public class JiraTicketInfo
    {
        public Fields Fields { get; set; } = new Fields();
    }

    public class Fields
    {
        public string Summary { get; set; }
        public string[] Labels { get; set; }
        public IssueType IssueType { get; set; } = new IssueType();
        public Status Status { get; set; } = new Status();
        public FixVersion[] FixVersions { get; set; }
    }

    public class IssueType
    {
        public string Name { get; set; }
    }

    public class Status
    {
        public string Name { get; set; }
    }

    public class FixVersion
    {
        public string Name { get; set; }
    }
}