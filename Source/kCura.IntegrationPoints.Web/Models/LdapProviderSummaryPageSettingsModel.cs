using kCura.IntegrationPoints.LDAPProvider;

namespace kCura.IntegrationPoints.Web.Models
{
	public class LdapProviderSummaryPageSettingsModel
	{
		public string ConnectionPath { get; set; }
		public string Filter { get; set; }
		public string ConnectionAuthenticationType { get; set; }
		public string UserName { get; set; }
		public string Password => "******";
		public string ImportNested { get; set; }

		public LdapProviderSummaryPageSettingsModel(LDAPSettings settings)
		{
			ConnectionPath = settings.ConnectionPath;
			Filter = settings.Filter;
			ConnectionAuthenticationType = settings.ConnectionAuthenticationType.ToString();
			UserName = settings.UserName;
			ImportNested = settings.ImportNested ? "Yes" : "No";
		}
	}
}