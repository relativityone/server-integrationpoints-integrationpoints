namespace Relativity.IntegrationPoints.Tests.Common
{
	public static class GlobalConst
	{
		public const string INTEGRATION_POINTS_APPLICATION_GUID = "DCF6E9D1-22B6-4DA3-98F6-41381E93C30C";

		public static class LDAP
		{
			public const string _OPEN_LDAP_USER = "cn=admin,dc=rip-openldap-cvnx78s,dc=eastus,dc=azurecontainer,dc=io";
			public const string _OPEN_LDAP_PASSWORD = "Test1234!";
			public static string _OPEN_LDAP_CONNECTION_PATH(string ou) => $"rip-openldap-cvnx78s.eastus.azurecontainer.io/{ou},dc=rip-openldap-cvnx78s,dc=eastus,dc=azurecontainer,dc=io";

			public const string _JUMP_CLOUD_USER = "uid=admin,ou=Users,o=609287decb206e4f6ef9beb5,dc=jumpcloud,dc=com";
			public const string _JUMP_CLOUD_PASSWORD = "Test1234!";
			public static string _JUMP_CLOUD_CONNECTION_PATH(int? port) => "ldap.jumpcloud.com" + (port == null ? string.Empty : $":{port}") + "/ou=Users,o=609287decb206e4f6ef9beb5,dc=jumpcloud,dc=com";
		}
	}
}
