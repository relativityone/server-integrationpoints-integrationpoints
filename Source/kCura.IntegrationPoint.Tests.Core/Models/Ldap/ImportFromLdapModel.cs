using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.Domain.Extensions;

namespace kCura.IntegrationPoint.Tests.Core.Models.Ldap
{
	public class ImportFromLdapModel
	{
		public IntegrationPointGeneralModel General { get; set; }

		public ImportFromLdapSourceConnectionModel Source { get; set; }

		public ImportSettingsModel SharedSettings { get; set; }

		public ImportCustodianSettingsModel ImportCustodianSettingsModel { get; set; }
	}
}
