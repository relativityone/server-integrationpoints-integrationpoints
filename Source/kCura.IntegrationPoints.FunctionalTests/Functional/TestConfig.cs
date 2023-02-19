using System.Configuration;
using System.IO;
using NUnit.Framework;

namespace Relativity.IntegrationPoints.Tests.Functional
{
    public static class TestConfig
    {
        public static string SqlServer => GetConfigValue("SqlServer");

        public static string SqlUsername => GetConfigValue("SqlUsername");

        public static string SqlPassword => GetConfigValue("SqlPassword");

        public static string ConnectionStringEDDS => string.Format("Data Source={0};Initial Catalog=EDDS", SqlServer);

        public static string ConnectionStringWorkspace(int workspaceID) => string.Format("Data Source={0};Initial Catalog=EDDS{1}", SqlServer, workspaceID);

        public static bool DocumentImportEnforceWebMode => bool.Parse(GetConfigValue("DocumentImportEnforceWebMode"));

        public static int DocumentImportTimeout => int.Parse(GetConfigValue("DocumentImportTimeout"));

        public static string ARMTestServicesRapFileLocation => Path.Combine(GetConfigValue("BuildToolsDirectory"), "ARMTestServices.RAP\\lib\\ARMTestServices.rap");

        public static string DataTransferLegacyRapFileLocation => Path.Combine(GetConfigValue("BuildToolsDirectory"), "DataTransfer.Legacy\\lib\\DataTransfer.Legacy.rap");

        public static string AzureADProviderRapFileLocation => Path.Combine(GetConfigValue("BuildToolsDirectory"), "AADProvider\\lib\\AADProvider.rap");

        public static int ExistingWorkspaceArtifactId => int.Parse(GetConfigValue("ExistingWorkspaceArtifactId"));

        public static string LogsDirectoryPath => Path.Combine(TestContext.CurrentContext.WorkDirectory, "Artifacts", "Logs");
        private static string GetConfigValue(string name) => TestContext.Parameters.Exists(name)
            ? TestContext.Parameters[name]
            : ConfigurationManager.AppSettings.Get(name);
    }
}
