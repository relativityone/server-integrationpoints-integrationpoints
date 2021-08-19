namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models
{
    internal class ImportFromLDAPConnectToSource
    {
        public string ConnectionPath { get; set; }

        public IntegrationPointAuthentication Authentication { get; } = IntegrationPointAuthentication.FastBind;

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
