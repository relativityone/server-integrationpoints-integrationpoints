namespace Relativity.Sync.Dashboards.Api
{
	public class SplunkKVCollectionItem
	{
		public string Jira { get; set; }
		public string Summary { get; set; }
		public string Status { get; set; }
		public string IssueType { get; set; }
		public string[] Labels { get; set; }
		public string SearchMatch { get; set; }
	}
}