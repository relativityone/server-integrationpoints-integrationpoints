using System;

namespace IntegrationPointsUITests.Config
{
    public abstract class TestConfig
    {
        private const string _PL3_SERVER = "https://il1ddmlpl3wb001.kcura.corp/Relativity/";
        private const string _RELATIVITY_ADMIN_USER = "relativity.admin@kcura.com";
        private const string _RELATIVITY_ADMIN_PASSWORD = "Test1234!";

        private static string _serverAddress = _PL3_SERVER;

        public static string ServerAddress
        {
            get
            {
                string envValue = Environment.GetEnvironmentVariable("IntegrationPointsUITests.ServerAddress");
                return string.IsNullOrWhiteSpace(envValue) ? _serverAddress : envValue;
            }
            set { _serverAddress = value; }
        }

        private static string _username = _RELATIVITY_ADMIN_USER;

        public static string Username
        {
            get
            {
                string envValue = Environment.GetEnvironmentVariable("IntegrationPointsUITests.Username");
                return string.IsNullOrWhiteSpace(envValue) ? _username : envValue;
            }
            set { _username = value; }
        }

        private static string _password = _RELATIVITY_ADMIN_PASSWORD;

        public static string Password
        {
            get {
                string envValue = Environment.GetEnvironmentVariable("IntegrationPointsUITests.Password");
                return string.IsNullOrWhiteSpace(envValue) ? _password : envValue;
            }
            set { _password = value; }
        }
        

        private TestConfig()
        {

        }
        
    }
}
