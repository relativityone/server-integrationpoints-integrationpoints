﻿using kCura.IntegrationPoint.Tests.Core.Models.Shared;

namespace kCura.IntegrationPoint.Tests.Core.Models.Ldap
{
	public class ImportFromLdapModel
	{
		public IntegrationPointGeneralModel General { get; set; } 

		public ImportFromLdapSourceConnectionModel Source { get; set; }

		public ImportSettingsModel SharedImportSettings { get; set; }

		public ImportCustodianSettingsModel ImportCustodianSettingsModel { get; set; }

		public ImportFromLdapModel(string name, string transferredObject)
		{
			General = new IntegrationPointGeneralModel(name)
			{
				Type = IntegrationPointGeneralModel.IntegrationPointTypeEnum.Import,
				SourceProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_SOURCE_PROVIDER_LDAP,
				TransferredObject = transferredObject
			};
			Source = new ImportFromLdapSourceConnectionModel();
			SharedImportSettings = new ImportSettingsModel();
			ImportCustodianSettingsModel = new ImportCustodianSettingsModel();
		}
	}
}
