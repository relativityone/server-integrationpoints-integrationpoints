using System;


namespace kCura.IntegrationPoint.Tests.Core.Models.Shared
{
	public class ImportCustodianSettingsModel
	{
		public string UniqueIdentifier { get; set; }

		public bool CustodianManagerContainsLink { get; set; } = true;
	}
}
