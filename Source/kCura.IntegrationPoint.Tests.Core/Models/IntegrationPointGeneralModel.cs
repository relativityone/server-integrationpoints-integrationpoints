namespace kCura.IntegrationPoint.Tests.Core.Models
{
	public class IntegrationPointGeneralModel
	{
		public const string INTEGRATION_POINT_SOURCE_PROVIDER_FTP = "FTP (CSV File)";
		public const string INTEGRATION_POINT_SOURCE_PROVIDER_LDAP = "LDAP";

		public const string INTEGRATION_POINT_PROVIDER_LOADFILE = "Load File";

		public const string INTEGRATION_POINT_DESTINATION_PROVIDER_RELATIVITY = "Relativity";

		public string Name { get; }
		public IntegrationPointTypeEnum Type { get; set; }
		public string SourceProvider { get; set; }
		public string DestinationProvider { get; set; }
		public string Profile { get; set; }
		public bool? IncludeInEcaPromote { get; set; }

		public enum IntegrationPointTypeEnum
		{
			Import,
			Export
		}

		public IntegrationPointGeneralModel(string ipName)
		{
			Name = ipName;
			Type = IntegrationPointTypeEnum.Export;
			DestinationProvider = INTEGRATION_POINT_DESTINATION_PROVIDER_RELATIVITY;
			Profile = string.Empty;
			IncludeInEcaPromote = false;
		}
	}
}
