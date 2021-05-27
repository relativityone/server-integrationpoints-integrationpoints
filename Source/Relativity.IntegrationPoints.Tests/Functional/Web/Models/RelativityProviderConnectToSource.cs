namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models
{
	internal class RelativityProviderConnectToSource
	{
		public RelativityProviderSources Source { get; set; }

		public string SavedSearch { get; set; }

		public string DestinationWorkspace { get; set; }

		public RelativityProviderDestinationLocations Location { get; set; }
	}
}
