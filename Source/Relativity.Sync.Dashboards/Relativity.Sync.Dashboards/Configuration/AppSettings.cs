namespace Relativity.Sync.Dashboards.Configuration
{
	public class AppSettings
	{
		public string JiraURL { get; set; }
		public string JiraUserName { get; set; }
		public string JiraUserPassword { get; set; }

		public string SplunkURL { get; set; }
		public string SplunkAccessToken { get; set; }
		public string SplunkKVCollectionName { get; set; }
	}
}