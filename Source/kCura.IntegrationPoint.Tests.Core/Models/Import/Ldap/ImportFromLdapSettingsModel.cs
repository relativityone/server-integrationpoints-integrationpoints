using kCura.IntegrationPoint.Tests.Core.Models.Shared;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import.Ldap
{
	public class ImportFromLdapSettingsModel
	{
		public OverwriteType Overwrite { get; set; }

		public string UniqueIdentifier { get; set; }

		public bool CustodianManagerContainsLink { get; set; } = true;

	}
}