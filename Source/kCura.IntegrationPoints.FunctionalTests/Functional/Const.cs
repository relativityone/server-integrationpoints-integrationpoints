namespace Relativity.IntegrationPoints.Tests.Functional
{
	internal static class Const
	{
		public const string INTEGRATION_POINTS_APPLICATION_NAME = "Integration Points";

		public const string INTEGRATION_POINTS_AGENT_TYPE_NAME = "Integration Points Agent";
		public const int INTEGRATION_POINTS_AGENT_RUN_INTERVAL = 5;

		public static class LDAP
		{
			public const string OPEN_LDAP_USERNAME = "cn=admin,dc=rip-openldap-cvnx78s,dc=eastus,dc=azurecontainer,dc=io";
			public const string OPEN_LDAP_PASSWORD = "Test1234!";
			public const string OPEN_LDAP_CONNECTION_PATH = "rip-openldap-cvnx78s.eastus.azurecontainer.io/ou=Human Resources,dc=rip-openldap-cvnx78s,dc=eastus,dc=azurecontainer,dc=io";
		}
	}

}
