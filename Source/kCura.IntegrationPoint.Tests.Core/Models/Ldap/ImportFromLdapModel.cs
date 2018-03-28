using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Extensions;

namespace kCura.IntegrationPoint.Tests.Core.Models.Ldap
{
	public class ImportFromLdapModel
	{
		public IntegrationPointGeneralModel General { get; set; }

		public ImportFromLdapSourceConnectionModel Source { get; set; }

		public List<Tuple<string, string>> FieldMapping { get; set; }

		public ImportFromLdapSettingsModel Settings { get; set; }
		
	}
}
