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

        public static class AAD
        {
            public const string _APPLICATION_ID = "51074844-cf65-4163-9961-d065744eb625";
            public const string _PASSWORD = "bzE7Q~a6NDA_2CYvJyGai77Wpj~J3BFP1rDH2";
            public const string _DOMAIN = "6784f901-a257-4c13-a4e0-e33bed071824";

            public const string _AZURE_AD_PROVIDER_IDENTIFIER = "FFADC80B-54DA-4773-B356-5396F550B8F6";

            public static class Fields
            {
                public const string _ID = "ID";
                public const string _FIRST_NAME = "First Name";
                public const string _FIRST_NAME_ID = "givenName";
                public const string _LAST_NAME = "Last Name";
                public const string _LAST_NAME_ID = "surname";
                public const string _MANAGER = "Manager";
                public const string _MANAGER_ID = "manager";

                public const string _TEXT_FIELD_TYPE = "String";
            }
        }
    }
}
